using System;
using System.IO;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using FileApi.V1.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FileApi
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var code = HttpStatusCode.InternalServerError;
            var message = ex.Message;

            if (ex is DirectoryNotFoundException _)
                code = HttpStatusCode.BadRequest;
            else if (ex is PathTooLongException _ || ex is SecurityException _ || ex is UnauthorizedAccessException _)
                code = HttpStatusCode.Unauthorized;
            else
                message = "Internal server error";

            var result = JsonConvert.SerializeObject(new ErrorResponse {Message = message});
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) code;
            return context.Response.WriteAsync(result);
        }
    }
}