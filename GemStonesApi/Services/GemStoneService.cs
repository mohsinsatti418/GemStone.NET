using GemStonesApi.Interfaces;
using GemStonesApi.Maps;
using GemStonesApi.Models;
using GemStonesApi.ViewModels;

namespace GemStonesApi.Services
{
    public class GemStoneService : IGemStoneService
    {
        private readonly IGemStoneRepository _repository;
        private readonly IAzureBlobService _blobService;
        private readonly string _containerName;

        public GemStoneService(
            IGemStoneRepository repository,
            IAzureBlobService blobService,
            IConfiguration config)
        {
            _repository = repository;
            _blobService = blobService;
            _containerName = config["AzureBlob:ContainerName"];
        }

        public async Task<int> CreateGemStoneAsync(
            GemStoneCreateVM viewModel,
            List<IFormFile> images,
            int thumbnailIndex)
        {
            // 1. Map ViewModel → Model
            var gemStone = GemStoneMap.ToModel(viewModel);

            // 2. Save gemstone, get new Id
            var newId = await _repository.CreateGemStoneAsync(gemStone);

            // 3. Upload each image to Azure Blob
            if (images != null && images.Count > 0)
            {
                var allowedTypes = new[]
                {
                    "image/jpeg", "image/png",
                    "image/webp", "image/gif"
                };

                for (int i = 0; i < images.Count; i++)
                {
                    var file = images[i];

                    // Security: validate content type
                    if (!allowedTypes.Contains(
                        file.ContentType.ToLower()))
                        continue;

                    // Security: max 5 MB
                    if (file.Length > 5 * 1024 * 1024)
                        continue;

                    // Upload to Azure — get back public URL
                    var imageUrl = await _blobService
                        .UploadImageAsync(file, _containerName);

                    var image = new GemStoneImage
                    {
                        GemStoneId = newId,
                        ImageUrl = imageUrl,
                        ImageName = file.FileName,
                        IsThumbnail = (i == thumbnailIndex),
                        DisplayOrder = i + 1,
                        UploadedAt = DateTime.UtcNow
                    };

                    await _repository.CreateGemStoneImageAsync(image);
                }
            }

            return newId;
        }

        public async Task<IEnumerable<GemStoneListVM>>
            GetAllGemStonesAsync()
        {
            return await _repository.GetAllGemStonesAsync();
        }

        public async Task<GemStoneDetailVM>
            GetGemStoneByIdAsync(int id)
        {
            return await _repository.GetGemStoneByIdAsync(id);
        }

        public async Task UpdateGemStoneAsync(
            GemStoneUpdateVM viewModel,
            List<IFormFile> newImages)
        {
            // 1. Map and update stone details
            var gemStone = GemStoneMap.ToModel(viewModel);
            await _repository.UpdateGemStoneAsync(gemStone);

            // 2. Delete selected images from Azure AND database
            if (viewModel.DeleteImageIds != null
                && viewModel.DeleteImageIds.Count > 0)
            {
                // Get current stone to find image URLs
                var existing = await _repository
                    .GetGemStoneByIdAsync(viewModel.Id);

                foreach (var imageId in viewModel.DeleteImageIds)
                {
                    // Find the URL of this image
                    var imageToDelete = existing?.Images?
                        .FirstOrDefault(img => img.Id == imageId);

                    if (imageToDelete != null)
                    {
                        // Delete from Azure first
                        await _blobService.DeleteImageAsync(
                            imageToDelete.ImageUrl,
                            _containerName);
                    }

                    // Delete from database
                    await _repository.DeleteGemStoneImageAsync(
                        imageId, viewModel.Id);
                }
            }

            // 3. Upload new images to Azure
            if (newImages != null && newImages.Count > 0)
            {
                var allowedTypes = new[]
                {
                    "image/jpeg", "image/png",
                    "image/webp", "image/gif"
                };

                var existingStone = await _repository
                    .GetGemStoneByIdAsync(viewModel.Id);
                var startOrder = existingStone?.Images?.Count ?? 0;

                for (int i = 0; i < newImages.Count; i++)
                {
                    var file = newImages[i];

                    if (!allowedTypes.Contains(
                        file.ContentType.ToLower()))
                        continue;

                    if (file.Length > 5 * 1024 * 1024)
                        continue;

                    var imageUrl = await _blobService
                        .UploadImageAsync(file, _containerName);

                    var image = new GemStoneImage
                    {
                        GemStoneId = viewModel.Id,
                        ImageUrl = imageUrl,
                        ImageName = file.FileName,
                        IsThumbnail = false,
                        DisplayOrder = startOrder + i + 1,
                        UploadedAt = DateTime.UtcNow
                    };

                    await _repository.CreateGemStoneImageAsync(image);
                }
            }

            // 4. Change thumbnail if requested
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