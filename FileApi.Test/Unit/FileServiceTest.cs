using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileApi.Service;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileApi.Test.Unit
{
    public class FileServiceTest : IDisposable
    {
        private readonly FileService _sut;
        private readonly Mock<IFileRepository> _fileRepositoryMock;

        private const string NoDirectory = "NoDirectory";
        private const string DirectoryWithFiles1 = "DirectoryWithFiles1";
        private const string DirectoryWithFiles2 = "DirectoryWithFiles2";
        private const string DirectoryWithFiles3 = "DirectoryWithFiles3";
        private const string DirectoryWithFiles4 = "DirectoryWithFiles4";
        private const string DirectoryWithDirectory = "DirectoryWithDirectory";
        private const string EmptyDirectory = "EmptyDirectory";
        private const string InaccessibleDirectory = "InaccessibleDirectory";
        private const string DirectoryWithInaccessibleFile = "DirectoryWithInaccessibleFile";
        private const string ValidFile = "ValidFile";
        private const string InaccessibleFile = "InaccessibleFile";
        private const long DefaultFileLength = 1;

        public FileServiceTest()
        {
            var loggerMock = new Mock<ILogger<FileService>>();
            _fileRepositoryMock = new Mock<IFileRepository>();

            _fileRepositoryMock.Setup(x => x.EnumerateDirectories(It.Is<string>(s => s.Equals(DirectoryWithDirectory)), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new List<string>()
                {
                    EmptyDirectory,
                    DirectoryWithFiles4,
                    DirectoryWithFiles3,
                    DirectoryWithFiles2,
                    DirectoryWithFiles1
                }.AsEnumerable()));

            _fileRepositoryMock.Setup(x => x.EnumerateDirectories(It.Is<string>(s => s.Equals(EmptyDirectory)), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new List<string>().AsEnumerable()));

            _fileRepositoryMock.Setup(x => x.EnumerateDirectories(It.Is<string>(s => s.Equals(InaccessibleDirectory)), It.IsAny<CancellationToken>()))
                .Throws<UnauthorizedAccessException>();
            
            _fileRepositoryMock.Setup(x => x.EnumerateDirectories(It.Is<string>(s => s.Equals(NoDirectory)), It.IsAny<CancellationToken>()))
                .Throws<DirectoryNotFoundException>();

            _fileRepositoryMock.Setup(x => x.EnumerateFiles(It.Is<string>(s => s.StartsWith(DirectoryWithInaccessibleFile)), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new List<string>()
                {
                    InaccessibleFile
                }.AsEnumerable()));

            _fileRepositoryMock.Setup(x => x.EnumerateFiles(It.Is<string>(s => s.StartsWith(DirectoryWithFiles1)), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new List<string>()
                {
                    ValidFile
                }.AsEnumerable()));

            _fileRepositoryMock.Setup(x => x.EnumerateFiles(It.Is<string>(s => s.StartsWith(DirectoryWithFiles2)), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new List<string>()
                {
                    ValidFile,
                    ValidFile
                }.AsEnumerable()));

            _fileRepositoryMock.Setup(x => x.EnumerateFiles(It.Is<string>(s => s.StartsWith(DirectoryWithFiles3)), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new List<string>()
                {
                    ValidFile,
                    ValidFile,
                    ValidFile
                }.AsEnumerable()));

            _fileRepositoryMock.Setup(x => x.EnumerateFiles(It.Is<string>(s => s.StartsWith(DirectoryWithFiles4)), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new List<string>()
                {
                    ValidFile,
                    ValidFile,
                    ValidFile,
                    ValidFile
                }.AsEnumerable()));

            _fileRepositoryMock.Setup(x => x.GetFileLength(It.Is<string>(s => s.Equals(ValidFile)), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(DefaultFileLength));

            _fileRepositoryMock.Setup(x => x.GetFileLength(It.Is<string>(s => s.Equals(InaccessibleFile)), It.IsAny<CancellationToken>()))
                .Throws<UnauthorizedAccessException>();


            _sut = new FileService(_fileRepositoryMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task GetBigFolders_ShouldReturnCorrectAmount_WhenFolderAndSomeFiles()
        {
            var result = await _sut.GetBigFolders(DirectoryWithDirectory, 3, false);

            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetBigFolders_ShouldReturnOrdered_WhenFolderAndSomeFiles()
        {
            var result = await _sut.GetBigFolders(DirectoryWithDirectory, 3, false);

            var array = result.ToArray();
            Assert.Equal(DirectoryWithFiles4, array[0]);
            Assert.Equal(DirectoryWithFiles3, array[1]);
            Assert.Equal(DirectoryWithFiles2, array[2]);
        }

        [Fact]
        public async Task GetBigFolders_ShouldReturnAsCount_WhenCountIsBiggerThanFolderCount()
        {
            var result = await _sut.GetBigFolders(DirectoryWithDirectory, 10, false);

            Assert.Equal(5, result.Count());
        }

        [Fact]
        public async Task GetBigFolders_ShouldThrowUnauthorizedAccessException_WhenInaccessibleFolderAndNoSuppress()
        {
            _fileRepositoryMock.Setup(x => x.EnumerateDirectories(It.Is<string>(s => s.Equals(DirectoryWithDirectory)), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new List<string>()
                {
                    EmptyDirectory,
                    InaccessibleDirectory,
                    DirectoryWithFiles4,
                    DirectoryWithFiles3,
                    DirectoryWithFiles2,
                    DirectoryWithFiles1,
                    ValidFile
                }.AsEnumerable()));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _sut.GetBigFolders(DirectoryWithDirectory, 3, false));
        }

        [Fact]
        public async Task GetBigFolders_ShouldNotCountUnauthorizedFolders_WhenInaccessibleFolderAndSuppress()
        {
            _fileRepositoryMock.Setup(x => x.EnumerateDirectories(It.Is<string>(s => s.Equals(DirectoryWithDirectory)), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new List<string>()
                {
                    EmptyDirectory,
                    InaccessibleDirectory,
                    DirectoryWithFiles4,
                    DirectoryWithFiles3,
                    DirectoryWithFiles2,
                    DirectoryWithFiles1,
                    ValidFile
                }.AsEnumerable()));

            var result = await _sut.GetBigFolders(DirectoryWithDirectory, 3, true);

            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetBigFolders_ShouldThrowDirectoryNotFoundException_WhenInitialFolderIsNotFound()
        {
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                _sut.GetBigFolders(NoDirectory, 3, false));
        }

        public void Dispose()
        {
        }
    }
}