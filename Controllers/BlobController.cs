using Azure.Storage.Blobs;
using AzureBlobDemo.Domain.Entities;
using AzureBlobDemo.Infrastructure.Context;
using AzureBlobDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzureBlobDemo.Controllers
{
	public class BlobController : Controller
	{
        private const string _containerName = "images";
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly ApplicationDbContext _context;

        public BlobController(BlobServiceClient blobServiceClient, ApplicationDbContext context)
		{
            _blobServiceClient = blobServiceClient;
            _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            _containerClient.CreateIfNotExists();
            _context = context;
        }
        public IActionResult Index()
		{
            return View();
		}

		public IActionResult Add()
		{
			return View();
		}

        [HttpPost]
        public async Task<IActionResult> Add([FromForm] CreateItemVM model)
        {
            if (ModelState.IsValid)
            {
                IFormFile image = model.Image;
                if (image != null && image.Length > 0)
                {
                    string fileName = Guid.NewGuid() + "_" + image.FileName;
                    BlobClient blobClient = _containerClient.GetBlobClient(fileName);
                    await blobClient.UploadAsync(image.OpenReadStream(), true);

                    Item item = new Item
                    {
                        Title = model.Title,
                        Description = model.Description,
                        FileName = fileName
                    };
                    await _context.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        
        [HttpPost]
		public async Task<IActionResult> LoadData()
		{
            // Filter parameters of datatable request
            string draw = HttpContext.Request.Form["draw"].FirstOrDefault() ?? "";
            string searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            int length = Int32.Parse(Request.Form["length"].FirstOrDefault() ?? "10");
            int start = Int32.Parse(Request.Form["start"].FirstOrDefault() ?? "10");
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();


            // Get all records from db
            var records = _context.Set<Item>()
                .Select(r => new ItemVM
                {
                    FileName = r.FileName,
                    CreatedDate = r.CreatedDate,
                    Description = r.Description,
                    Id = r.Id,
                    Title = r.Title,
                });

            var total = records.Count();

            // Filter by search value
            if (!String.IsNullOrEmpty(searchValue))
            {
                records = records.Where(r => r.Id.ToString().Contains(searchValue)
                    || r.Title.ToLower().Contains(searchValue)
                    || r.Description.ToLower().Contains(searchValue)
                    || r.CreatedDate.ToString().ToLower().Contains(searchValue)
                    || r.FileName.ToLower().Contains(searchValue));
            }

            // Order table by column
            if (!String.IsNullOrEmpty(sortColumn))
            {
                switch (Int32.Parse(sortColumn))
                {
                    case (2):
                        records = sortColumnDirection == "asc" ? records.OrderBy(r => r.Title) : records.OrderByDescending(r => r.Title);
                        break;
                    case (3):
                        records = sortColumnDirection == "asc" ? records.OrderBy(r => r.Description) : records.OrderByDescending(r => r.Description);
                        break;
                    case (4):
                        records = sortColumnDirection == "asc" ? records.OrderBy(r => r.CreatedDate) : records.OrderByDescending(r => r.CreatedDate);
                        break;
                    default:
                        records = sortColumnDirection == "asc" ? records.OrderBy(r => r.Id) : records.OrderByDescending(r => r.Id);
                        break;
                }
            }

            var filtered = records.Count();

            records = records
                .Skip(start)
                .Take(length);

            return Ok(new DataTableResponse<ItemVM>
            {
                Data = await records.ToListAsync(),
                RecordsFiltered = filtered,
                RecordsTotal = total,
                Draw = draw
            });
		}

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // 

            var item = await _context.Items.FindAsync(id);
            if(item is null)
            {
                return NotFound();
            }

            var fileName = item.FileName.Substring(item.FileName.LastIndexOf("/") + 1);

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();



            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item is null)
                {
                    return NotFound();
                }

                var fileName = item.FileName.Substring(item.FileName.LastIndexOf("/") + 1);

                var blobClient = _containerClient.GetBlobClient(fileName);
                var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;
                var contentType = blobClient.GetProperties().Value.ContentType;
                return File(memoryStream, contentType, fileName);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
    }
}
