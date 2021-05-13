using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<string> _action;

        public RequestResponseLoggingMiddleware(RequestDelegate next, Action<string> action)
        {
            _next = next;
            _action = action;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();
                var requestBody = await GetRequestBody(context.Request);

                var originalBodyStream = context.Response.Body;
                using var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;

                await _next(context);
                var responseBody = await GetResponseBody(context.Response);

                var message = new
                {
                    Endpoint = context.Request.Path,
                    IPAddress = context.Connection.RemoteIpAddress.ToString(),
                    RequestHeaders = context.Request.Headers,
                    RequestBody = requestBody,
                    ResponseHeaders = context.Response.Headers,
                    ResponseBody = responseBody,
                    ResponseStatusCode = context.Response.StatusCode,
                    Time = DateTime.Now
                };

                var logRecord = JsonConvert.SerializeObject(message);

                _action(logRecord);

                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                context.Request.EnableBuffering();
                var requestBody = await GetRequestBody(context.Request);
                var message = new
                {
                    Endpoint = context.Request.Path,
                    IPAddress = context.Connection.RemoteIpAddress.ToString(),
                    RequestHeaders = context.Request.Headers,
                    RequestBody = requestBody,
                    ResponseBody = ex.Message,
                    Time = DateTime.Now
                };

                var logRecord = JsonConvert.SerializeObject(message);

                _action(logRecord);

                throw;
            }
        }

        private async Task<string> GetRequestBody(HttpRequest request)
        {
            using var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private async Task<string> GetResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            string body = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return body;
        }
    }
}
