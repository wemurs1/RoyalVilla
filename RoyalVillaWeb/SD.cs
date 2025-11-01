namespace RoyalVillaWeb
{
    public static class SD
    {
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
        public const string SessionAccessToken = "JWTToken";
        public const string SessionRefreshToken = "RefreshToken";
        public const string CurrentAPIVersion = "v2";
        public static string APIBaseUrl { get; set; }


        public static string GetImageUrl(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return "/images/placeholder-villa.jpg";
            }
            if (imageUrl.StartsWith("http"))
            {
                return imageUrl;
            }
            return $"{APIBaseUrl}{imageUrl}";
        }
    }
}
