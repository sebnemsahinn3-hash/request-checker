using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PTN.WebAPI.Localization
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name]
        {
            get
            {
                var value = GetValue(name);
                return new LocalizedString(name, value ?? name, value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var val = GetValue(name);
                var value = val != null ? string.Format(val, arguments) : name;
                return new LocalizedString(name, value, val == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", $"{culture}.json");
            if (!File.Exists(filePath)) return Enumerable.Empty<LocalizedString>();

            var json = File.ReadAllText(filePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return dict?.Select(k => new LocalizedString(k.Key, k.Value)) ?? Enumerable.Empty<LocalizedString>();
        }

        private string? GetValue(string key)
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", $"{culture}.json");

            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "tr.json");
            }

            if (!File.Exists(filePath)) return null;

            var jsonText = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            if (root.TryGetProperty(key, out var directElement))
            {
                return directElement.GetString();
            }

            var parts = key.Split(':');
            var current = root;
            foreach (var part in parts)
            {
                if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(part, out var child))
                {
                    current = child;
                }
                else
                {
                    return null;
                }
            }

            return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
        }
    }

    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource)
        {
            return new JsonStringLocalizer();
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return new JsonStringLocalizer();
        }
    }
}