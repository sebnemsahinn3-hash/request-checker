
using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Services;
using System.Text.Json;

namespace PTN.WebAPI.Controllers
{
    [ApiController]
    [Route("api/logs/stream")]
    public class LogsStreamController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public LogsStreamController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        /// <summary>
        /// GET /api/logs/stream
        /// Server-Sent Events (SSE) stream endpoint for live real-time log streaming
        /// </summary>
        [HttpGet]
        public async Task GetLogStream(CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Connection = "keep-alive";
            int lastSeenId = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var logs = await _requestService.GetAllLogsAsync(cancellationToken);
                    var newLogs = logs.Where(l => l.Id > lastSeenId).OrderBy(l => l.Id).ToList();

                    if (newLogs.Any())
                    {
                        lastSeenId = newLogs.Max(l => l.Id);
                        foreach (var log in newLogs)
                        {
                            var json = JsonSerializer.Serialize(log);
                            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                        }
                        await Response.Body.FlushAsync(cancellationToken);
                    }

                    await Task.Delay(2000, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    await Response.WriteAsync($"event: error\ndata: {ex.Message}\n\n", cancellationToken);
                    break;
                }
            }
        }
    }
}
