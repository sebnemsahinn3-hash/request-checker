using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Localization;

namespace PTN.WebAPI.Extensions
{
    public static class LocalizationExtension
    {
        public static IServiceCollection AddCustomLocalization(this IServiceCollection services)
        {
            services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

            return services;
        }

        // TÜM Enum'lar İçin Kesin Çalışan Akıllı Global GetDescription() Metodu
        public static string GetDescription(this Enum value, IStringLocalizer? localizer = null)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            // [Description("StatusCodes:04")] etiketini okur
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            string key = attribute != null ? attribute.Description : value.ToString();

            // 1. Eğer yerel localizer geldiyse onu kullanır
            if (localizer != null)
            {
                var localized = localizer[key];
                if (!localized.ResourceNotFound)
                {
                    return localized.Value;
                }
            }

            // 2. Gelmediyse doğrudan tr.json / en.json dosyasını açıp okur!
            try
            {
                var jsonLocalizer = new JsonStringLocalizer();
                var result = jsonLocalizer[key];
                if (!result.ResourceNotFound)
                {
                    return result.Value;
                }
            }
            catch
            {
                // Hata durumunda varsayılan anahtar kalır
            }

            return key;
        }
    }
}