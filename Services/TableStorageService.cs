using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABCRetailApp.Services
{
    public class TableStorageService<T> where T : class, ITableEntity, new()
    {
        private readonly TableClient _tableClient;

        public TableStorageService(string storageConnectionString, string tableName)
        {
            if (string.IsNullOrEmpty(storageConnectionString))
                throw new ArgumentNullException(nameof(storageConnectionString), "Storage connection string cannot be null or empty.");

            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName), "Table name cannot be null or empty.");

            var tableServiceClient = new TableServiceClient(storageConnectionString);
            _tableClient = tableServiceClient.GetTableClient(tableName);

            try
            {
                _tableClient.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create or get table client: {ex.Message}", ex);
            }
        }

        public async Task InsertOrMergeEntityAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

            await _tableClient.UpsertEntityAsync(entity);
        }

        public async Task<List<T>> RetrieveAllEntitiesAsync()
        {
            var entities = new List<T>();

            var query = _tableClient.QueryAsync<T>();
            await foreach (var entity in query)
            {
                entities.Add(entity);
            }

            return entities;
        }

        public async Task DeleteEntityAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

            await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
        }

        public async Task<T> RetrieveEntityAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException($"Error retrieving entity with PartitionKey: {partitionKey} and RowKey: {rowKey}. Error: {ex.Message}", ex);
            }
        }
    }
}
