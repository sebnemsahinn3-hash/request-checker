using PTN.WebAPI.Constants;
using System.Diagnostics;

namespace PTN.WebAPI
{
    public class HealthCheckBackgroundService : BackgroundService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public static HealthMetrics Metrics { get; } = new HealthMetrics();
        private readonly IServiceProvider _serviceProvider;

        private static DateTime lastWriteTime = DateTime.Now;
        private static readonly TimeSpan fileWriteInterval = TimeSpan.FromMinutes(5);
        private static readonly int maxLogCount = 1000;

        public HealthCheckBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            if (!httpClient.DefaultRequestHeaders.Contains(RequestConstants.DefaultRequestHeaders))
            {
                httpClient.DefaultRequestHeaders.Add(RequestConstants.DefaultRequestHeaders, RequestConstants.DefaultHeaderValue);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string targetUrl = RequestConstants.BaseUrl;

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var apiSetting = dbContext.ApiSettings.FirstOrDefault();
                        if (apiSetting != null && !string.IsNullOrEmpty(apiSetting.Url))
                        {
                            targetUrl = apiSetting.Url;
                        }
                    }
                }
                catch { }

                await JobMetodu(targetUrl);
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task JobMetodu(string url)
        {
            Metrics.TotalRequests++;
            Stopwatch kronometre = Stopwatch.StartNew();

            string requestParams = RequestConstants.DefaultValue;
            string requestBody = RequestConstants.DefaultValue;
            string responseBody = "";
            string timing = "";
            string statusCode = "";
            string message = "";
            bool isHealthy = false;

            try
            {
                HttpResponseMessage cevap = await httpClient.GetAsync(url);
                kronometre.Stop();

                timing = $"{kronometre.ElapsedMilliseconds} ms";
                statusCode = $"{(int)cevap.StatusCode}";

                string hamResponse = await cevap.Content.ReadAsStringAsync();
                responseBody = hamResponse.Replace("\n", " ").Replace("\r", " ").Trim();

                if (responseBody.Length > 300)
                {
                    responseBody = responseBody.Substring(0, 300) + "...";
                }

                if (cevap.IsSuccessStatusCode)
                {
                    message = RequestConstants.RequestMessages.isSuccessStatusMessage;
                    isHealthy = true;
                    if ((int)cevap.StatusCode == 200) Metrics.Success200Count++;
                }
                else
                {
                    message = RequestConstants.RequestMessages.ServerRespondedMessage;
                    isHealthy = false;
                }
            }
            catch (TaskCanceledException)
            {
                kronometre.Stop();
                timing = $"{kronometre.ElapsedMilliseconds} ms";
                statusCode = "408";
                responseBody = RequestConstants.DefaultValue;
                message = RequestConstants.RequestMessages.TimeoutMessage;
                isHealthy = false;
            }
            catch (Exception ex)
            {
                kronometre.Stop();
                timing = $"{kronometre.ElapsedMilliseconds} ms";
                statusCode = "0";
                responseBody = RequestConstants.DefaultValue;
                message = ex.Message;
                isHealthy = false;
            }

            if (isHealthy) Metrics.HealthyCount++;
            else Metrics.UnhealthyCount++;

            Metrics.LastUpdate = DateTime.Now;

            string tekLogBlok = string.Format(
                RequestConstants.RequestLog,
                url,
                requestParams,
                requestBody,
                responseBody,
                timing,
                statusCode,
                message
            );

            // 1. PostgreSQL Veritabanına Repository üzerinden kayıt basma
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<PTN.WebAPI.Repositories.IRequestRepository>();
                    await repository.AddLogAsync(new PTN.WebAPI.Entities.RequestLogEntity
                    {
                        Url = url,
                        RequestParams = requestParams,
                        RequestBody = requestBody,
                        ResponseBody = responseBody,
                        Timing = timing,
                        StatusCode = (int)(statusCode.ToString() switch
                        {
                            "200" => PTN.WebAPI.Enums.StatusCodes.Success,
                            "400" => PTN.WebAPI.Enums.StatusCodes.BadRequest,
                            "401" => PTN.WebAPI.Enums.StatusCodes.Unauthorized,
                            "404" => PTN.WebAPI.Enums.StatusCodes.NotFound,
                            _ => PTN.WebAPI.Enums.StatusCodes.InternalServerError
                        }),
                        Message = message,
                        CreatedAt = DateTime.UtcNow // Tarih hatasını çözen kritik ekleme!
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Kayıt Hatası: {ex.Message}");
                // 2. Belleğe (RAM) Ekleme ve Maksimum 1000 Kayıt Sınırı
                lock (Metrics.RecentLogs)
                {
                    Metrics.RecentLogs.Add(tekLogBlok);

                    // 1000 kaydı aşarsa en eskiyi siler!
                    if (Metrics.RecentLogs.Count > maxLogCount)
                    {
                        Metrics.RecentLogs.RemoveAt(0);
                    }
                }
            }
        }

        private void DosyalaraBas()
        {
            try
            {
                lock (Metrics.RecentLogs)
                {
                    File.WriteAllText(RequestConstants.LogFilePath, string.Join(Environment.NewLine, Metrics.RecentLogs) + Environment.NewLine);
                }

                string ozetIcerik = string.Format(
                    RequestConstants.SummaryTemplate,
                    Metrics.LastUpdate,
                    Metrics.TotalRequests,
                    Metrics.Success200Count,
                    Metrics.HealthyCount,
                    Metrics.UnhealthyCount,
                    Metrics.HealthPercentage
                );

                File.WriteAllText(RequestConstants.SummaryFilePath, ozetIcerik);
            }
            catch { }
        }
    }
}