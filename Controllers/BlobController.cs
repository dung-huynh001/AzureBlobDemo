using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
		private readonly Dictionary<string, string> _contentTypes = new Dictionary<string, string>()
		{
			{".pdf", "application/pdf"},
			{".dxf", "image/vnd.dxf"},
			{".bmp", "image/bmp"},
			{".btif", "image/prs.btif"},
			{".sub", "image/vnd.dvb.subtitle"},
			{".ras", "image/x-cmu-raster"},
			{".cgm", "image/cgm"},
			{".cmx", "image/x-cmx"},
			{".uvi", "image/vnd.dece.graphic"},
			{".djvu", "image/vnd.djvu"},
			{".dwg", "image/vnd.dwg"},
			{".mmr", "image/vnd.fujixerox.edmics-mmr"},
			{".rlc", "image/vnd.fujixerox.edmics-rlc"},
			{".xif", "image/vnd.xiff"},
			{".fst", "image/vnd.fst"},
			{".fbs", "image/vnd.fastbidsheet"},
			{".fpx", "image/vnd.fpx"},
			{".npx", "image/vnd.net-fpx"},
			{".fh", "image/x-freehand"},
			{".g3", "image/g3fax"},
			{".gif", "image/gif"},
			{".ico", "image/x-icon"},
			{".ief", "image/ief"},
			{".jpeg", "image/jpeg"},
			{".jpg", "image/jpeg" },
			{".mdi", "image/vnd.ms-modi"},
			{".ktx", "image/ktx"},
			{".pcx", "image/x-pcx"},
			{".psd", "image/vnd.adobe.photoshop"},
			{".pic", "image/x-pict"},
			{".pnm", "image/x-portable-anymap"},
			{".pbm", "image/x-portable-bitmap"},
			{".pgm", "image/x-portable-graymap"},
			{".png", "image/png"},
			{".ppm", "image/x-portable-pixmap"},
			{".svg", "image/svg+xml"},
			{".rgb", "image/x-rgb"},
			{".tiff", "image/tiff"},
			{".wbmp", "image/vnd.wap.wbmp"},
			{".webp", "image/webp"},
			{".xbm", "image/x-xbitmap"},
			{".xpm", "image/x-xpixmap"},
			{".xwd", "image/x-xwindowdump"},
		};

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
        public async Task<IActionResult> Upload([FromForm] CreateItemVM model)
        {
            if (ModelState.IsValid)
            {
                IFormFile image = model.Image;
                if (image != null && image.Length > 0)
                {
                    string fileName = Guid.NewGuid() + "_" + image.FileName;
                    BlobClient blobClient = _containerClient.GetBlobClient(fileName);
                    string extension = Path.GetExtension(blobClient.Uri.AbsoluteUri);
					string? contentType;
					if (!_contentTypes.TryGetValue(extension, out contentType))
					{
                        // set default contentType if it is null
                        contentType = "application/octet-stream"; 
					}
                    BlobHttpHeaders headers = new BlobHttpHeaders
					{
						ContentType = contentType
					};
					await blobClient.UploadAsync(image.OpenReadStream(), new BlobUploadOptions
					{
						HttpHeaders = headers
					});
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

        public async Task<IActionResult> ViewDetail(int id)
        {
            try
            {
                Item? item = await _context.Items.FindAsync(id);
                if (item is null)
                {
                    return NotFound();
                }
                string fileName = item.FileName.Substring(item.FileName.LastIndexOf("/") + 1);
                BlobClient blobClient = _containerClient.GetBlobClient(fileName);
                string imageUrl = blobClient.Uri.AbsoluteUri;
                ViewItemDetail details = new ViewItemDetail
                {
                    Id = item.Id,
                    Description = item.Description,
                    Title = item.Title,
                    Url = imageUrl
                };
                return View(details);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
        
        [HttpPost]
		public async Task<IActionResult> LoadData()
		{
            // Filter parameters of datatable request
            string draw = HttpContext.Request.Form["draw"].FirstOrDefault() ?? "";
            string searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
            int length = Int32.Parse(Request.Form["length"].FirstOrDefault() ?? "10");
            int start = Int32.Parse(Request.Form["start"].FirstOrDefault() ?? "10");
            string? sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            string? sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
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
            int total = records.Count();
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
            int filtered = records.Count();
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
            Item? item = await _context.Items.FindAsync(id);
            if(item is null)
            {
                return NotFound();
            }
            string fileName = item.FileName.Substring(item.FileName.LastIndexOf("/") + 1);
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
			BlobClient blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                Item? item = await _context.Items.FindAsync(id);
                if (item is null)
                {
                    return NotFound();
                }
                string fileName = item.FileName.Substring(item.FileName.LastIndexOf("/") + 1);
                BlobClient blobClient = _containerClient.GetBlobClient(fileName);
                MemoryStream memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;
                string contentType = blobClient.GetProperties().Value.ContentType;
                return File(memoryStream, contentType, fileName);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
    }
}
