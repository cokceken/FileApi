using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using FileApi.Service;
using FileApi.V1.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileApi.V1.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(GetFoldersResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<GetFoldersResponse> GetFolders([Required] string path, [Range(1, int.MaxValue)] int count = 5, bool suppressAccessErrors = false, CancellationToken cancellation = default)
        {
            var folders = await _fileService.GetBigFolders(path, count,
                suppressAccessErrors, cancellation);

            var result = new GetFoldersResponse()
            {
                Paths = folders
            };

            return result;
        }
    }
}