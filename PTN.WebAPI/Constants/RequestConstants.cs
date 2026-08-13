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

        public static class Query
        {
            public const string AllStatus = "ALL";
            public const string SuccessStatus = "200";
            public const string ClientErrorStatus = "400";
            public const string ServerErrorStatus = "500";

            public const string SortById = "id";
            public const string SortByUrl = "url";
            public const string SortByStatusCode = "statuscode";
            public const string SortByCreatedAt = "createdat";

            public const string Ascending = "asc";
            public const string Descending = "desc";

            public const int FirstPage = 1;
            public const int DefaultPageSize = 50;
            public const int MaxPageSize = 100;
        }

        public static class RequestMessages
        {
            private const string Prefix = nameof(RequestMessages);

            public const string isSuccessStatusMessage =
                $"{Prefix}:SuccessMessage";

            public const string ServerRespondedMessage =
                $"{Prefix}:ServerRespondedMessage";

            public const string TimeoutMessage =
                $"{Prefix}:TimeoutMessage";
        }

        public static class EndpointHealth
        {
            public const string DefaultEndpoint = "GET /api/check";
            public const string MillisecondSuffix = "ms";
            public const string Healthy = "Healthy";
            public const string Degraded = "Degraded";
        }
        public static class TimeRange
        {
            public const string OneHour = "1h";
            public const string TwentyFourHours = "24h";
        }
        public static class Dashboard
        {
            public const string DefaultBucket = "5m";
            public const string TimestampFormat = "yyyy-MM-dd HH:mm";

            public const int DefaultBucketMinutes = 5;
            public const int MaxTimeSeriesPoints = 12;
        }
    }
}