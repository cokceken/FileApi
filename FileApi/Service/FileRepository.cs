using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileApi.Service
{
    public interface IFileRepository
    {
        Task<long> GetFileLength(string path, CancellationToken cancellation = default);
        Task<IEnumerable<string>> EnumerateDirectories(string path, CancellationToken cancellation = default);
        Task<IEnumerable<string>> EnumerateFiles(string path, CancellationToken cancellation = default);
    }

    public class FileRepository : IFileRepository
    {
        public Task<long> GetFileLength(string path, CancellationToken cancellation = default)
        {
            return Task.FromResult(new FileInfo(path).Length);
        }

        public Task<IEnumerable<string>> EnumerateDirectories(string path, CancellationToken cancellation = default)
        {
            return Task.FromResult(Directory.EnumerateDirectories(path));
        }

        public Task<IEnumerable<string>> EnumerateFiles(string path, CancellationToken cancellation = default)
        {
            return Task.FromResult(Directory.EnumerateFiles(path));
        }
    }
}