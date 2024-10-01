using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;

namespace ABCRetailApp.Services
{
    public class QueueStorageService
    {
        private readonly QueueClient _queueClient;
        private readonly ILogger<QueueStorageService> _logger;

        public QueueStorageService(string storageConnectionString, string queueName, ILogger<QueueStorageService> logger)
        {
            var queueServiceClient = new QueueServiceClient(storageConnectionString);
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _queueClient.CreateIfNotExists();
            _logger = logger;
        }

        public async Task SendMessageAsync(string messageText)
        {
            try
            {
                await _queueClient.SendMessageAsync(messageText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                throw new ApplicationException($"Error sending message: {ex.Message}", ex);
            }
        }

        public async Task<List<QueueMessage>> ReceiveMessagesAsync(int maxMessages = 10)
        {
            var messages = new List<QueueMessage>();
            try
            {
                var response = await _queueClient.ReceiveMessagesAsync(maxMessages);
                messages.AddRange(response.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages");
                throw new ApplicationException($"Error receiving messages: {ex.Message}", ex);
            }
            return messages;
        }

        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            try
            {
                await _queueClient.DeleteMessageAsync(messageId, popReceipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message");
                throw new ApplicationException($"Error deleting message: {ex.Message}", ex);
            }
        }
    }
}
