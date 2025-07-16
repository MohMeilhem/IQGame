using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using IQGame.Application.Models.Session;
using IQGame.Application.Models;
using IQGame.Shared.Models;
using IQGame.Client.Models;
using System.Text.Json;

public class SessionService
{
    private readonly HttpClient _http;

    public SessionService(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, string? Error, int? SessionId)> CreateSessionAsync(CreateSessionRequest request)
    {
        try
        {
            Console.WriteLine($"🚀 Sending request to create session with name: {request.SessionName}");
            var response = await _http.PostAsJsonAsync("api/sessions", request);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📝 Raw API Response: {responseContent}");
            Console.WriteLine($"📝 Response Status Code: {response.StatusCode}");
            Console.WriteLine($"📝 Response Content Type: {response.Content.Headers.ContentType}");
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // Parse the full session response
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var sessionResponse = await response.Content.ReadFromJsonAsync<SessionResponse>(options);
                    if (sessionResponse != null && sessionResponse.SessionId > 0)
                    {
                        Console.WriteLine($"✅ Session created successfully with ID: {sessionResponse.SessionId}");
                        return (true, null, sessionResponse.SessionId);
                    }
                    else
                    {
                        var error = "Invalid or null session response from server";
                        Console.WriteLine($"❌ {error}");
                        return (false, error, null);
                    }
                }
                catch (Exception ex)
                {
                    var error = $"Could not parse session response: {ex.Message}";
                    Console.WriteLine($"❌ {error}");
                    return (false, error, null);
                }
            }
            
