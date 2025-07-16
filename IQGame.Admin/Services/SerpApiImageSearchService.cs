using System.Net.Http;
using System.Text.Json;
using System.Web;

namespace IQGame.Admin.Services
{
    public class SerpApiImageSearchService : IImageSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly SerpApiConfig _config;

        public SerpApiImageSearchService(SerpApiConfig config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> SearchAndDownloadImageAsync(string searchQuery, bool isAnswer = false)
        {
            try
            {
                Console.WriteLine($"[SerpAPI] Starting search for: {searchQuery}");
                Console.WriteLine($"[SerpAPI] API Key: {_config.ApiKey?.Substring(0, 10)}...");
                
                // Test the API key first with a simple search
                var testUrl = $"https://serpapi.com/search.json?engine=google&q=test&api_key={_config.ApiKey}";
                Console.WriteLine($"[SerpAPI] Testing API with URL: {testUrl}");
                
                var testResponse = await _httpClient.GetAsync(testUrl);
                Console.WriteLine($"[SerpAPI] Test response status: {testResponse.StatusCode}");
                
                if (!testResponse.IsSuccessStatusCode)
                {
                    var errorContent = await testResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"[SerpAPI] API test failed: {errorContent}");
                    return "/images/defaults/question-placeholder.png";
                }
                
                // Now try the actual image search
                var encodedQuery = HttpUtility.UrlEncode(searchQuery);
                var requestUrl = $"https://serpapi.com/search.json?engine=google&q={encodedQuery}&tbm=isch&api_key={_config.ApiKey}";
                
                Console.WriteLine($"[SerpAPI] Image search URL: {requestUrl}");

                var response = await _httpClient.GetAsync(requestUrl);
                Console.WriteLine($"[SerpAPI] Image search response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[SerpAPI] Image search error: {errorContent}");
                    return "/images/defaults/question-placeholder.png";
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[SerpAPI] Response content length: {content.Length}");
                Console.WriteLine($"[SerpAPI] Response preview: {content.Substring(0, Math.Min(500, content.Length))}");
                
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                // Log the response structure for debugging
                Console.WriteLine($"[SerpAPI] Response keys: {string.Join(", ", root.EnumerateObject().Select(p => p.Name))}");

                // Try different possible response structures
                string? imageUrl = null;
                
                // Try images_results first (most common)
                if (root.TryGetProperty("images_results", out var imagesResults))
                {
                    Console.WriteLine($"[SerpAPI] Found images_results with {imagesResults.GetArrayLength()} items");
                    if (imagesResults.GetArrayLength() > 0)
                    {
                        var firstImage = imagesResults[0];
                        Console.WriteLine($"[SerpAPI] First image keys: {string.Join(", ", firstImage.EnumerateObject().Select(p => p.Name))}");
                        
                        if (firstImage.TryGetProperty("original", out var original))
                        {
                            imageUrl = original.GetString();
                            Console.WriteLine($"[SerpAPI] Found original URL: {imageUrl}");
                        }
                        else if (firstImage.TryGetProperty("link", out var link))
                        {
                            imageUrl = link.GetString();
                            Console.WriteLine($"[SerpAPI] Found link URL: {imageUrl}");
                        }
                        else if (firstImage.TryGetProperty("thumbnail", out var thumbnail))
                        {
                            imageUrl = thumbnail.GetString();
                            Console.WriteLine($"[SerpAPI] Found thumbnail URL: {imageUrl}");
                        }
                    }
                }
                // Try organic_results as fallback
                else if (root.TryGetProperty("organic_results", out var organicResults))
                {
                    Console.WriteLine($"[SerpAPI] Found organic_results with {organicResults.GetArrayLength()} items");
                    if (organicResults.GetArrayLength() > 0)
                    {
                        var firstResult = organicResults[0];
                        if (firstResult.TryGetProperty("thumbnail", out var thumbnail))
                        {
                            imageUrl = thumbnail.GetString();
                            Console.WriteLine($"[SerpAPI] Found organic thumbnail URL: {imageUrl}");
                        }
                    }
                }

                if (string.IsNullOrEmpty(imageUrl))
                {
                    Console.WriteLine($"[SerpAPI] No image URL found in response");
                    return "/images/defaults/question-placeholder.png";
                }

                Console.WriteLine($"[SerpAPI] Downloading image from: {imageUrl}");

                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                var fileName = $"{Guid.NewGuid()}.jpg";
                var folderName = isAnswer ? "answers" : "questions";

                var absolutePath = Path.Combine("C:\\Project\\IQGame\\IQGame\\wwwroot", "images", folderName);
                Console.WriteLine($"[SerpAPI] Saving image to: {absolutePath}");

                Directory.CreateDirectory(absolutePath);
                var filePath = Path.Combine(absolutePath, fileName);

                await File.WriteAllBytesAsync(filePath, imageBytes);
                Console.WriteLine($"[SerpAPI] Image saved successfully: {fileName}");
                return $"/images/{folderName}/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SerpAPI] Image fetch failed: {ex.Message}");
                Console.WriteLine($"[SerpAPI] Stack trace: {ex.StackTrace}");
                return "/images/defaults/question-placeholder.png";
            }
        }
    }
} 