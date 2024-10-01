using ABCRetailApp.Models;
using ABCRetailApp.Services;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace ABCRetailApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly QueueStorageService _queueService;
        private readonly CustomerService _customerService;
        private readonly ProductService _productService;
        private readonly TableStorageService<Order> _orderTableService;
        private readonly ILogger<OrderController> _logger;
        private readonly HttpClient _httpClient;

        public OrderController(
            QueueStorageService queueService,
            CustomerService customerService,
            ProductService productService,
            TableStorageService<Order> orderTableService,
            ILogger<OrderController> logger,
            HttpClient httpClient)
        {
            _queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _orderTableService = orderTableService ?? throw new ArgumentNullException(nameof(orderTableService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient;
        }

        // GET: /Order/
        public async Task<IActionResult> Index()
{
    try
    {
        var orders = await _orderTableService.RetrieveAllEntitiesAsync(); // Fetch directly from Azure Table Storage
        return View(orders);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching orders from table storage.");
        return StatusCode(500, "Internal server error.");
    }
}
        // GET: /Order/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                var products = await _productService.GetAllProductsAsync();

                var model = new OrderCreateViewModel
                {
                    Customers = customers.Select(c => new SelectListItem
                    {
                        Value = c.RowKey,
                        Text = c.FirstName
                    }).ToList(),

                    Products = products.Select(p => new SelectListItem
                    {
                        Value = p.RowKey,
                        Text = p.ProductName
                    }).ToList()
                };

                if (ModelState.IsValid)
                {
                    var order = new Order
                    {
                        RowKey = Guid.NewGuid().ToString(),
                        CustomerId = model.SelectedCustomerId,
                        ProductId = model.SelectedProductId,
                        Quantity = model.Quantity,
                        Price = model.Price,
                        OrderDate = model.OrderDate.ToUniversalTime(),
                        Status = "Pending"
                    };

                    var orderJson = JsonSerializer.Serialize(order);
                    var content = new StringContent(orderJson, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync("https://st10378552cloud.azurewebsites.net/api/orders/send?code=sPOm4GuVb03xTQ9twuII6cnvI9s8FFxygfxcxGc2p9-wAzFu3grgpA%3D%3D", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["OrderSuccess"] = "Your order has been successfully placed and is now in the queue for processing.";
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("", "Error placing order.");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers or products for Create view.");
                return StatusCode(500, "Internal server error.");
            }
        }

      [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(OrderCreateViewModel viewModel)
{
    if (ModelState.IsValid)
    {
        try
        {
            var order = new Order
            {
                RowKey = Guid.NewGuid().ToString(),
                CustomerId = viewModel.SelectedCustomerId,
                ProductId = viewModel.SelectedProductId,
                Quantity = viewModel.Quantity,
                Price = viewModel.Price,
                OrderDate = viewModel.OrderDate.ToUniversalTime(), // Ensure UTC
                Status = "Pending"
            };

            // Serialize and send to queue
            var messageText = JsonSerializer.Serialize(order);
            await _queueService.SendMessageAsync(messageText);

                    // Set success message
                    TempData["OrderSuccess"] = "Your order has been successfully placed and is now in the queue for processing.\r\nIt may take a few days to appear on the Orders page.\r\n Please note that multiple orders are being processed simultaneously, indicating that the system is functioning as intended.\r\n If you have any questions, feel free to reach out to us at ABC@gmail.com";


                    // Redirect after successful creation
                    return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to queue.");
            ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
        }
    }

    // Repopulate ViewModel in case of error
    viewModel.Customers = await GetCustomerSelectList();
    viewModel.Products = await GetProductSelectList();

    return View(viewModel);
}


        private async Task<List<Order>> FetchOrdersFromQueue()
        {
            var orders = new List<Order>();

            try
            {
                var messages = await _queueService.ReceiveMessagesAsync();

                foreach (var message in messages)
                {
                    try
                    {
                        _logger.LogInformation("Received message: {MessageText}", message.MessageText);

                        var order = JsonSerializer.Deserialize<Order>(message.MessageText);
                        if (order != null)
                        {
                            orders.Add(order);

                            // Store order in Azure Table Storage
                            await _orderTableService.InsertOrMergeEntityAsync(order);
                            _logger.LogInformation("Order inserted/merged in table storage: {Order}", order);

                            // Delete the message after processing
                            await _queueService.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to deserialize order from message: {MessageText}", message.MessageText);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message from queue: {MessageText}", message.MessageText);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages from queue.");
            }

            return orders;
        }

        private async Task<List<SelectListItem>> GetCustomerSelectList()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return customers.Select(c => new SelectListItem
            {
                Value = c.RowKey,
                Text = c.FirstName
            }).ToList();
        }

        private async Task<List<SelectListItem>> GetProductSelectList()
        {
            var products = await _productService.GetAllProductsAsync();
            return products.Select(p => new SelectListItem
            {
                Value = p.RowKey,
                Text = p.ProductName
            }).ToList();
        }
    }
}
