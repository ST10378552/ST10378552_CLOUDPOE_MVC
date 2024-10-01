using Azure;
using Azure.Data.Tables;
using System;

namespace ABCRetailApp.Models
{
    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; } = "Orders";
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string CustomerId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public double Price { get; set; } = 0.00;

        // Ensure OrderDate is in UTC
        private DateTime _orderDate = DateTime.UtcNow;
        public DateTime OrderDate
        {
            get => _orderDate;
            set => _orderDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public string Status { get; set; } = "Pending";
    }
}
