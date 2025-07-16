namespace IQGame.Application.Models
{
    public class CreateSessionRequest
    {
        public string SessionName { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }

        public List<int> CategoryIds { get; set; } // 🆕 IDs of 6 selected categories
    }


}
