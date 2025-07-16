using System.Net.Http;
using System.Text.Json;
using System.Web;

namespace IQGame.Admin.Services
{
    public class GoogleCseImageSearchService : IImageSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleCseConfig _config;

        public GoogleCseImageSearchService(GoogleCseConfig config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> SearchAndDownloadImageAsync(string searchQuery, bool isAnswer = false)
        {
            try
            {
                var encodedQuery = HttpUtility.UrlEncode(searchQuery);
                var requestUrl = $"https://www.googleapis.com/customsearch/v1?key={_config.ApiKey}&cx={_config.SearchEngineId}&searchType=image&q={encodedQuery}&lr=lang_ar&gl=sa";

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                if (!doc.RootElement.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
                    return "/images/defaults/question-placeholder.png";

                var imageUrl = items[0].GetProperty("link").GetString();
                if (string.IsNullOrEmpty(imageUrl))
                    return "/images/defaults/question-placeholder.png";

                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                var fileName = $"{Guid.NewGuid()}.jpg";
                var folderName = isAnswer ? "answers" : "questions";

                var absolutePath = Path.Combine("C:\\Project\\IQGame\\IQGame\\wwwroot", "images", folderName);
                Console.WriteLine($"[GoogleCSE] Saving image to: {absolutePath}");

                Directory.CreateDirectory(absolutePath);
                var filePath = Path.Combine(absolutePath, fileName);

                await File.WriteAllBytesAsync(filePath, imageBytes);
                return $"/images/{folderName}/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GoogleCSE] Image fetch failed: {ex.Message}");
                return "/images/defaults/question-placeholder.png";
            }
        }
    }
}
