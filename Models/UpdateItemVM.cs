namespace AzureBlobDemo.Models
{
    public class UpdateItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}
