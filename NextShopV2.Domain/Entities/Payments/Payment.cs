using System;

namespace NextShopV2.Domain.Entities.Payments
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public string Method { get; set; } = "COD"; // COD, CreditCard, PayPal, ...
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Paid, Failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
