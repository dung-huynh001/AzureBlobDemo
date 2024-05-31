using AzureBlobDemo.Domain.Common;

namespace AzureBlobDemo.Domain.Entities
{
	public class Item : BaseEntity
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string FileName { get; set; } = string.Empty;
	}
}
