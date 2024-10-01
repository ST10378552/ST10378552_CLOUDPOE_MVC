using Azure.Data.Tables;
using Azure;

namespace ABCRetailApp.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; } = 0.00;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
