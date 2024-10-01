using Microsoft.AspNetCore.Mvc;
using ABCRetailApp.Models;
using ABCRetailApp.Services;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace ABCRetailApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly TableStorageService<CustomerProfile> _tableStorageService;
        private readonly HttpClient _httpClient;

        public CustomerController(TableStorageService<CustomerProfile> tableStorageService, HttpClient httpClient)
        {
            _tableStorageService = tableStorageService;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _tableStorageService.RetrieveAllEntitiesAsync();
            return View(customers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StoreCustomer(CustomerProfile customer)
        {
            var json = JsonConvert.SerializeObject(customer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://st10378552cloud.azurewebsites.net/api/StoreCustomerProfile?code=sPOm4GuVb03xTQ9twuII6cnvI9s8FFxygfxcxGc2p9-wAzFu3grgpA%3D%3D", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View("Error", new { message = "Failed to store customer profile." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerProfile customer)
        {
            if (ModelState.IsValid)
            {
                customer.PartitionKey = "CustomerProfile";
                customer.RowKey = Guid.NewGuid().ToString();
                await _tableStorageService.InsertOrMergeEntityAsync(customer);
                return RedirectToAction("Index");
            }
            return View(customer);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var customer = new CustomerProfile
            {
                PartitionKey = partitionKey,
                RowKey = rowKey
            };
            await _tableStorageService.DeleteEntityAsync(customer);
            return RedirectToAction("Index");
        }
    
}
}
