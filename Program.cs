using Azure.Data.Tables;
using Azure.Storage.Blobs;
using ABCRetailApp.Models;
using ABCRetailApp.Services;
using Microsoft.Extensions.Logging;
using ABCRetailApp.Controllers;

namespace ABCRetailApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient<CustomerController>();
            builder.Services.AddHttpClient<FilesController>();


            // Retrieve the connection string from configuration
            var storageConnectionString = builder.Configuration.GetConnectionString("AzureStorage");

            if (string.IsNullOrEmpty(storageConnectionString))
            {
                throw new ArgumentException("Azure Storage connection string is missing or empty.");
            }

            // Register the TableStorageService for CustomerProfile
            builder.Services.AddSingleton(
                new TableStorageService<CustomerProfile>(
                    storageConnectionString,
                    "Customers" // Ensure this matches your table name in Azure
                )
            );

            // Register the TableStorageService for Product
            builder.Services.AddSingleton(
                new TableStorageService<Product>(
                    storageConnectionString,
                    "Products" // Ensure this matches your table name in Azure
                )
            );

            // Register the TableStorageService for Order
            builder.Services.AddSingleton(
                new TableStorageService<Order>(
                    storageConnectionString,
                    "Orders" // Ensure this matches your table name in Azure
                )
            );

            // Register the CustomerService
            builder.Services.AddSingleton<CustomerService>(serviceProvider =>
            {
                var tableService = serviceProvider.GetRequiredService<TableStorageService<CustomerProfile>>();
                return new CustomerService(tableService);
            });

            // Register the ProductService
            builder.Services.AddSingleton<ProductService>(serviceProvider =>
            {
                var tableService = serviceProvider.GetRequiredService<TableStorageService<Product>>();
                return new ProductService(tableService);
            });

            // Register the OrderService
            builder.Services.AddSingleton<OrderService>(serviceProvider =>
            {
                var tableService = serviceProvider.GetRequiredService<TableStorageService<Order>>();
                return new OrderService(tableService);
            });

            // Register the BlobStorageService with ILogger
            builder.Services.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<BlobStorageService>>();
                return new BlobStorageService(
                    storageConnectionString,
                    "product", // Ensure this matches your blob container name in Azure
                    logger
                );
            });

            // Register the QueueStorageService with ILogger
            builder.Services.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<QueueStorageService>>();
                return new QueueStorageService(
                    storageConnectionString,
                    "order", // Ensure this matches your queue name in Azure
                    logger
                );
            });

            // Register the FileStorageService with ILogger
            builder.Services.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<FileStorageService>>();
                return new FileStorageService(
                    storageConnectionString,
                    "contract-logs"// Ensure this matches your file share name in Azure
                );
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
