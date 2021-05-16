using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<string> _action;
        private readonly LogProperties _logProperties;
        private const string _localhostTag = "::1";
        private const string _localhostName = "localhost";

        public RequestResponseLoggingMiddleware(RequestDelegate next, Action<string> action = null, LogProperties logProperties = null)
        {
            _next = next;
            _action = action;
            _logProperties = logProperties;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            dynamic message = new ExpandoObject();

            try
            {
                context.Request.EnableBuffering();
                var requestBody = await GetRequestBody(context.Request);

                var originalBodyStream = context.Response.Body;
                using var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;

                await _next(context);
                var responseBody = await GetResponseBody(context.Response);

                if (_logProperties?.HasRequestPath != false)
                    message.RequestPath = context.Request.Path;

                if (_logProperties?.HasRemoteIpAddress != false)
                    message.RemoteIpAddress = context.Connection.RemoteIpAddress.ToString() == _localhostTag
                        ? _localhostName : context.Connection.RemoteIpAddress.ToString();

                if (_logProperties?.HasRequestHeaders != false)
                    message.RequestHeaders = context.Request.Headers;

                if (_logProperties?.HasRequestBody != false)
                    message.RequestBody = requestBody;

                if (_logProperties?.HasResponseStatusCode != false)
                    message.ResponseStatusCode = context.Response.StatusCode;

                if (_logProperties?.HasResponseBody != false)
                    message.ResponseBody = responseBody;

                if (_logProperties?.HasCreatedAt != false)
                    message.CreatedAt = DateTime.UtcNow;

                var logRecord = JsonConvert.SerializeObject(message);

                (_action ?? Console.WriteLine)(logRecord);

                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                context.Request.EnableBuffering();
                var requestBody = await GetRequestBody(context.Request);

                if (_logProperties?.HasRequestPath != false)
                    message.RequestPath = context.Request.Path;

                if (_logProperties?.HasRemoteIpAddress != false)
                    message.RemoteIpAddress = context.Connection.RemoteIpAddress.ToString() == _localhostTag
                        ? _localhostName : context.Connection.RemoteIpAddress.ToString();

                if (_logProperties?.HasRequestHeaders != false)
                    message.RequestHeaders = context.Request.Headers;

                if (_logProperties?.HasRequestBody != false)
                    message.RequestBody = requestBody;

                if (_logProperties?.HasResponseStatusCode != false)
                    message.ResponseStatusCode = null;

                if (_logProperties?.HasResponseBody != false)
                    message.ResponseBody = ex.Message;

                if (_logProperties?.HasCreatedAt != false)
                    message.CreatedAt = DateTime.UtcNow;

                var logRecord = JsonConvert.SerializeObject(message);

                (_action ?? Console.WriteLine)(logRecord);

                throw;
            }
        }

        private async Task<dynamic> GetRequestBody(HttpRequest request)
        {
            using var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var requestBody = await reader.ReadToEndAsync();
            var body = JsonConvert.DeserializeObject<dynamic>(requestBody);
            request.Body.Position = 0;
            return body;
        }

        private async Task<dynamic> GetResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
            var body = JsonConvert.DeserializeObject<dynamic>(responseBody);
            response.Body.Seek(0, SeekOrigin.Begin);
            return body;
        }
    }
}
