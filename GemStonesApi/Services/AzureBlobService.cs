using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GemStonesApi.Interfaces;

namespace GemStonesApi.Services
{
    public class AzureBlobService : IAzureBlobService
    {
        private readonly string _connectionString;

        public AzureBlobService(IConfiguration config)
        {
            _connectionString = config["AzureBlob:ConnectionString"];
        }

        public async Task<string> UploadImageAsync(
            IFormFile file,
            string containerName)
        {
            // Connect to the container
            var containerClient = new BlobContainerClient(
                _connectionString, containerName);

            // Create container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync(
                PublicAccessType.Blob);

            // Generate a unique filename — never use original name
            var extension = Path.GetExtension(file.FileName).ToLower();
            var blobName = $"{Guid.NewGuid()}{extension}";

            // Get a reference to the blob
            var blobClient = containerClient.GetBlobClient(blobName);

            // Set the correct content type so browser renders image
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            // Upload the file stream directly to Azure
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });

            // Return the full public URL
            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(
            string imageUrl,
            string containerName)
        {
            // Extract just the blob name from the full URL
            // URL looks like: https://account.blob.core.windows.net/container/filename.jpg
            var uri = new Uri(imageUrl);
            var blobName = Path.GetFileName(uri.LocalPath);

            var containerClient = new BlobContainerClient(
                _connectionString, containerName);

            var blobClient = containerClient.GetBlobClient(blobName);

            // Delete if exists — no error if already gone
            await blobClient.DeleteIfExistsAsync();
        }
    }
}