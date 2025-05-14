using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Payment.Domain.Models;
using Payment.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Payment.Infrastructure.Repositories
{
    public class DynamoDBTransactionRepository : ITransactionRepository
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly string _tableName;

        public DynamoDBTransactionRepository(IAmazonDynamoDB dynamoDbClient, IConfiguration configuration)
        {
            _dynamoDbClient = dynamoDbClient;
            _tableName = configuration["AWS:DynamoDB:TransactionsTable"];
        }

        public async Task<Transaction> GetByIdAsync(Guid id)
        {
            var request = new GetItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = id.ToString() } }
                }
            };

            var response = await _dynamoDbClient.GetItemAsync(request);

            if (response.Item == null || response.Item.Count == 0)
            {
                return null;
            }

            return MapToTransaction(response.Item);
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId)
        {
            // Using Query operation with a GSI on UserId
            var request = new QueryRequest
            {
                TableName = _tableName,
                IndexName = "UserIdIndex",
                KeyConditionExpression = "UserId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = userId.ToString() } }
                }
            };

            var response = await _dynamoDbClient.QueryAsync(request);

            if (response.Items == null || response.Items.Count == 0)
            {
                return new List<Transaction>();
            }

            var transactions = response.Items.Select(MapToTransaction).ToList();
            
            // Sort by CreatedAt descending
            return transactions.OrderByDescending(t => t.CreatedAt);
        }

        public async Task<Transaction> CreateAsync(Transaction transaction)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = transaction.Id.ToString() } },
                { "UserId", new AttributeValue { S = transaction.UserId.ToString() } },
                { "Amount", new AttributeValue { N = transaction.Amount.ToString() } },
                { "Currency", new AttributeValue { S = transaction.Currency } },
                { "Status", new AttributeValue { S = transaction.Status.ToString() } },
                { "PaymentMethodId", new AttributeValue { S = transaction.PaymentMethod.Id.ToString() } },
                { "CreatedAt", new AttributeValue { S = transaction.CreatedAt.ToString("o") } }
            };

            if (transaction.ExternalTransactionId != null)
            {
                item.Add("ExternalTransactionId", new AttributeValue { S = transaction.ExternalTransactionId });
            }

            if (transaction.UpdatedAt.HasValue)
            {
                item.Add("UpdatedAt", new AttributeValue { S = transaction.UpdatedAt.Value.ToString("o") });
            }

            if (transaction.ErrorMessage != null)
            {
                item.Add("ErrorMessage", new AttributeValue { S = transaction.ErrorMessage });
            }

            // Serialize PaymentMethod as JSON
            var paymentMethodJson = JsonSerializer.Serialize(transaction.PaymentMethod);
            item.Add("PaymentMethodData", new AttributeValue { S = paymentMethodJson });

            var request = new PutItemRequest
            {
                TableName = _tableName,
                Item = item
            };

            await _dynamoDbClient.PutItemAsync(request);
            return transaction;
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            // Implement update logic similar to create but using UpdateItem operation
            var updateExpressions = new List<string>();
            var expressionAttributeValues = new Dictionary<string, AttributeValue>();
            var expressionAttributeNames = new Dictionary<string, string>();

            // Update Status
            updateExpressions.Add("#status = :status");
            expressionAttributeNames.Add("#status", "Status");
            expressionAttributeValues.Add(":status", new AttributeValue { S = transaction.Status.ToString() });

            // Update ExternalTransactionId if not null
            if (transaction.ExternalTransactionId != null)
            {
                updateExpressions.Add("#extId = :extId");
                expressionAttributeNames.Add("#extId", "ExternalTransactionId");
                expressionAttributeValues.Add(":extId", new AttributeValue { S = transaction.ExternalTransactionId });
            }

            // Update ErrorMessage if not null
            if (transaction.ErrorMessage != null)
            {
                updateExpressions.Add("#errMsg = :errMsg");
                expressionAttributeNames.Add("#errMsg", "ErrorMessage");
                expressionAttributeValues.Add(":errMsg", new AttributeValue { S = transaction.ErrorMessage });
            }

            // Update UpdatedAt
            updateExpressions.Add("#updatedAt = :updatedAt");
            expressionAttributeNames.Add("#updatedAt", "UpdatedAt");
            expressionAttributeValues.Add(":updatedAt", new AttributeValue { S = DateTime.UtcNow.ToString("o") });

            var request = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = transaction.Id.ToString() } }
                },
                UpdateExpression = "SET " + string.Join(", ", updateExpressions),
                ExpressionAttributeNames = expressionAttributeNames,
                ExpressionAttributeValues = expressionAttributeValues
            };

            await _dynamoDbClient.UpdateItemAsync(request);
        }

        public async Task<IEnumerable<Transaction>> GetPendingTransactionsAsync()
        {
            // Using Query operation with a GSI on Status
            var request = new QueryRequest
            {
                TableName = _tableName,
                IndexName = "StatusIndex",
                KeyConditionExpression = "#status = :status",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#status", "Status" }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":status", new AttributeValue { S = TransactionStatus.Pending.ToString() } }
                }
            };

            var response = await _dynamoDbClient.QueryAsync(request);

            if (response.Items == null || response.Items.Count == 0)
            {
                return new List<Transaction>();
            }

            var transactions = response.Items.Select(MapToTransaction).ToList();
            
            // Sort by CreatedAt ascending for oldest first
            return transactions.OrderBy(t => t.CreatedAt);
        }

        private Transaction MapToTransaction(Dictionary<string, AttributeValue> item)
        {
            // This is a simplified implementation - you'd need to properly reconstruct
            // the domain object based on your actual model structure
            // For now, we'll return a basic transaction object
            
            var transaction = new Transaction(
                Guid.Parse(item["UserId"].S),
                decimal.Parse(item["Amount"].N),
                item["Currency"].S,
                null // We'll set this from JSON below
            );

            // Use reflection or other means to set the ID and other properties
            // This is just a simple example - you'd need to adapt to your actual model
            
            // Load the payment method from JSON
            if (item.ContainsKey("PaymentMethodData"))
            {
                var paymentMethod = JsonSerializer.Deserialize<PaymentMethod>(item["PaymentMethodData"].S);
                // Set payment method property using reflection or other means
            }

            return transaction;
        }
    }
}