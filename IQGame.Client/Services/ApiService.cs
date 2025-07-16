using System.Net.Http.Json;
using IQGame.Shared.Models;

namespace IQGame.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Categories
        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Category>>("api/categories");
                return response ?? new List<Category>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching categories: {ex.Message}");
                return new List<Category>();
            }
        }

        public async Task<Category?> GetCategoryAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Category>($"api/categories/{id}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching category {id}: {ex.Message}");
                return null;
            }
        }

        // Questions
        public async Task<List<Question>> GetQuestionsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Question>>("api/questions");
                return response ?? new List<Question>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching questions: {ex.Message}");
                return new List<Question>();
            }
        }

        public async Task<Question?> GetQuestionAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Question>($"api/questions/{id}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching question {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Question>> GetQuestionsByCategoryAsync(int categoryId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Question>>($"api/questions/category/{categoryId}");
                return response ?? new List<Question>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching questions for category {categoryId}: {ex.Message}");
                return new List<Question>();
            }
        }

        // Answers
        public async Task<List<Answer>> GetAnswersForQuestionAsync(int questionId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Answer>>($"api/answers/question/{questionId}");
                return response ?? new List<Answer>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching answers for question {questionId}: {ex.Message}");
                return new List<Answer>();
            }
        }
    }
} 