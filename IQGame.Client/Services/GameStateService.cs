using System;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;

namespace IQGame.Client.Services
{
    public class GameStateService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string STORAGE_KEY = "game_state";
        public event Action OnChange;

        public int SessionId { get; set; }
        public string SessionName { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public string CurrentTurn { get; set; }

        public GameStateService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task UpdateSessionAsync(int sessionId, string sessionName, string team1, string team2)
        {
            SessionId = sessionId;
            SessionName = sessionName;
            Team1 = team1;
            Team2 = team2;
            CurrentTurn = team1; // Default to first team
            await SaveStateAsync();
            NotifyStateChanged();
        }

        public async Task ClearSessionAsync()
        {
            SessionId = 0;
            SessionName = null;
            Team1 = null;
            Team2 = null;
            CurrentTurn = null;
            await SaveStateAsync();
            NotifyStateChanged();
        }

        public async Task UpdateCurrentTurnAsync(string team)
        {
            if (team != Team1 && team != Team2)
                throw new ArgumentException("Invalid team name");

            CurrentTurn = team;
            await SaveStateAsync();
            NotifyStateChanged();
        }

        public async Task SaveAsync()
        {
            await SaveStateAsync();
            NotifyStateChanged();
        }

        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("🔄 GameStateService: Initializing...");
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
                if (!string.IsNullOrEmpty(json))
                {
                    var state = JsonSerializer.Deserialize<GameState>(json);
                    if (state != null)
                    {
                        SessionId = state.SessionId;
                        Team1 = state.Team1;
                        Team2 = state.Team2;
                        CurrentTurn = state.CurrentTurn;
                        SessionName = state.SessionName;
                        Console.WriteLine($"✅ GameStateService: Loaded state - SessionId: {SessionId}, Team1: {Team1}, Team2: {Team2}");
                        NotifyStateChanged();
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️ GameStateService: No saved state found");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Error loading game state: {ex.Message}");
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        private async Task SaveStateAsync()
        {
            try
            {
                var state = new GameState
                {
                    SessionId = SessionId,
                    Team1 = Team1,
                    Team2 = Team2,
                    CurrentTurn = CurrentTurn,
                    SessionName = SessionName
                };
                var json = JsonSerializer.Serialize(state);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", STORAGE_KEY, json);
                Console.WriteLine($"💾 GameStateService: Saved state - SessionId: {SessionId}, Team1: {Team1}, Team2: {Team2}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Error saving game state: {ex.Message}");
            }
        }

        private class GameState
        {
            public int SessionId { get; set; }
            public string Team1 { get; set; }
            public string Team2 { get; set; }
            public string CurrentTurn { get; set; }
            public string SessionName { get; set; }
        }
    }
}
