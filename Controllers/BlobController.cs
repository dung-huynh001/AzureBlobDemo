using AzureBlobDemo.Models;
using AzureBlobDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureBlobDemo.Controllers
{
	public class BlobController : Controller
	{
		private readonly BlobService _blobService;

		public BlobController(BlobService blobService)
		{
			this._blobService = blobService;
		}
        public IActionResult Index()
		{
			return View();
		}

		public IActionResult Add()
		{
			return View();
		}

		public IActionResult Update()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Upload(ItemViewModel item)
		{
			var images = item.Image;
			if (images != null && images.Length > 0)
			{
				using (var stream = images.OpenReadStream())
				{
					await _blobService.UploadFileAsync(images.FileName, stream);
				}
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Upload(IFormFile file)
		{
			if (file != null && file.Length > 0)
			{
				using (var stream = file.OpenReadStream())
				{
					await _blobService.UploadFileAsync(file.FileName, stream);
				}
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Download(string fileName)
		{
			var stream = await _blobService.DownloadFileAsync(fileName);
			return File(stream, "application/octet-stream", fileName);
		}

		[HttpPost]
		public async Task<IActionResult> Delete(string fileName)
		{
			await _blobService.DeleteFileAsync(fileName);
			return RedirectToAction("Index");
		}
	}
}
