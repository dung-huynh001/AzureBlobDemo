namespace AzureBlobDemo.Models
{
    public class CreateItemVM
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public required IFormFile Image { get; set; }
    }
}
