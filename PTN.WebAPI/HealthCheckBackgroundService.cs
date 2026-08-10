using Microsoft.AspNetCore.SignalR;
using PTN.WebAPI.Constants;
using PTN.WebAPI.Hubs;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI
{
    public class HealthCheckBackgroundService : BackgroundService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public static HealthMetrics Metrics { get; } = new HealthMetrics();
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<HealthHub> _hubContext;

        // Son 3 Dakika Sağlıksız Durum Takip Değişkenleri
        private DateTime? _unhealthyStartTime = null;
        private bool _alertSent = false;

        public HealthCheckBackgroundService(IServiceProvider serviceProvider, IHubContext<HealthHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
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
                
                // Demo / Test Amacıyla Takip (5 saniyede bir kontrol edilir)
                await CheckThreeMinuteThresholdAsync();
            }
        }

        private async Task CheckThreeMinuteThresholdAsync()
        {
            if (_unhealthyStartTime.HasValue)
            {
                var duration = DateTime.Now - _unhealthyStartTime.Value;

                // Son 3 dakika boyunca kesintisiz yanıt alınamadıysa SignalR canlı bildirimi fırlatır!
                if (duration >= TimeSpan.FromMinutes(3) && !_alertSent)
                {
                    await _hubContext.Clients.All.SendAsync(
                        "ReceiveCriticalHealthAlert", 
                        "KRİTİK UYARI: SİSTEM YANIT VERMİYOR!", 
                        "API servisi son 3 dakikadır kesintisiz yanıt vermiyor! Lütfen sunucuyu kontrol edin."
                    );
                    _alertSent = true;
                }
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

            if (isHealthy)
            {
                Metrics.HealthyCount++;
                // Sistem normale dönünce 3 dakika takibini sıfırla
                _unhealthyStartTime = null;
                _alertSent = false;
            }
            else
            {
                Metrics.UnhealthyCount++;
                // İlk kez hataya düştüyse zamanı kaydet
                if (!_unhealthyStartTime.HasValue)
                {
                    _unhealthyStartTime = DateTime.Now;
                }
            }

            Metrics.LastUpdate = DateTime.Now;

            // PostgreSQL Veritabanı Kaydı
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
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Kayıt Hatası: {ex.Message}");
            }
        }
    }
}