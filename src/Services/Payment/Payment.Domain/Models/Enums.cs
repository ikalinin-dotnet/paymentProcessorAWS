namespace Payment.Domain.Models
{
    public enum TransactionStatus
    {
        Pending,
        Processing,
        Successful,
        Failed
    }

    public enum PaymentMethodType
    {
        CreditCard,
        DebitCard,
        BankAccount,
        DigitalWallet
    }
    
    public enum PaymentProvider
    {
        Stripe,
        PayPal,
        BankTransfer
    }
}