using Microsoft.AspNetCore.Http;

namespace Abp.Spark.Services;

public static class FlashExtensions
{
    public static bool HasKey(this HttpContext? context, string key)
    {
        if (context == null) return false;
        return context.Request.Cookies.ContainsKey($"flash_{key}");
    }

    public static string GetFlash(this HttpContext? context, string key)
    {
        if (context == null) return string.Empty;
        if (context.Request.Cookies.TryGetValue($"flash_{key}", out var val))
        {
            context.Response.Cookies.Delete($"flash_{key}");
            return val ?? string.Empty;
        }
        return string.Empty;
    }

    public static void SetFlash(this HttpContext? context, string key, string message)
    {
        if (context == null) return;
        context.Response.Cookies.Append($"flash_{key}", message, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });
    }
}
