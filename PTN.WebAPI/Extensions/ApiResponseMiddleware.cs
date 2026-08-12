using Microsoft.AspNetCore.Http;
using PTN.WebAPI.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PTN.WebAPI.Extensions
{
    public class ApiResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
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
                var message = isSuccess ? "İşlem başarıyla tamamlandı" : "İşlem sırasında bir hata oluştu";

                var wrappedResponse = new
                {
                    message = message,
                    success = isSuccess,
                    status = statusCode,
                    data = dataPayload
                };

                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var finalJson = JsonSerializer.Serialize(wrappedResponse, jsonOptions);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(finalJson);
            }
            catch (Exception ex)
            {
                context.Response.Body = originalBodyStream;
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    message = ex.Message,
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