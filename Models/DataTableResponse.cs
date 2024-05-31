namespace AzureBlobDemo.Models
{
    public class DataTableResponse<T> where T : class
    {
        public List<T> Data { get; set; } = new List<T>();
        public int RecordsFiltered { get; set; }
        public int RecordsTotal { get; set; }
        public string Draw { get; set; } = string.Empty;
    }
}
