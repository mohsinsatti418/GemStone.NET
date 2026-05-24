namespace GemStonesApi.Interfaces
{
    public interface IAzureBlobService
    {
        Task<string> UploadImageAsync(
            IFormFile file,
            string containerName);

        Task DeleteImageAsync(
            string imageUrl,
            string containerName);
    }
}