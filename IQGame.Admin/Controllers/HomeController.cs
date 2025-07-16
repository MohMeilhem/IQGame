using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IQGame.Admin.Models;
using IQGame.Admin.Services;

namespace IQGame.Admin.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Unregistered()
    {
        return View();
    }

    public async Task<IActionResult> ImageSearchStatus()
    {
        var switcher = HttpContext.RequestServices.GetRequiredService<ImageSearchProviderSwitcher>();
        var status = await switcher.GetProviderStatus();
        
        ViewBag.ProviderStatus = status;
        return View();
    }

    public async Task<IActionResult> TestSerpAPI()
    {
        try
        {
            // Get configuration for debugging
            var config = HttpContext.RequestServices.GetRequiredService<IOptions<ImageSearchConfiguration>>();
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            
            logger.LogInformation($"TestSerpAPI - PreferredProvider: {config.Value.PreferredProvider}");
            logger.LogInformation($"TestSerpAPI - SerpAPI Key Length: {config.Value.SerpAPI.ApiKey?.Length ?? 0}");
            logger.LogInformation($"TestSerpAPI - GoogleCSE Key Length: {config.Value.GoogleCSE.ApiKey?.Length ?? 0}");
            
            var imageService = HttpContext.RequestServices.GetRequiredService<IImageSearchService>();
            var result = await imageService.SearchAndDownloadImageAsync("cat", false);
            
            ViewBag.TestResult = result;
            ViewBag.ServiceType = imageService.GetType().Name;
            ViewBag.ConfigInfo = new
            {
                PreferredProvider = config.Value.PreferredProvider,
                SerpAPIConfigured = !string.IsNullOrEmpty(config.Value.SerpAPI.ApiKey),
                GoogleCSEConfigured = !string.IsNullOrEmpty(config.Value.GoogleCSE.ApiKey) && !string.IsNullOrEmpty(config.Value.GoogleCSE.SearchEngineId)
            };
            
            return View();
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            ViewBag.StackTrace = ex.StackTrace;
            return View();
        }
    }

    public async Task<IActionResult> TestGoogleCSE()
    {
        try
        {
            // Get configuration for debugging
            var config = HttpContext.RequestServices.GetRequiredService<IOptions<ImageSearchConfiguration>>();
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            
            logger.LogInformation($"TestGoogleCSE - PreferredProvider: {config.Value.PreferredProvider}");
            logger.LogInformation($"TestGoogleCSE - GoogleCSE Key Length: {config.Value.GoogleCSE.ApiKey?.Length ?? 0}");
            logger.LogInformation($"TestGoogleCSE - GoogleCSE SearchEngineId Length: {config.Value.GoogleCSE.SearchEngineId?.Length ?? 0}");
            logger.LogInformation($"TestGoogleCSE - SerpAPI Key Length: {config.Value.SerpAPI.ApiKey?.Length ?? 0}");
            
            // Create a specific Google CSE service for testing
            var factory = HttpContext.RequestServices.GetRequiredService<ImageSearchServiceFactory>();
            var googleCseService = factory.CreateService("GoogleCSE");
            
            if (googleCseService == null)
            {
                ViewBag.Error = "Google CSE service could not be created. Please check your configuration.";
                ViewBag.ConfigInfo = new
                {
                    PreferredProvider = config.Value.PreferredProvider,
                    SerpAPIConfigured = !string.IsNullOrEmpty(config.Value.SerpAPI.ApiKey),
                    GoogleCSEConfigured = !string.IsNullOrEmpty(config.Value.GoogleCSE.ApiKey) && !string.IsNullOrEmpty(config.Value.GoogleCSE.SearchEngineId)
                };
                return View();
            }
            
            var result = await googleCseService.SearchAndDownloadImageAsync("cat", false);
            
            ViewBag.TestResult = result;
            ViewBag.ServiceType = googleCseService.GetType().Name;
            ViewBag.ConfigInfo = new
            {
                PreferredProvider = config.Value.PreferredProvider,
                SerpAPIConfigured = !string.IsNullOrEmpty(config.Value.SerpAPI.ApiKey),
                GoogleCSEConfigured = !string.IsNullOrEmpty(config.Value.GoogleCSE.ApiKey) && !string.IsNullOrEmpty(config.Value.GoogleCSE.SearchEngineId),
                GoogleCSEApiKey = config.Value.GoogleCSE.ApiKey?.Substring(0, Math.Min(10, config.Value.GoogleCSE.ApiKey?.Length ?? 0)) + "...",
                GoogleCSESearchEngineId = config.Value.GoogleCSE.SearchEngineId?.Substring(0, Math.Min(10, config.Value.GoogleCSE.SearchEngineId?.Length ?? 0)) + "..."
            };
            
            return View();
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            ViewBag.StackTrace = ex.StackTrace;
            return View();
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
