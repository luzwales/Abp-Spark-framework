using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Spark.Templates.Blazor.Application.Services;

/// <summary>
/// Lightweight JSON-based i18n service (similar to vue-i18n) for Blazor & ASP.NET Core
/// </summary>
public class I18nService
{
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> Cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHostEnvironment _env;

    public const string DefaultLocale = "zh";
    public const string CookieName = "spark_locale";

    public I18nService(IHttpContextAccessor httpContextAccessor, IHostEnvironment env)
    {
        _httpContextAccessor = httpContextAccessor;
        _env = env;
    }

    /// <summary>
    /// Indexer access: @L["dashboard"]
    /// </summary>
    public string this[string key] => Get(key);

    /// <summary>
    /// Get translated string with optional format arguments
    /// </summary>
    public string Get(string key, params object[] args)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;

        var culture = CurrentCulture;
        var dict = LoadTranslations(culture);

        if (!dict.TryGetValue(key, out var val))
        {
            // Fallback to default locale if not found in current locale
            if (!string.Equals(culture, DefaultLocale, StringComparison.OrdinalIgnoreCase))
            {
                var defaultDict = LoadTranslations(DefaultLocale);
                if (defaultDict.TryGetValue(key, out val))
                {
                    return args.Length > 0 ? string.Format(val, args) : val;
                }
            }
            return key;
        }

        return args.Length > 0 ? string.Format(val, args) : val;
    }

    /// <summary>
    /// Current language code ("zh" or "en")
    /// </summary>
    public string CurrentCulture
    {
        get
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx != null && ctx.Request.Cookies.TryGetValue(CookieName, out var cookieVal) && !string.IsNullOrWhiteSpace(cookieVal))
            {
                var lower = cookieVal.Trim().ToLowerInvariant();
                if (lower.StartsWith("en")) return "en";
                if (lower.StartsWith("zh")) return "zh";
            }
            return DefaultLocale;
        }
    }

    private Dictionary<string, string> LoadTranslations(string culture)
    {
        culture = culture.ToLowerInvariant();
        return Cache.GetOrAdd(culture, c =>
        {
            try
            {
                var filePath = Path.Combine(_env.ContentRootPath, "Locales", $"{c}.json");
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(AppContext.BaseDirectory, "Locales", $"{c}.json");
                }

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (dict != null)
                    {
                        return new Dictionary<string, string>(dict, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
                // Ignore load error and fallback to empty dict
            }

            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        });
    }
}
