using GemStonesApi.ViewModels;
using Microsoft.AspNetCore.Http;

namespace GemStonesApi.Interfaces
{
    public interface IGemStoneService
    {
        Task<int> CreateGemStoneAsync(
            GemStoneCreateVM viewModel,
            List<IFormFile> images,
            int thumbnailIndex
        );

        Task<IEnumerable<GemStoneListVM>> GetAllGemStonesAsync();

        Task<GemStoneDetailVM> GetGemStoneByIdAsync(int id);

        Task UpdateGemStoneAsync(
            GemStoneUpdateVM viewModel,
            List<IFormFile> newImages
        );

        Task DeleteGemStoneAsync(int id);
        Task RestoreGemStoneAsync(int id);
    }
}