            var errorMessage = $"Failed to create session. Status: {response.StatusCode}, Response: {responseContent}";
            Console.WriteLine($"❌ {errorMessage}");
            return (false, errorMessage, null);
        }
        catch (Exception ex)
        {
            var error = $"Error creating session: {ex.Message}";
            Console.WriteLine($"❌ {error}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            return (false, error, null);
        }
    }

    private class SessionResponse
    {
        public int SessionId { get; set; }
        public string SessionName { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public List<int> CategoryIds { get; set; }
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        try
        {
            Console.WriteLine($"🔍 [SessionService] Attempting to fetch categories from: {_http.BaseAddress}api/categories");
            
            var response = await _http.GetAsync("api/categories");
            Console.WriteLine($"📡 [SessionService] Response Status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ [SessionService] API Error: {errorContent}");
                throw new Exception($"API returned {response.StatusCode}: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📦 [SessionService] Raw Response: {json}");

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = System.Text.Json.JsonSerializer.Deserialize<List<Category>>(json, options);

            // Update image URLs to use the API base address
            if (parsed != null)
            {
                foreach (var category in parsed)
                {
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        // If the ImageUrl is a relative path, make it absolute
                        if (!category.ImageUrl.StartsWith("http"))
                        {
                            category.ImageUrl = $"{_http.BaseAddress}{category.ImageUrl.TrimStart('/')}";
                        }
                    }
                }
            }

            Console.WriteLine($"✅ [SessionService] Successfully parsed {parsed?.Count ?? 0} categories");
            return parsed ?? new List<Category>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ [SessionService] Network Error: {ex.Message}");
            Console.WriteLine($"❌ [SessionService] Inner Exception: {ex.InnerException?.Message}");
            throw new Exception("Failed to connect to the API server. Please ensure the server is running.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SessionService] Error: {ex.Message}");
            Console.WriteLine($"❌ [SessionService] Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        try
        {
            Console.WriteLine($"🔍 [SessionService] Attempting to fetch category by ID: {id} from: {_http.BaseAddress}api/categories/{id}");
            var category = await _http.GetFromJsonAsync<Category>($"api/categories/{id}");
            if (category != null && !string.IsNullOrEmpty(category.ImageUrl) && !category.ImageUrl.StartsWith("http"))
            {
                category.ImageUrl = $"{_http.BaseAddress}{category.ImageUrl.TrimStart('/')}";
            }
            Console.WriteLine($"✅ [SessionService] Successfully retrieved category by ID: {id}");
            return category;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ [SessionService] Network Error getting category by ID {id}: {ex.Message}");
            throw new Exception("Failed to connect to the API server when getting category description.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SessionService] Error getting category by ID {id}: {ex.Message}");
            Console.WriteLine($"❌ [SessionService] Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<List<Question>> GetCategoryQuestionsAsync(int categoryId)
    {
        try
        {
            Console.WriteLine($"🔍 [SessionService] Attempting to fetch questions for category ID: {categoryId}");
            var questions = await _http.GetFromJsonAsync<List<Question>>($"api/categories/{categoryId}/questions");
            Console.WriteLine($"✅ [SessionService] Successfully retrieved {questions?.Count ?? 0} questions for category ID: {categoryId}");
            return questions ?? new List<Question>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ [SessionService] Network Error getting questions for category {categoryId}: {ex.Message}");
            throw new Exception("Failed to connect to the API server when getting category questions.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SessionService] Error getting questions for category {categoryId}: {ex.Message}");
            Console.WriteLine($"❌ [SessionService] Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<List<CategoryWithQuestionsViewModel>> GetSessionQuestionsAsync(int sessionId)
    {
        Console.WriteLine($"[SessionService.Client] Fetching questions for Session ID: {sessionId} from URL: api/sessions/{sessionId}/questions");
        var result = await _http.GetFromJsonAsync<List<CategoryWithQuestionsViewModel>>($"api/sessions/{sessionId}/questions");
        
        if (result != null)
        {
            foreach (var category in result)
            {
                // Fix category image URL
                if (!string.IsNullOrEmpty(category.CategoryImageUrl) && !category.CategoryImageUrl.StartsWith("http"))
                {
                    category.CategoryImageUrl = $"{_http.BaseAddress}{category.CategoryImageUrl.TrimStart('/')}";
                }

                // Fix question image URLs
                if (category.Questions != null)
                {
                    foreach (var question in category.Questions)
                    {
                        if (!string.IsNullOrEmpty(question.ImageUrl) && !question.ImageUrl.StartsWith("http"))
                        {
                            question.ImageUrl = $"{_http.BaseAddress}{question.ImageUrl.TrimStart('/')}";
                        }

                        // Fix answer image URLs
                        if (question.Answers != null)
                        {
                            foreach (var answer in question.Answers)
                            {
                                if (!string.IsNullOrEmpty(answer.ImageUrl) && !answer.ImageUrl.StartsWith("http"))
                                {
                                    answer.ImageUrl = $"{_http.BaseAddress}{answer.ImageUrl.TrimStart('/')}";
                                }
                            }
                        }
                    }
                }
            }
        }

        return result ?? new List<CategoryWithQuestionsViewModel>();
    }

    public async Task<SessionResultViewModel> GetSessionResultsAsync(int sessionId)
    {
        try
        {
            Console.WriteLine($"🔍 [SessionService] Getting session results for SessionId: {sessionId}");
            
            var result = await _http.GetFromJsonAsync<SessionResultViewModel>($"api/sessions/{sessionId}/results");
            
            if (result != null)
            {
                Console.WriteLine($"✅ [SessionService] Session results - Team1: {result.Team1?.Name} (Score: {result.Team1?.Score}), Team2: {result.Team2?.Name} (Score: {result.Team2?.Score}), Winner: {result.Winner}");
            }
            else
            {
                Console.WriteLine($"❌ [SessionService] Session results returned null");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SessionService] GetSessionResultsAsync exception: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> ScoreQuestionAsync(ScoreQuestionRequest request)
    {
        try
        {
            Console.WriteLine($"🔍 [SessionService] Scoring question - SessionId: {request.SessionId}, QuestionId: {request.QuestionId}, TeamName: '{request.TeamName}'");
            
            var res = await _http.PostAsJsonAsync($"api/sessions/{request.SessionId}/score-question", request);
            
            Console.WriteLine($"🔍 [SessionService] ScoreQuestionAsync response status: {res.StatusCode}");
            
            if (!res.IsSuccessStatusCode)
            {
                var errorContent = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ [SessionService] ScoreQuestionAsync error: {errorContent}");
            }
            
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SessionService] ScoreQuestionAsync exception: {ex.Message}");
            return false;
        }
    }

    public async Task<GameStatusViewModel> GetGameStatusAsync(int sessionId)
    {
        try
        {
            Console.WriteLine($"🔍 [SessionService] Getting game status for SessionId: {sessionId}");
            
            var result = await _http.GetFromJsonAsync<GameStatusViewModel>($"api/sessions/{sessionId}/status");
            
            if (result != null)
            {
                Console.WriteLine($"✅ [SessionService] Game status - GameFinished: {result.GameFinished}, TotalQuestionsAnswered: {result.TotalQuestionsAnswered}");
                Console.WriteLine($"✅ [SessionService] Team scores - Team1: {result.Team1Name} (Score: {result.Team1Score}), Team2: {result.Team2Name} (Score: {result.Team2Score})");
            }
            else
            {
                Console.WriteLine($"❌ [SessionService] Game status returned null");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SessionService] GetGameStatusAsync exception: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetCurrentTurnAsync(int sessionId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<CurrentTurnResponse>($"api/sessions/{sessionId}/current-turn");
            return response?.currentTurn;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error getting current turn: {ex.Message}");
            return null;
        }
    }

    private class CurrentTurnResponse
    {
        public string? currentTurn { get; set; }
    }

    public async Task<bool> ChangeTurnAsync(int sessionId, string teamName)
    {
        var res = await _http.PostAsJsonAsync($"api/sessions/{sessionId}/change-turn?teamName={teamName}", "");
        return res.IsSuccessStatusCode;
    }
    public async Task<bool> UseHelpAsync(int sessionId, string teamName, string helpType, int? questionId = null)
    {
        var res = await _http.PostAsJsonAsync("api/help/use", new
        {
            sessionId,
            teamName,
            helpType,
            questionId
        });

        return res.IsSuccessStatusCode;
    }

    public async Task<bool> EndGameAsync(int sessionId)
    {
        try
        {
            var response = await _http.PostAsync($"api/sessions/{sessionId}/end", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error ending game: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateTeamScoreAsync(int sessionId, string team, int change)
    {
        try
        {
            var request = new UpdateScoreRequest
            {
                Team = team,
                ScoreChange = change
            };
            
            var response = await _http.PostAsJsonAsync($"api/sessions/{sessionId}/update-score", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error updating team score: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ResetSessionAsync(int sessionId, string sessionName, string team1Name, string team2Name)
    {
        try
        {
            var request = new ResetSessionRequest
            {
                SessionName = sessionName,
                Team1Name = team1Name,
                Team2Name = team2Name
            };
            
            var response = await _http.PostAsJsonAsync($"api/sessions/{sessionId}/reset", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error resetting session: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Answer>> GetAnswersForQuestionAsync(int questionId)
    {
        try
        {
            Console.WriteLine($"🔍 [SessionService] Attempting to fetch answers for question ID: {questionId}");
            var answers = await _http.GetFromJsonAsync<List<Answer>>($"api/answers/question/{questionId}");
            
            if (answers != null)
            {
                // Fix answer image URLs
                foreach (var answer in answers)
                {
                    if (!string.IsNullOrEmpty(answer.ImageUrl) && !answer.ImageUrl.StartsWith("http"))
                    {
                        answer.ImageUrl = $"{_http.BaseAddress}{answer.ImageUrl.TrimStart('/')}";
                    }
                }
            }
            
            Console.WriteLine($"✅ [SessionService] Successfully retrieved {answers?.Count ?? 0} answers for question ID: {questionId}");
            return answers ?? new List<Answer>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ [SessionService] Network Error getting answers for question {questionId}: {ex.Message}");
            throw new Exception("Failed to connect to the API server when getting question answers.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SessionService] Error getting answers for question {questionId}: {ex.Message}");
            Console.WriteLine($"❌ [SessionService] Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<List<Group>> GetAllGroupsAsync()
    {
        try
        {
            Console.WriteLine($"🔍 [SessionService] Attempting to fetch groups from: {_http.BaseAddress}api/groups");
            
            var response = await _http.GetAsync("api/groups");
            Console.WriteLine($"📡 [SessionService] Groups Response Status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ [SessionService] Groups API Error: {errorContent}");
                throw new Exception($"API returned {response.StatusCode}: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📦 [SessionService] Groups Raw Response: {json}");

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = System.Text.Json.JsonSerializer.Deserialize<List<Group>>(json, options);
            Console.WriteLine($"✅ [SessionService] Successfully parsed {parsed?.Count ?? 0} groups");
            return parsed ?? new List<Group>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ [SessionService] Network Error getting groups: {ex.Message}");
            throw new Exception("Failed to connect to the API server when getting groups.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SessionService] Error getting groups: {ex.Message}");
            Console.WriteLine($"❌ [SessionService] Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<List<Category>> GetCategoriesByGroupAsync(int groupId)
    {
        try
        {
            Console.WriteLine($"🔍 [SessionService] Attempting to fetch categories for group ID: {groupId}");
            var categories = await _http.GetFromJsonAsync<List<Category>>($"api/groups/{groupId}/categories");
            
            if (categories != null)
            {
                foreach (var category in categories)
                {
                    if (!string.IsNullOrEmpty(category.ImageUrl) && !category.ImageUrl.StartsWith("http"))
                    {
                        category.ImageUrl = $"{_http.BaseAddress}{category.ImageUrl.TrimStart('/')}";
                    }
                }
            }
            
            Console.WriteLine($"✅ [SessionService] Successfully retrieved {categories?.Count ?? 0} categories for group ID: {groupId}");
            return categories ?? new List<Category>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ [SessionService] Network Error getting categories for group {groupId}: {ex.Message}");
            throw new Exception("Failed to connect to the API server when getting group categories.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SessionService] Error getting categories for group {groupId}: {ex.Message}");
            Console.WriteLine($"❌ [SessionService] Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<List<CategoryAvailability>> GetCategoriesAvailabilityAsync()
    {
        try
        {
            Console.WriteLine($"🔍 [SessionService] Attempting to fetch categories availability from: {_http.BaseAddress}api/sessions/categories/availability");
            
            var response = await _http.GetAsync("api/sessions/categories/availability");
            Console.WriteLine($"📡 [SessionService] Categories Availability Response Status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ [SessionService] Categories Availability API Error: {errorContent}");
                throw new Exception($"API returned {response.StatusCode}: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📦 [SessionService] Categories Availability Raw Response: {json}");

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = System.Text.Json.JsonSerializer.Deserialize<List<CategoryAvailability>>(json, options);
            
            // Update image URLs to use the API base address
            if (parsed != null)
            {
                foreach (var availability in parsed)
                {
                    if (!string.IsNullOrEmpty(availability.CategoryImageUrl) && !availability.CategoryImageUrl.StartsWith("http"))
                    {
                        availability.CategoryImageUrl = $"{_http.BaseAddress}{availability.CategoryImageUrl.TrimStart('/')}";
                    }
                }
            }

            Console.WriteLine($"✅ [SessionService] Successfully parsed {parsed?.Count ?? 0} category availabilities");
            return parsed ?? new List<CategoryAvailability>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ [SessionService] Network Error getting categories availability: {ex.Message}");
            throw new Exception("Failed to connect to the API server when getting categories availability.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SessionService] Error getting categories availability: {ex.Message}");
            Console.WriteLine($"❌ [SessionService] Stack Trace: {ex.StackTrace}");
            throw;
        }
    }
}

public class UpdateScoreRequest
{
    public string Team { get; set; }
    public int ScoreChange { get; set; }
}

public class ResetSessionRequest
{
    public string SessionName { get; set; }
    public string Team1Name { get; set; }
    public string Team2Name { get; set; }
}
