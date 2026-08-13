using PTN.WebAPI.Constants;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PTN.WebAPI.Extensions
{
    public static class DynamicQueryExtensions
    {
        public static IQueryable<T> ApplyDynamicFilter<T>(
            this IQueryable<T> query,
            string? filterValue,
            string? propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(filterValue))
            {
                return query;
            }

            var properties = GetFilterableProperties<T>(propertyName);

            if (properties.Length == 0)
            {
                return query.Where(_ => false);
            }

            var parameter = Expression.Parameter(
                typeof(T),
                "item");

            Expression? combinedExpression = null;

            foreach (var property in properties)
            {
                var propertyExpression = Expression.Property(
                    parameter,
                    property);

                var filterExpression = CreateFilterExpression(
                    propertyExpression,
                    property.PropertyType,
                    filterValue.Trim());

                if (filterExpression == null)
                {
                    continue;
                }

                combinedExpression = combinedExpression == null
                    ? filterExpression
                    : Expression.OrElse(
                        combinedExpression,
                        filterExpression);
            }

            if (combinedExpression == null)
            {
                return query.Where(_ => false);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(
                combinedExpression,
                parameter);

            return query.Where(lambda);
        }

        public static IQueryable<T> ApplyDynamicSorting<T>(
            this IQueryable<T> query,
            string? propertyName,
            string? sortDirection)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return query;
            }

            var property = GetProperty<T>(propertyName);

            if (property == null)
            {
                return query;
            }

            var parameter = Expression.Parameter(
                typeof(T),
                "item");

            var propertyExpression = Expression.Property(
                parameter,
                property);

            var orderExpression = Expression.Lambda(
                propertyExpression,
                parameter);

            var methodName = string.Equals(
                sortDirection,
                RequestConstants.Query.Descending,
                StringComparison.OrdinalIgnoreCase)
                    ? nameof(Queryable.OrderByDescending)
                    : nameof(Queryable.OrderBy);

            var methodCall = Expression.Call(
                typeof(Queryable),
                methodName,
                new[]
                {
                    typeof(T),
                    property.PropertyType
                },
                query.Expression,
                Expression.Quote(orderExpression));

            return query.Provider.CreateQuery<T>(methodCall);
        }

        private static PropertyInfo[] GetFilterableProperties<T>(
            string? propertyName)
        {
            if (!string.IsNullOrWhiteSpace(propertyName))
            {
                var selectedProperty = GetProperty<T>(propertyName);

                return selectedProperty == null
                    ? Array.Empty<PropertyInfo>()
                    : new[] { selectedProperty };
            }

            return typeof(T)
                .GetProperties(
                    BindingFlags.Public |
                    BindingFlags.Instance)
                .Where(property =>
                    property.CanRead &&
                    property.GetIndexParameters().Length == 0)
                .ToArray();
        }

        private static PropertyInfo? GetProperty<T>(
            string propertyName)
        {
            return typeof(T).GetProperty(
                propertyName,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);
        }

        private static Expression? CreateFilterExpression(
            Expression propertyExpression,
            Type propertyType,
            string filterValue)
        {
            var actualType =
                Nullable.GetUnderlyingType(propertyType) ??
                propertyType;

            if (actualType == typeof(string))
            {
                return CreateStringFilterExpression(
                    propertyExpression,
                    filterValue);
            }

            if (!TryConvertValue(
                    filterValue,
                    actualType,
                    out var convertedValue))
            {
                return null;
            }

            var constantExpression = Expression.Constant(
                convertedValue,
                actualType);

            if (Nullable.GetUnderlyingType(propertyType) != null)
            {
                var hasValueExpression = Expression.Property(
                    propertyExpression,
                    "HasValue");

                var valueExpression = Expression.Property(
                    propertyExpression,
                    "Value");

                var equalsExpression = Expression.Equal(
                    valueExpression,
                    constantExpression);

                return Expression.AndAlso(
                    hasValueExpression,
                    equalsExpression);
            }

            return Expression.Equal(
                propertyExpression,
                constantExpression);
        }

        private static Expression CreateStringFilterExpression(
            Expression propertyExpression,
            string filterValue)
        {
            var nullExpression = Expression.Constant(
                null,
                typeof(string));

            var notNullExpression = Expression.NotEqual(
                propertyExpression,
                nullExpression);

            var toLowerMethod = typeof(string).GetMethod(
                nameof(string.ToLower),
                Type.EmptyTypes)!;

            var containsMethod = typeof(string).GetMethod(
                nameof(string.Contains),
                new[] { typeof(string) })!;

            var lowercaseProperty = Expression.Call(
                propertyExpression,
                toLowerMethod);

            var lowercaseFilter = Expression.Constant(
                filterValue.ToLowerInvariant());

            var containsExpression = Expression.Call(
                lowercaseProperty,
                containsMethod,
                lowercaseFilter);

            return Expression.AndAlso(
                notNullExpression,
                containsExpression);
        }

        private static bool TryConvertValue(
            string filterValue,
            Type targetType,
            out object? convertedValue)
        {
            try
            {
                if (targetType.IsEnum)
                {
                    convertedValue = Enum.Parse(
                        targetType,
                        filterValue,
                        true);

                    return true;
                }

                if (targetType == typeof(Guid))
                {
                    var success = Guid.TryParse(
                        filterValue,
                        out var guidValue);

                    convertedValue = guidValue;
                    return success;
                }

                var converter = TypeDescriptor.GetConverter(
                    targetType);

                if (!converter.CanConvertFrom(typeof(string)))
                {
                    convertedValue = null;
                    return false;
                }

                convertedValue = converter.ConvertFrom(
                    null,
                    CultureInfo.InvariantCulture,
                    filterValue);

                return true;
            }
            catch
            {
                convertedValue = null;
                return false;
            }
        }
    }
}