namespace SmartGear.PM0902.Caching;
public static class CacheKeys
{    public static string Products(string? categorySlug = null)
        => $"products:list:{categorySlug?.ToLowerInvariant() ?? "all"}";
}