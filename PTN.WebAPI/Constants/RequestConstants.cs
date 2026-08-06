using Microsoft.AspNetCore.Http;
using System;

namespace PTN.WebAPI.Constants
{
    public static class RequestConstants

    {
        public const string BaseUrl = "https://ventral-vivan-brinkless.ngrok-free.dev/api/gtfs/check";
        
        public const string RequestLog =
        "Url: {0}\n" +
        "RequestParams: {1}\n" +
        "RequestBody: {2}\n" +
        "ResponseBody: {3}\n" +
        "Timing: {4}\n" +
        "StatusCode: {5}\n" +
        "Message: {6}\n" +
        "--------------------------------------------------";
        public const string SummaryTemplate =
       @"=== SERVİS SAĞLIK & İSTATİSTİK ÖZET RAPORU (5 DAKİKALIK PERİYOTLU) ===
       Son Güncelleme Zamanı : {0:yyyy-MM-dd HH:mm:ss}
 
       Toplam Atılan İstek Sayısı     : {1}
       200 OK Dönüş Sayısı           : {2}
       Sağlıklı (Healthy) İstekler   : {3}
       Sağlıksız (Unhealthy) İstekler: {4}
       Başarı / Sağlık Oranı (%)     : %{5:F2}
===============================================";
        public const string DefaultRequestHeaders = "ngrok-skip-browser-warning";
        public const string LogFilePath = "istek_analiz_log.txt";
        public const string SummaryFilePath = "ozet_rapor.txt";
        public const string DefaultValue = "-";
        public const string DefaultHeaderValue = "true";
        public static class RequestMessages
        {
            private const string Prefix = nameof(RequestMessages);
            public const string isSuccessStatusMessage = $"{Prefix}:SuccessMessage";
            public const string ServerRespondedMessage = "Sunucudan yanıt döndü.";
            public const string TimeoutMessage = " istek zaman aşımı. ";
        }
    
    }
}