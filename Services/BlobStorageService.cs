using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ABCRetailApp.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(
            string connectionString,
            string containerName,
            ILogger<BlobStorageService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _containerClient = new BlobContainerClient(connectionString, containerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task UploadBlobAsync(string fileName, Stream fileStream)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            try
            {
                _logger.LogInformation("Uploading blob {FileName}.", fileName);
                await blobClient.UploadAsync(fileStream, overwrite: true);
                _logger.LogInformation("Blob {FileName} uploaded successfully.", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading blob {FileName}. Error: {ErrorMessage}", fileName, ex.Message);
                throw;
            }
        }

        public string GetBlobUrl(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }
    }
}
