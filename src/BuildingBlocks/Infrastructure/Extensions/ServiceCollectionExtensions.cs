using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.SimpleEmail;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace PaymentProcessor.BuildingBlocks.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAwsServices(this IServiceCollection services, IConfiguration configuration)
        {
            var awsOptions = configuration.GetSection("AWS");
            var region = awsOptions["Region"] ?? "us-east-1";
            var regionEndpoint = RegionEndpoint.GetBySystemName(region);
            
            var serviceUrl = awsOptions["ServiceURL"];
            
            // Register AWS clients
            if (!string.IsNullOrEmpty(serviceUrl))
            {
                // For local development with LocalStack
                var dynamoDbConfig = new AmazonDynamoDBConfig { ServiceURL = serviceUrl };
                var s3Config = new AmazonS3Config { ServiceURL = serviceUrl };
                var sqsConfig = new AmazonSQSConfig { ServiceURL = serviceUrl };
                var snsConfig = new AmazonSimpleNotificationServiceConfig { ServiceURL = serviceUrl };
                var sesConfig = new AmazonSimpleEmailServiceConfig { ServiceURL = serviceUrl };
                
                services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(dynamoDbConfig));
                services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(s3Config));
                services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(sqsConfig));
                services.AddSingleton<IAmazonSimpleNotificationService>(_ => new AmazonSimpleNotificationServiceClient(snsConfig));
                services.AddSingleton<IAmazonSimpleEmailService>(_ => new AmazonSimpleEmailServiceClient(sesConfig));
            }
            else
            {
                // For production use
                services.AddAWSService<IAmazonDynamoDB>(new Amazon.Extensions.NETCore.Setup.AWSOptions
                {
                    Region = regionEndpoint
                });
                
                services.AddAWSService<IAmazonS3>(new Amazon.Extensions.NETCore.Setup.AWSOptions
                {
                    Region = regionEndpoint
                });
                
                services.AddAWSService<IAmazonSQS>(new Amazon.Extensions.NETCore.Setup.AWSOptions
                {
                    Region = regionEndpoint
                });
                
                services.AddAWSService<IAmazonSimpleNotificationService>(new Amazon.Extensions.NETCore.Setup.AWSOptions
                {
                    Region = regionEndpoint
                });
                
                services.AddAWSService<IAmazonSimpleEmailService>(new Amazon.Extensions.NETCore.Setup.AWSOptions
                {
                    Region = regionEndpoint
                });
            }
            
            return services;
        }
    }
}