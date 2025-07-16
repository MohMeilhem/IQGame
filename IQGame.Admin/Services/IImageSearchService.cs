using System.Threading.Tasks;

namespace IQGame.Admin.Services
{
    public interface IImageSearchService
    {
        Task<string> SearchAndDownloadImageAsync(string searchQuery, bool isAnswer = false);
    }
}
