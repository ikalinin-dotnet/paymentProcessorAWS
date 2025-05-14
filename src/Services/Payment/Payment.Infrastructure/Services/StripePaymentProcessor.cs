using Payment.Domain.Models;
using Payment.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Payment.Infrastructure.Services
{
    public class StripePaymentProcessor : IPaymentProcessor
    {
        private readonly IConfiguration _configuration;

        public StripePaymentProcessor(IConfiguration configuration)
        {
            _configuration = configuration;
            Stripe.StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<(bool IsSuccessful, string? TransactionId, string? ErrorMessage)> ProcessPaymentAsync(
            decimal amount, 
            string currency, 
            Payment.Domain.Models.PaymentMethod paymentMethod)
        {
            try
            {
                // Convert amount to cents/smallest currency unit as required by Stripe
                var amountInSmallestUnit = Convert.ToInt64(amount * 100);
                
                var options = new Stripe.PaymentIntentCreateOptions
                {
                    Amount = amountInSmallestUnit,
                    Currency = currency.ToLower(),
                    PaymentMethod = paymentMethod.ExternalPaymentMethodId,
                    Confirm = true,
                    ConfirmationMethod = "automatic",
                    CaptureMethod = "automatic"
                };

                var service = new Stripe.PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                if (paymentIntent.Status == "succeeded")
                {
                    return (true, paymentIntent.Id, null);
                }
                else if (paymentIntent.Status == "requires_action")
                {
                    // This would require client-side handling for 3D Secure authentication
                    return (false, null, "Payment requires additional authentication");
                }
                else
                {
                    return (false, null, $"Payment failed with status: {paymentIntent.Status}");
                }
            }
            catch (Stripe.StripeException ex)
            {
                return (false, null, $"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Payment processing error: {ex.Message}");
            }
        }

        public async Task<Payment.Domain.Models.PaymentMethod> CreatePaymentMethodAsync(
            Guid userId, 
            PaymentMethodType type, 
            string token, 
            bool setAsDefault = false)
        {
            try
            {
                // Retrieve the payment method from Stripe using the token
                var stripeService = new Stripe.PaymentMethodService();
                var stripePaymentMethod = await stripeService.GetAsync(token);

                // Create a customer or retrieve existing one (simplified example)
                Stripe.CustomerService customerService = new Stripe.CustomerService();
                Stripe.Customer customer;
                
                try
                {
                    // Try to find existing customer by userId metadata
                    var listOptions = new Stripe.CustomerListOptions
                    {
                        Limit = 1
                    };
                    listOptions.AddExtraParam("metadata[userId]", userId.ToString());
                    
                    var customers = await customerService.ListAsync(listOptions);
                    customer = customers.FirstOrDefault();
                    
                    if (customer == null)
                    {
                        // Create new customer
                        var customerOptions = new Stripe.CustomerCreateOptions
                        {
                            PaymentMethod = token,
                            Metadata = new Dictionary<string, string>
                            {
                                { "userId", userId.ToString() }
                            }
                        };
                        
                        customer = await customerService.CreateAsync(customerOptions);
                    }
                }
                catch
                {
                    // If error, create a new customer
                    var customerOptions = new Stripe.CustomerCreateOptions
                    {
                        PaymentMethod = token,
                        Metadata = new Dictionary<string, string>
                        {
                            { "userId", userId.ToString() }
                        }
                    };
                    
                    customer = await customerService.CreateAsync(customerOptions);
                }

                // Attach the payment method to the customer
                var attachOptions = new Stripe.PaymentMethodAttachOptions
                {
                    Customer = customer.Id,
                };
                await stripeService.AttachAsync(token, attachOptions);

                // Get card details
                string? cardLastFour = null;
                string? cardBrand = null;
                
                if (stripePaymentMethod.Type == "card" && stripePaymentMethod.Card != null)
                {
                    cardLastFour = stripePaymentMethod.Card.Last4;
                    cardBrand = stripePaymentMethod.Card.Brand;
                }

                // Create a payment method in our domain
                var paymentMethodType = MapToPaymentMethodType(stripePaymentMethod.Type);
                var paymentMethod = new Payment.Domain.Models.PaymentMethod(
                    userId,
                    paymentMethodType,
                    stripePaymentMethod.Id,
                    cardLastFour,
                    cardBrand);

                if (setAsDefault)
                {
                    paymentMethod.SetAsDefault();
                    
                    // Also set as default in Stripe
                    var updateOptions = new Stripe.CustomerUpdateOptions
                    {
                        InvoiceSettings = new Stripe.CustomerInvoiceSettingsOptions
                        {
                            DefaultPaymentMethod = stripePaymentMethod.Id
                        }
                    };
                    await customerService.UpdateAsync(customer.Id, updateOptions);
                }

                return paymentMethod;
            }
            catch (Stripe.StripeException ex)
            {
                throw new Exception($"Stripe error while creating payment method: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating payment method: {ex.Message}", ex);
            }
        }

        private PaymentMethodType MapToPaymentMethodType(string stripeType)
        {
            return stripeType switch
            {
                "card" => PaymentMethodType.CreditCard, // We'd need more info to distinguish between credit/debit
                "bank_account" => PaymentMethodType.BankAccount,
                _ => PaymentMethodType.DigitalWallet // Default for other types
            };
        }
    }
}