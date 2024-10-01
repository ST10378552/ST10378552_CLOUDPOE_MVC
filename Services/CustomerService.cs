using ABCRetailApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABCRetailApp.Services
{
    public class CustomerService
    {
        private readonly TableStorageService<CustomerProfile> _tableService;

        public CustomerService(TableStorageService<CustomerProfile> tableService)
        {
            _tableService = tableService;
        }

        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            return await _tableService.RetrieveAllEntitiesAsync();
        }
    }
}
