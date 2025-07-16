using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace IQGame.Admin.Services
{
    public class ImageSearchServiceFactory
    {
        private readonly ImageSearchConfiguration _config;
        private readonly ILogger<ImageSearchServiceFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ImageSearchServiceFactory(IOptions<ImageSearchConfiguration> config, ILogger<ImageSearchServiceFactory> logger, IServiceProvider serviceProvider)
        {
            _config = config.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
            
            // Debug configuration loading
            _logger.LogInformation($"Configuration loaded - PreferredProvider: '{_config.PreferredProvider}'");
            _logger.LogInformation($"GoogleCSE ApiKey length: {_config.GoogleCSE.ApiKey?.Length ?? 0}");
            _logger.LogInformation($"GoogleCSE SearchEngineId length: {_config.GoogleCSE.SearchEngineId?.Length ?? 0}");
            _logger.LogInformation($"SerpAPI ApiKey length: {_config.SerpAPI.ApiKey?.Length ?? 0}");
        }

        public IImageSearchService CreateImageSearchService()
        {
            _logger.LogInformation($"Creating image search service. Preferred provider: {_config.PreferredProvider}");
            _logger.LogInformation($"GoogleCSE configured: {!string.IsNullOrEmpty(_config.GoogleCSE.ApiKey) && !string.IsNullOrEmpty(_config.GoogleCSE.SearchEngineId)}");
            _logger.LogInformation($"SerpAPI configured: {!string.IsNullOrEmpty(_config.SerpAPI.ApiKey)}");
            
            // Try to create the preferred provider first
            if (!string.IsNullOrEmpty(_config.PreferredProvider))
            {
                var preferredService = CreateService(_config.PreferredProvider);
                if (preferredService != null)
                {
                    _logger.LogInformation($"Using preferred image search provider: {_config.PreferredProvider}");
                    return preferredService;
                }
                else
                {
                    _logger.LogWarning($"Preferred provider '{_config.PreferredProvider}' failed to initialize, trying fallback");
                }
            }

            // Fallback logic: try Google CSE first, then SerpAPI
            var googleCseService = CreateService("GoogleCSE");
            if (googleCseService != null)
            {
                _logger.LogInformation("Using Google CSE as image search provider");
                return googleCseService;
            }

            var serpApiService = CreateService("SerpAPI");
            if (serpApiService != null)
            {
                _logger.LogInformation("Using SerpAPI as image search provider");
                return serpApiService;
            }

            // If all else fails, return a null service that returns default images
            _logger.LogWarning("No image search providers available, using default image service");
            return new DefaultImageSearchService();
        }

        public IImageSearchService? CreateService(string provider)
        {
            try
            {
                _logger.LogInformation($"Attempting to create service for provider: {provider}");
                
                switch (provider.ToUpperInvariant())
                {
                    case "GOOGLECSE":
                        _logger.LogInformation($"GoogleCSE config - ApiKey: {!string.IsNullOrEmpty(_config.GoogleCSE.ApiKey)}, SearchEngineId: {!string.IsNullOrEmpty(_config.GoogleCSE.SearchEngineId)}");
                        if (!string.IsNullOrEmpty(_config.GoogleCSE.ApiKey) && !string.IsNullOrEmpty(_config.GoogleCSE.SearchEngineId))
                        {
                            var httpClient = _serviceProvider.GetRequiredService<HttpClient>();
                            _logger.LogInformation("GoogleCSE service created successfully");
                            return new GoogleCseImageSearchService(_config.GoogleCSE, httpClient);
                        }
                        else
                        {
                            _logger.LogWarning("GoogleCSE service not created: missing API key or search engine ID");
                        }
                        break;

                    case "SERPAPI":
                        _logger.LogInformation($"SerpAPI config - ApiKey: {!string.IsNullOrEmpty(_config.SerpAPI.ApiKey)}");
                        if (!string.IsNullOrEmpty(_config.SerpAPI.ApiKey))
                        {
                            _logger.LogInformation($"Creating SerpAPI service with key: {_config.SerpAPI.ApiKey.Substring(0, 10)}...");
                            var httpClient = _serviceProvider.GetRequiredService<HttpClient>();
                            var service = new SerpApiImageSearchService(_config.SerpAPI, httpClient);
                            _logger.LogInformation("SerpAPI service created successfully");
                            return service;
                        }
                        else
                        {
                            _logger.LogWarning("SerpAPI service not created: API key is empty or null");
                        }
                        break;
                        
                    default:
                        _logger.LogWarning($"Unknown provider: {provider}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create image search service for provider: {provider}");
            }

            return null;
        }
    }

    // Fallback service that returns default images
    public class DefaultImageSearchService : IImageSearchService
    {
        public Task<string> SearchAndDownloadImageAsync(string searchQuery, bool isAnswer = false)
        {
            var defaultImage = isAnswer ? "/images/defaults/answer-placeholder.png" : "/images/defaults/question-placeholder.png";
            return Task.FromResult(defaultImage);
        }
    }
} 