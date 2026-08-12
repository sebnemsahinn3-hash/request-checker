using PTN.WebAPI.Constants;
using PTN.WebAPI.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PTN.WebAPI.Extensions
{
    public static class RequestLogQueryExtensions
    {
        public static IEnumerable<RequestLogDto> ApplyFilters(
            this IEnumerable<RequestLogDto> logs,
            RequestLogQueryDto query)
        {
            var filteredLogs = logs;

            if (!string.IsNullOrWhiteSpace(query.Status) &&
                !string.Equals(
                    query.Status,
                    RequestConstants.Query.AllStatus,
                    StringComparison.OrdinalIgnoreCase))
            {
                var status = query.Status.Trim();

                filteredLogs = filteredLogs.Where(log =>
                    MatchesStatus(log.StatusCode, status));
            }

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                var searchText = query.Query.Trim();

                filteredLogs = filteredLogs.Where(log =>
                    log.Url.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase) ||

                    log.Message.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase) ||

                    log.StatusCode
                        .ToString()
                        .Contains(
                            searchText,
                            StringComparison.OrdinalIgnoreCase));
            }

            return filteredLogs;
        }

        public static IEnumerable<RequestLogDto> ApplySorting(
            this IEnumerable<RequestLogDto> logs,
            RequestLogQueryDto query)
        {
            var sortBy = query.SortBy
                .Trim()
                .ToLowerInvariant();

            var descending = string.Equals(
                query.SortDirection,
                RequestConstants.Query.Descending,
                StringComparison.OrdinalIgnoreCase);

            return sortBy switch
            {
                RequestConstants.Query.SortByUrl =>
                    descending
                        ? logs.OrderByDescending(log => log.Url)
                        : logs.OrderBy(log => log.Url),

                RequestConstants.Query.SortByStatusCode =>
                    descending
                        ? logs.OrderByDescending(log => log.StatusCode)
                        : logs.OrderBy(log => log.StatusCode),

                RequestConstants.Query.SortByCreatedAt =>
                    descending
                        ? logs.OrderByDescending(log => log.CreatedAt)
                        : logs.OrderBy(log => log.CreatedAt),

                _ =>
                    descending
                        ? logs.OrderByDescending(log => log.Id)
                        : logs.OrderBy(log => log.Id)
            };
        }

        private static bool MatchesStatus(
            int statusCode,
            string requestedStatus)
        {
            return requestedStatus switch
            {
                RequestConstants.Query.SuccessStatus =>
                    statusCode is >= 200 and < 300 ||
                    statusCode == 1,

                RequestConstants.Query.ClientErrorStatus =>
                    statusCode is >= 400 and < 500 ||
                    statusCode is 2 or 3 or 4,

                RequestConstants.Query.ServerErrorStatus =>
                    statusCode is >= 500 and < 600 ||
                    statusCode == 5,

                _ => statusCode.ToString() == requestedStatus
            };
        }
    }
}