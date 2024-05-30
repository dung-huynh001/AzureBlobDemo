namespace AzureBlobDemo.Domain.Common
{
	public abstract class BaseEntity
	{
		public DateTime CreatedDate { get; set; }
		public DateTime UpdatedDate { get; set; }
		public bool DeletedFlag { get; set; }
	}
}
