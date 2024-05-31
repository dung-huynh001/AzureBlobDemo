namespace AzureBlobDemo.Models
{
	public class ItemVM
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public required string FileName { get; set; }
		public DateTime CreatedDate { get; set; }
    }
}
