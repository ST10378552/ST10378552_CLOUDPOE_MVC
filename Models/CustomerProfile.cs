using Azure;
using Azure.Data.Tables;

namespace ABCRetailApp.Models
{
    public class CustomerProfile : ITableEntity
    {
        // Required for Azure Table Storage
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Custom properties
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

        // Constructor initializing properties
        public CustomerProfile()
        {
            PartitionKey = string.Empty;
            RowKey = string.Empty;
            ETag = default;
            FirstName = string.Empty;
            Surname = string.Empty;
            Email = string.Empty;
        }
    }
}
