using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using System.Linq;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Constants;

namespace PTN.WebAPI.Extensions
{
    public class ApiResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IStringLocalizer<ApiResponseMiddleware> localizer)
        {
            // Swagger, SignalR, SSE streams ve CORS OPTIONS preflight isteklerini pas geç
            if (HttpMethods.IsOptions(context.Request.Method) ||
                context.Request.Path.StartsWithSegments("/swagger") || 
                context.Request.Path.StartsWithSegments("/hubs") ||
                context.Request.Path.StartsWithSegments("/api/logs/stream"))
            {
                await _next(context);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                await _next(context);
                if (context.Response.Headers.ContainsKey(
                        HeaderNames.ContentDisposition))
                {
                    context.Response.Body = originalBodyStream;

                    memoryStream.Seek(
                        0,
                        SeekOrigin.Begin);

                    await memoryStream.CopyToAsync(
                        originalBodyStream);

                    return;
                }
                context.Response.Body = originalBodyStream;
                memoryStream.Seek(0, SeekOrigin.Begin);

                var responseBodyText = await new StreamReader(memoryStream).ReadToEndAsync();
                var statusCode = context.Response.StatusCode;

                // Eğer yanıt zaten ApiResponse formatındaysa müdahale etme
                if (responseBodyText.Contains("\"success\":") && responseBodyText.Contains("\"status\":"))
                {
                    await context.Response.WriteAsync(responseBodyText);
                    return;
                }

                object? dataPayload = null;
                if (!string.IsNullOrWhiteSpace(responseBodyText))
                {
                    try
                    {
                        dataPayload = JsonSerializer.Deserialize<object>(responseBodyText);
                    }
                    catch
                    {
                        dataPayload = responseBodyText;
                    }
                }

                var isSuccess = statusCode >= 200 && statusCode < 300;
                var messageKey = isSuccess
                    ? ExceptionCodes.SuccessMessage
                    : ExceptionCodes.ErrorMessage;

                var message = localizer[messageKey].Value;

                var wrappedResponse = new
                {
                    message = message,
                    success = isSuccess,
                    status = statusCode,
                    data = dataPayload
                };

                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var finalJson = JsonSerializer.Serialize(wrappedResponse, jsonOptions);

                context.Response.Headers.Remove("Content-Length");
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsync(finalJson);
            }
            catch (UnauthorizedAccessException ex)
            {
                context.Response.Body = originalBodyStream;
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                context.Response.ContentType =
                    "application/json; charset=utf-8";

                context.Response.Headers.Remove(
                    "Content-Length");

                var errorResponse = new
                {
                    message = ex.Message,
                    success = false,
                    status = StatusCodes.Status401Unauthorized,
                    data = (object?)null
                };

                var finalJson =
                    JsonSerializer.Serialize(errorResponse);

                await context.Response.WriteAsync(finalJson);
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                context.Response.Body = originalBodyStream;
                context.Response.StatusCode =
                    StatusCodes.Status404NotFound;

                context.Response.ContentType =
                    "application/json; charset=utf-8";

                context.Response.Headers.Remove(
                    "Content-Length");

                var errorResponse = new
                {
                    message = ex.Message,
                    success = false,
                    status = StatusCodes.Status404NotFound,
                    data = (object?)null
                };

                var finalJson =
                    JsonSerializer.Serialize(errorResponse);

                await context.Response.WriteAsync(finalJson);
            }
            catch (ValidationException ex)
            {
                context.Response.Body = originalBodyStream;
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json; charset=utf-8";

                var errors = ex.Errors
                    .Select(error => error.ErrorMessage)
                    .ToList();

                var errorResponse = new
                {
                    message = errors.FirstOrDefault(),
                    success = false,
                    status = StatusCodes.Status400BadRequest,
                    data = new
                    {
                        errors
                    }
                };

                var finalJson = JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(finalJson);
            }
            catch (Exception )
            {
                context.Response.Body = originalBodyStream;
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    message = localizer[
                        ExceptionCodes.ErrorMessage
                    ].Value,
                    success = false,
                    status = 500,
                    data = (object?)null
                };

                var finalJson = JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(finalJson);
            }
        }
    }
}