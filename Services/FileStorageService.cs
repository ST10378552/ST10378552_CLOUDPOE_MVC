using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using ABCRetailApp.Models;

namespace ABCRetailApp.Services
{
    public class FileStorageService
    {
        private readonly ShareClient _shareClient;
        private readonly ShareDirectoryClient _directoryClient;

        public FileStorageService(string storageConnectionString, string shareName)
        {
            var shareServiceClient = new ShareServiceClient(storageConnectionString);
            _shareClient = shareServiceClient.GetShareClient(shareName);
            _shareClient.CreateIfNotExists();
            _directoryClient = _shareClient.GetRootDirectoryClient();
        }

        public async Task UploadFileAsync(string fileName, Stream fileStream)
        {
            var fileClient = _directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadAsync(fileStream);
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var fileClient = _directoryClient.GetFileClient(fileName);
            var response = await fileClient.DownloadAsync();
            return response.Value.Content;
        }

        public async Task<List<ContractModel>> ListFilesAsync()
        {
            var files = new List<ContractModel>();
            await foreach (var fileItem in _directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!fileItem.IsDirectory)
                {
                    var fileClient = _directoryClient.GetFileClient(fileItem.Name);
                    var properties = await fileClient.GetPropertiesAsync();

                    files.Add(new ContractModel
                    {
                        FileName = fileItem.Name,
                        FilePath = fileClient.Uri.ToString(),
                        Size = properties.Value.ContentLength,
                        UploadedDate = properties.Value.LastModified
                    });
                }
            }
            return files;
        }
    }
}
