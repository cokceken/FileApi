using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileApi.Service;

namespace FileApi.Test.Integration
{
    public class FakeFileRepository : IFileRepository
    {
        public Task<long> GetFileLength(string path, CancellationToken cancellation = default)
        {
            return Task.FromResult(1L);
        }

        public Task<IEnumerable<string>> EnumerateDirectories(string path, CancellationToken cancellation = default)
        {
            IEnumerable<string> result;
            switch (path)
            {
                case "BaseDirectory":
                    result = new List<string>()
                    {
                        "SubFolder1",
                        "SubFolder2",
                        "SubFolder3",
                        "SubFolder4",
                        "Inaccessible"
                    };
                    break;
                case "No-Directory":
                    throw new DirectoryNotFoundException();
                default: 
                    result = new List<string>();
                    break;
            }

            return Task.FromResult(result);
        }

        public Task<IEnumerable<string>> EnumerateFiles(string path, CancellationToken cancellation = default)
        {
            IEnumerable<string> result;
            switch (path)
            {
                case "SubFolder1":
                    result = new List<string>()
                    {
                        "file"
                    };
                    break;
                case "SubFolder2":
                    result = new List<string>()
                    {
                        "file",
                        "file"
                    };
                    break;
                case "SubFolder3":
                    result = new List<string>()
                    {
                        "file",
                        "file",
                        "file"
                    };
                    break;
                case "SubFolder4":
                    result = new List<string>()
                    {
                        "file",
                        "file",
                        "file",
                        "file"
                    };
                    break;
                case "Inaccessible":
                    throw new UnauthorizedAccessException();
                default:
                    result = new List<string>();
                    break;
            }

            return Task.FromResult(result);
        }
    }
}