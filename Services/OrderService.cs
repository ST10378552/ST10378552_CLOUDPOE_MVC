using ABCRetailApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABCRetailApp.Services
{
    public class OrderService
    {
        private readonly TableStorageService<Order> _tableService;

        public OrderService(TableStorageService<Order> tableService)
        {
            _tableService = tableService;
        }

        public Task AddOrderAsync(Order order)
        {
            return _tableService.InsertOrMergeEntityAsync(order);
        }

        public Task<Order> GetOrderAsync(string partitionKey, string rowKey)
        {
            return _tableService.RetrieveEntityAsync(partitionKey, rowKey);
        }

        public Task<List<Order>> GetAllOrdersAsync()
        {
            return _tableService.RetrieveAllEntitiesAsync();
        }
    }
}
