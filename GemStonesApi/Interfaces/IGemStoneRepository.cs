using GemStonesApi.Models;
using GemStonesApi.ViewModels;

namespace GemStonesApi.Interfaces
{
    public interface IGemStoneRepository
    {
        Task<int> CreateGemStoneAsync(GemStone gemStone);
        Task CreateGemStoneImageAsync(GemStoneImage image);
        Task SetThumbnailAsync(int gemStoneId, int imageId);

        Task<IEnumerable<GemStoneListVM>> GetAllGemStonesAsync();

        Task<GemStoneDetailVM> GetGemStoneByIdAsync(int id);

        Task UpdateGemStoneAsync(GemStone gemStone);
        Task DeleteGemStoneImageAsync(int imageId, int gemStoneId);

        Task DeleteGemStoneAsync(int id);
        Task RestoreGemStoneAsync(int id);
    }
}