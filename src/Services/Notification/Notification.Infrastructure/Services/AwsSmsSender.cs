using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Configuration;
using Notification.Domain.Services;
using System;
using System.Threading.Tasks;

namespace Notification.Infrastructure.Services
{
    public class AwsSmsSender : ISmsSender
    {
        private readonly IConfiguration _configuration;
        private readonly AmazonSimpleNotificationServiceClient _snsClient;

        public AwsSmsSender(IConfiguration configuration)
        {
            _configuration = configuration;
            var awsRegion = _configuration["AWS:Region"] ?? "us-east-1";
            _snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName(awsRegion));
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                var request = new PublishRequest
                {
                    Message = message,
                    PhoneNumber = phoneNumber,
                    MessageAttributes = new System.Collections.Generic.Dictionary<string, MessageAttributeValue>
                    {
                        {
                            "AWS.SNS.SMS.SenderID", new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = _configuration["AWS:SNS:SenderId"] ?? "PAYMENT"
                            }
                        },
                        {
                            "AWS.SNS.SMS.SMSType", new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = "Transactional"
                            }
                        }
                    }
                };

                var response = await _snsClient.PublishAsync(request);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}