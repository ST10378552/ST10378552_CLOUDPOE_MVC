using ABCRetailApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABCRetailApp.Services
{
    public class ProductService
    {
        private readonly TableStorageService<Product> _tableService;

        public ProductService(TableStorageService<Product> tableService)
        {
            _tableService = tableService;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _tableService.RetrieveAllEntitiesAsync();
        }
    }
}
