using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ABCRetailApp.Models;
using ABCRetailApp.Services;
using Newtonsoft.Json;

namespace ABCRetailApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly TableStorageService<Product> _tableStorageService;
        private readonly BlobStorageService _blobStorageService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            TableStorageService<Product> tableStorageService,
            BlobStorageService blobStorageService,
            ILogger<ProductController> logger)
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        // GET: /Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Product/Create
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (product == null)
            {
                _logger.LogWarning("Product data is null.");
                ModelState.AddModelError("", "Product data is not valid.");
                return View(product);
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is not valid.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Model state error: {ErrorMessage}", error.ErrorMessage);
                }
                return View(product);
            }

            _logger.LogInformation("Product Price: {Price}", product.Price);

            string imageUrl = string.Empty;

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                using var fileStream = imageFile.OpenReadStream();
                try
                {
                    _logger.LogInformation("Uploading file {FileName} to blob storage.", fileName);
                    await _blobStorageService.UploadBlobAsync(fileName, fileStream);
                    imageUrl = _blobStorageService.GetBlobUrl(fileName);
                    _logger.LogInformation("File uploaded successfully. URL: {ImageUrl}", imageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image: {ErrorMessage}", ex.Message);
                    ModelState.AddModelError("", $"Error uploading image: {ex.Message}");
                    return View(product);
                }
            }

            product.PartitionKey = "Product";
            product.RowKey = Guid.NewGuid().ToString();
            product.ImageUrl = imageUrl;

            try
            {
                _logger.LogInformation("Inserting or merging product with RowKey: {RowKey}", product.RowKey);
                await _tableStorageService.InsertOrMergeEntityAsync(product);
                _logger.LogInformation("Product with RowKey: {RowKey} saved successfully.", product.RowKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving product with RowKey: {RowKey}. Error: {ErrorMessage}", product.RowKey, ex.Message);
                ModelState.AddModelError("", $"Error saving product: {ex.Message}");
                return View(product);
            }

            try
            {
                var httpClient = new HttpClient();
                var jsonContent = JsonConvert.SerializeObject(product);
                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://st10378552cloud.azurewebsites.net/api/StoreProduct?code=sPOm4GuVb03xTQ9twuII6cnvI9s8FFxygfxcxGc2p9-wAzFu3grgpA%3D%3D", httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error saving product: {errorMessage}");
                    return View(product);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling function to store product: {ErrorMessage}", ex.Message);
                ModelState.AddModelError("", $"Error saving product: {ex.Message}");
                return View(product);
            }

            return RedirectToAction("Index");
        }

        // GET: /Product/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _tableStorageService.RetrieveAllEntitiesAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving products.");
            }
        }

        // GET: /Product/Details/{rowKey}
        public async Task<IActionResult> Details(string rowKey)
        {
            try
            {
                var product = await _tableStorageService.RetrieveEntityAsync("Product", rowKey);
                if (product == null)
                {
                    _logger.LogWarning("Product with RowKey: {RowKey} not found.", rowKey);
                    return NotFound();
                }
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with RowKey: {RowKey}. Error: {ErrorMessage}", rowKey, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving product.");
            }
        }
        // GET: /Product/Delete/{rowKey}
        public async Task<IActionResult> Delete(string rowKey)
        {
            try
            {
                var product = await _tableStorageService.RetrieveEntityAsync("Product", rowKey);
                if (product == null)
                {
                    _logger.LogWarning("Product with RowKey: {RowKey} not found.", rowKey);
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction("Index");
                }
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with RowKey: {RowKey}. Error: {ErrorMessage}", rowKey, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving product.");
            }
        }
        // POST: /Product/DeleteConfirmed
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(string rowKey, string partitionKey)
        {
            try
            {
                var product = await _tableStorageService.RetrieveEntityAsync(partitionKey, rowKey);
                if (product == null)
                {
                    _logger.LogWarning("Product with RowKey: {RowKey} not found.", rowKey);
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                await _tableStorageService.DeleteEntityAsync(product);
                _logger.LogInformation("Product with RowKey: {RowKey} deleted successfully.", rowKey);
                TempData["SuccessMessage"] = "Product deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with RowKey: {RowKey}. Error: {ErrorMessage}", rowKey, ex.Message);
                TempData["ErrorMessage"] = $"Error deleting product: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

    }
}
