using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FileApi.Service
{
    public interface IFileService
    {
        Task<IEnumerable<string>> GetBigFolders(string path, int count, bool suppressAccessErrors,
            CancellationToken cancellation);
    }

    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<FileService> _logger;

        public FileService(IFileRepository fileRepository, ILogger<FileService> logger)
        {
            _fileRepository = fileRepository;
            _logger = logger;
        }

        //I've compared serial and parallel approaches, serial approach was 30%-300% slower in different kind of folders such as program files folder and a code base folder.
        //I've compared recursive calls and using a stack to iterate folders, there was not much differences in the speed but the function call stack is much higher with the recursive approach.
        //I've introduced async/await statements for the task definition.
        //Normally, while using Directory to access files, we wouldn't need any async operations so parallelism would be easier using Parallel.Foreach directly.
        public async Task<IEnumerable<string>> GetBigFolders(string path, int count, bool suppressAccessErrors,
            CancellationToken cancellation = default)
        {
            var result = new Dictionary<string, long>();
            var minPair = new KeyValuePair<string, long>("key", -1);
            var directories = await _fileRepository.EnumerateDirectories(path, cancellation);

            var parallelLock = new object();

            await Task.WhenAll(from directory in directories select Task.Run(async () =>
            {
                var length = await GetFolderTotalSize(directory, suppressAccessErrors, cancellation);
                lock (parallelLock)
                {
                    if (result.Count < count) result[directory] = length;
                    else
                    {
                        if (minPair.Value > length) return;
                        result.Remove(minPair.Key);
                        result[directory] = length;
                    }

                    minPair = FindMinimumPair(result, cancellation);
                }
            }, cancellation));

            return result.OrderByDescending(x => x.Value).Select(x => x.Key);
        }

        private async Task<long> GetFolderTotalSize(string path, bool suppressAccessErrors,
            CancellationToken cancellation)
        {
            long result = 0;
            var foldersToGo = new Stack<string>();
            foldersToGo.Push(path);

            while (foldersToGo.Any())
            {
                var folder = foldersToGo.Pop();

                var files = await SafeCall(() => _fileRepository.EnumerateFiles(folder, cancellation), suppressAccessErrors, cancellation);
                if (files != null)
                    foreach (var file in files)
                        result += await SafeCall(() => _fileRepository.GetFileLength(file, cancellation), suppressAccessErrors, cancellation);

                var directories = await SafeCall(() => _fileRepository.EnumerateDirectories(folder, cancellation),
                    suppressAccessErrors, cancellation);
                if (directories != null)
                    foreach (var directory in directories)
                        foldersToGo.Push(directory);
            }

            return result;
        }

        private KeyValuePair<string, long> FindMinimumPair(IDictionary<string, long> dictionary,
            CancellationToken cancellation)
        {
            return dictionary.Aggregate((left, right) =>
                left.Value < right.Value ? left : right);
        }

        private async Task<T> SafeCall<T>(Func<Task<T>> func, bool suppressAccessErrors, CancellationToken cancellation)
        {
            try
            {
                cancellation.ThrowIfCancellationRequested();
                return await func();
            }
            catch (PathTooLongException e)
            {
                if (!suppressAccessErrors) throw;
                _logger.LogInformation(e.Message);
                return default;
            }
            catch (SecurityException e)
            {
                if (!suppressAccessErrors) throw;
                _logger.LogInformation(e.Message);
                return default;
            }
            catch (UnauthorizedAccessException e)
            {
                if (!suppressAccessErrors) throw;
                _logger.LogInformation(e.Message);
                return default;
            }
        }
    }
}