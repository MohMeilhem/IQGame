using Microsoft.Extensions.Options;

namespace IQGame.Admin.Services
{
    public class ImageSearchProviderSwitcher
    {
        private readonly ImageSearchConfiguration _config;
        private readonly ImageSearchServiceFactory _factory;
        private readonly ILogger<ImageSearchProviderSwitcher> _logger;

        public ImageSearchProviderSwitcher(IOptions<ImageSearchConfiguration> config, ImageSearchServiceFactory factory, ILogger<ImageSearchProviderSwitcher> logger)
        {
            _config = config.Value;
            _factory = factory;
            _logger = logger;
        }

        /// <summary>
        /// Get the currently configured preferred provider
        /// </summary>
        public string GetCurrentProvider()
        {
            return _config.PreferredProvider;
        }

        /// <summary>
        /// Get available providers based on configuration
        /// </summary>
        public List<string> GetAvailableProviders()
        {
            var providers = new List<string>();

            if (!string.IsNullOrEmpty(_config.GoogleCSE.ApiKey) && !string.IsNullOrEmpty(_config.GoogleCSE.SearchEngineId))
                providers.Add("GoogleCSE");

            if (!string.IsNullOrEmpty(_config.SerpAPI.ApiKey))
                providers.Add("SerpAPI");

            return providers;
        }

        /// <summary>
        /// Test a specific provider
        /// </summary>
        public async Task<bool> TestProvider(string providerName)
        {
            try
            {
                var service = _factory.CreateService(providerName);
                if (service == null)
                    return false;

                var result = await service.SearchAndDownloadImageAsync("test");
                return !result.Contains("defaults");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to test provider: {providerName}");
                return false;
            }
        }

        /// <summary>
        /// Get provider status information
        /// </summary>
        public async Task<Dictionary<string, object>> GetProviderStatus()
        {
            var status = new Dictionary<string, object>
            {
                ["PreferredProvider"] = _config.PreferredProvider,
                ["AvailableProviders"] = GetAvailableProviders(),
                ["GoogleCSE_Configured"] = !string.IsNullOrEmpty(_config.GoogleCSE.ApiKey) && !string.IsNullOrEmpty(_config.GoogleCSE.SearchEngineId),
                ["SerpAPI_Configured"] = !string.IsNullOrEmpty(_config.SerpAPI.ApiKey)
            };

            // Test each configured provider
            foreach (var provider in GetAvailableProviders())
            {
                status[$"{provider}_Working"] = await TestProvider(provider);
            }

            return status;
        }
    }
} 