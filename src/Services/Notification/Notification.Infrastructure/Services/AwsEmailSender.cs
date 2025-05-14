using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Configuration;
using Notification.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notification.Infrastructure.Services
{
    public class AwsEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly AmazonSimpleEmailServiceClient _sesClient;
        private readonly string _senderEmail;

        public AwsEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
            var awsRegion = _configuration["AWS:Region"] ?? "us-east-1";
            _sesClient = new AmazonSimpleEmailServiceClient(RegionEndpoint.GetBySystemName(awsRegion));
            _senderEmail = _configuration["AWS:SES:SenderEmail"] ?? 
                           throw new InvalidOperationException("Sender email not configured");
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, string? replyTo = null)
        {
            try
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = _senderEmail,
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { to }
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = body
                            }
                        }
                    }
                };

                if (!string.IsNullOrEmpty(replyTo))
                {
                    sendRequest.ReplyToAddresses = new List<string> { replyTo };
                }

                var response = await _sesClient.SendEmailAsync(sendRequest);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}