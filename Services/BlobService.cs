using Azure.Storage.Blobs;

namespace AzureBlobDemo.Services
{
	public class BlobService
	{
		private readonly string _containerName = "images";

		private readonly BlobServiceClient _blobServiceClient;
		private readonly BlobContainerClient _containerClient;
		public BlobService(BlobServiceClient blobServiceClient)
		{
			_blobServiceClient = blobServiceClient;
			_containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
			_containerClient.CreateIfNotExists();
		}

		public async Task UploadFileAsync(string fileName, Stream fileStream)
		{
			var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
			var blobClient = containerClient.GetBlobClient(fileName);
			await blobClient.UploadAsync(fileStream, overwrite: true);
		}

		public async Task<Stream> DownloadFileAsync(string fileName)
		{
			var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
			var blobClient = containerClient.GetBlobClient(fileName);
			var response = await blobClient.DownloadAsync();
			return response.Value.Content;
		}
		public async Task DeleteFileAsync(string fileName)
		{
			var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
			var blobClient = containerClient.GetBlobClient(fileName);
			await blobClient.DeleteIfExistsAsync();
		}
	}
}
