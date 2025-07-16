namespace IQGame.Admin.Services
{
    public class ImageSearchConfiguration
    {
        public string PreferredProvider { get; set; } = "GoogleCSE";
        public GoogleCseConfig GoogleCSE { get; set; } = new();
        public SerpApiConfig SerpAPI { get; set; } = new();
    }

    public class GoogleCseConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SearchEngineId { get; set; } = string.Empty;
    }

    public class SerpApiConfig
    {
        public string ApiKey { get; set; } = string.Empty;
    }
} 