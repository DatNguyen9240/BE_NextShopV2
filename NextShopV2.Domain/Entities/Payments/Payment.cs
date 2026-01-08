using System;

namespace NextShopV2.Domain.Entities.Payments
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public string Method { get; set; } = "COD"; // COD, CreditCard, PayPal, ONLINE, ...
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Paid, Failed
        public string? ProviderPaymentId { get; set; } // id from payment provider (orderCode/paymentId)
        public string? ProviderData { get; set; } // raw provider payload (json)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
