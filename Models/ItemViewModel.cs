namespace AzureBlobDemo.Models
{
	public class ItemViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public required IFormFile Image { get; set; }

	}
}
