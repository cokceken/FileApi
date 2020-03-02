using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FileApi.Service;
using FileApi.V1.Model;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace FileApi.Test.Integration
{
    public class FileControllerIntegrationTest : IDisposable
    {
        private readonly IHost _host;
        private readonly HttpClient _client;
        private const string BaseDirectory = "BaseDirectory";
        private const string NoDirectory = "No-Directory";
        private const string Uri = "/api/v1/File?path={0}&count={1}&suppressAccessErrors={2}";

        public FileControllerIntegrationTest()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.UseStartup<Startup>();

                    webHost.ConfigureTestServices(collection =>
                    {
                        collection.AddTransient<IFileRepository, FakeFileRepository>();
                    });
                });

            _host = hostBuilder.StartAsync().Result;
            _client = _host.GetTestClient();
            _client.DefaultRequestVersion = new Version(1, 0, 0);
        }

        [Fact]
        public async Task GetFolders_ShouldReturnCorrectAmount()
        {
            var response = await _client.GetAsync(string.Format(Uri, HttpUtility.UrlEncode(BaseDirectory), 3, true));
            var responseMessage = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<GetFoldersResponse>(responseMessage);

            Assert.Equal(3, responseObject.Paths.Count());
        }

        [Fact]
        public async Task GetFolders_ShouldReturnCorrectOrder()
        {
            var response = await _client.GetAsync(string.Format(Uri, HttpUtility.UrlEncode(BaseDirectory), 3, true));
            var responseMessage = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<GetFoldersResponse>(responseMessage);

            Assert.EndsWith("4", responseObject.Paths.ToArray()[0]);
            Assert.EndsWith("3", responseObject.Paths.ToArray()[1]);
            Assert.EndsWith("2", responseObject.Paths.ToArray()[2]);
        }

        [Fact]
        public async Task GetFolders_ShouldReturnAllFolders_WhenTotalAmountIsHigher()
        {
            var response = await _client.GetAsync(string.Format(Uri, HttpUtility.UrlEncode(BaseDirectory), 10, true));
            var responseMessage = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<GetFoldersResponse>(responseMessage);

            Assert.Equal(5, responseObject.Paths.Count());
        }

        [Fact]
        public async Task GetFolders_ShouldReturnBadRequest_WhenFolderNotFound()
        {
            var response = await _client.GetAsync(string.Format(Uri, HttpUtility.UrlEncode(NoDirectory), 3, true));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetFolders_ShouldReturnBadRequest_WhenCountIsNotPositive()
        {
            var response = await _client.GetAsync(string.Format(Uri, HttpUtility.UrlEncode(BaseDirectory), -1, true));
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetFolders_ShouldReturnUnauthorized_WhenFolderIsNotAccessibleWithNoSuppress()
        {
            var response = await _client.GetAsync(string.Format(Uri, HttpUtility.UrlEncode(BaseDirectory), 3, false));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetFolders_ShouldReturnOk_WhenFolderIsNotAccessibleWithSuppress()
        {
            var response = await _client.GetAsync(string.Format(Uri, HttpUtility.UrlEncode(BaseDirectory), 3, true));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public void Dispose()
        {
            _host?.Dispose();
            _client?.Dispose();
        }
    }
}