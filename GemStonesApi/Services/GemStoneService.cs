using GemStonesApi.Interfaces;
using GemStonesApi.Maps;
using GemStonesApi.Models;
using GemStonesApi.ViewModels;
using Microsoft.AspNetCore.Http;

namespace GemStonesApi.Services
{
    public class GemStoneService : IGemStoneService
    {
        private readonly IGemStoneRepository _repository;
        private readonly IWebHostEnvironment _env;

        public GemStoneService(
            IGemStoneRepository repository,
            IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }

        public async Task<int> CreateGemStoneAsync(
            GemStoneCreateVM viewModel,
            List<IFormFile> images,
            int thumbnailIndex)
        {
            // 1. Map ViewModel → Model
            var gemStone = GemStoneMap.ToModel(viewModel);

            // 2. Save the gemstone, get back its new Id
            var newId = await _repository.CreateGemStoneAsync(gemStone);

            // 3. Save each image to disk and record it in DB
            if (images != null && images.Count > 0)
            {
                // Security: create uploads folder if it doesn't exist
                var uploadsFolder = Path.Combine(
                    _env.WebRootPath, "uploads", "gemstones");
                Directory.CreateDirectory(uploadsFolder);

                for (int i = 0; i < images.Count; i++)
                {
                    var file = images[i];

                    // Security: validate file is actually an image
                    var allowedTypes = new[] {
                        "image/jpeg", "image/png",
                        "image/webp", "image/gif"
                    };
                    if (!allowedTypes.Contains(file.ContentType.ToLower()))
                        continue;

                    // Security: limit file size to 5 MB
                    if (file.Length > 5 * 1024 * 1024)
                        continue;

                    // Security: generate a new random filename
                    // Never use the original filename from the client
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    var safeFile = $"{Guid.NewGuid()}{extension}";
                    var fullPath = Path.Combine(uploadsFolder, safeFile);
                    var relativeUrl = $"/uploads/gemstones/{safeFile}";

                    // Write the file to disk
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Build the image record
                    var image = new GemStoneImage
                    {
                        GemStoneId = newId,
                        ImageUrl = relativeUrl,
                        ImageName = file.FileName, // original name for display only
                        IsThumbnail = (i == thumbnailIndex),
                        DisplayOrder = i + 1,
                        UploadedAt = DateTime.UtcNow
                    };

                    await _repository.CreateGemStoneImageAsync(image);
                }
            }

            return newId;
        }

        public async Task<IEnumerable<GemStoneListVM>> GetAllGemStonesAsync()
        {
            return await _repository.GetAllGemStonesAsync();
        }

        public async Task<GemStoneDetailVM> GetGemStoneByIdAsync(int id)
        {
            return await _repository.GetGemStoneByIdAsync(id);
        }

        public async Task UpdateGemStoneAsync(
    GemStoneUpdateVM viewModel,
    List<IFormFile> newImages)
        {
            // 1. Map ViewModel → Model
            var gemStone = GemStoneMap.ToModel(viewModel);

            // 2. Update the stone details
            await _repository.UpdateGemStoneAsync(gemStone);

            // 3. Delete images the user flagged for removal
            if (viewModel.DeleteImageIds != null
                && viewModel.DeleteImageIds.Count > 0)
            {
                foreach (var imageId in viewModel.DeleteImageIds)
                {
                    // Also delete the file from disk
                    // First get the image details to find its path
                    // (we handle this simply — delete from DB,
                    // file cleanup can be a background job)
                    await _repository.DeleteGemStoneImageAsync(
                        imageId, viewModel.Id);
                }
            }

            // 4. Upload any new images that were added
            if (newImages != null && newImages.Count > 0)
            {
                var uploadsFolder = Path.Combine(
                    _env.WebRootPath, "uploads", "gemstones");
                Directory.CreateDirectory(uploadsFolder);

                // Find current highest display order
                var existingStone = await _repository
                    .GetGemStoneByIdAsync(viewModel.Id);
                var startOrder = existingStone?.Images?.Count ?? 0;

                for (int i = 0; i < newImages.Count; i++)
                {
                    var file = newImages[i];

                    var allowedTypes = new[]
                    {
                "image/jpeg", "image/png",
                "image/webp", "image/gif"
            };

                    if (!allowedTypes.Contains(file.ContentType.ToLower()))
                        continue;

                    if (file.Length > 5 * 1024 * 1024)
                        continue;

                    var extension = Path.GetExtension(file.FileName).ToLower();
                    var safeFile = $"{Guid.NewGuid()}{extension}";
                    var fullPath = Path.Combine(uploadsFolder, safeFile);
                    var relativeUrl = $"/uploads/gemstones/{safeFile}";

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var image = new GemStoneImage
                    {
                        GemStoneId = viewModel.Id,
                        ImageUrl = relativeUrl,
                        ImageName = file.FileName,
                        IsThumbnail = false,
                        DisplayOrder = startOrder + i + 1,
                        UploadedAt = DateTime.UtcNow
                    };

                    await _repository.CreateGemStoneImageAsync(image);
                }
            }

            // 5. Change thumbnail if user selected a new one
            if (viewModel.NewThumbnailImageId.HasValue)
            {
                await _repository.SetThumbnailAsync(
                    viewModel.Id,
                    viewModel.NewThumbnailImageId.Value);
            }
        }

        public async Task DeleteGemStoneAsync(int id)
        {
            await _repository.DeleteGemStoneAsync(id);
        }

        public async Task RestoreGemStoneAsync(int id)
        {
            await _repository.RestoreGemStoneAsync(id);
        }
    }
}