using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Shared.Interfaces;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Domain.Entities.Payments;
using NextShopV2.Domain.Entities.Orders;
using NextShopV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PayOS;
using PayOS.Models.V2.PaymentRequests;

namespace NextShopV2.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly PayOSClient _payosClient;
    private readonly IRedisCartService _cartService;
    private readonly ISocketNotificationService _notificationService;

    private readonly StackExchange.Redis.IConnectionMultiplexer _redis;

    public PaymentService(AppDbContext db, PayOSClient payosClient, IRedisCartService cartService, StackExchange.Redis.IConnectionMultiplexer redis, ISocketNotificationService notificationService)
    {
        _db = db;
        _payosClient = payosClient;
        _cartService = cartService;
        _redis = redis;
        _notificationService = notificationService;
    }

    public async Task<bool> HandlePayOSWebhookAsync(string body, string? signature, string checksumKey)
    {
        // Validate signature if provided
        if (!string.IsNullOrEmpty(checksumKey) && !string.IsNullOrEmpty(signature))
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(checksumKey));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(body));
            var computed = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            if (!string.Equals(computed, signature, StringComparison.OrdinalIgnoreCase))
            {
                return false; // Invalid signature
            }
        }

        // Parse JSON
        var json = JsonDocument.Parse(body).RootElement;
        string? orderCode = null;
        string? status = null;

        // orderCode is inside "data"
        if (json.TryGetProperty("data", out var data))
        {
            if (data.TryGetProperty("orderCode", out var oc))
            {
                if (oc.ValueKind == JsonValueKind.String)
                    orderCode = oc.GetString();
                else if (oc.ValueKind == JsonValueKind.Number)
                    orderCode = oc.GetInt64().ToString();
            }
            if (data.TryGetProperty("paymentId", out var pid))
            {
                if (pid.ValueKind == JsonValueKind.String)
                    orderCode ??= pid.GetString();
                else if (pid.ValueKind == JsonValueKind.Number)
                    orderCode ??= pid.GetInt64().ToString();
            }
            if (data.TryGetProperty("status", out var st)) status = st.GetString();
            // For PayOS, check code for success
            if (data.TryGetProperty("code", out var code) && code.GetString() == "00")
            {
                status = "paid";
            }
        }

        // Fallback to root level
        if (string.IsNullOrEmpty(orderCode) && json.TryGetProperty("orderCode", out var ocRoot))
        {
            if (ocRoot.ValueKind == JsonValueKind.String)
                orderCode = ocRoot.GetString();
            else if (ocRoot.ValueKind == JsonValueKind.Number)
                orderCode = ocRoot.GetInt64().ToString();
        }
        if (string.IsNullOrEmpty(orderCode) && json.TryGetProperty("paymentId", out var pidRoot)) orderCode ??= pidRoot.GetString();
        if (string.IsNullOrEmpty(status) && json.TryGetProperty("status", out var stRoot)) status = stRoot.GetString();

        if (status != null) status = status.ToLowerInvariant();

        // Find payment by ProviderPaymentId
        var payment = _db.Payments.FirstOrDefault(p => p.ProviderPaymentId == orderCode);

        // Fallback: find by OrderId if orderId provided
        if (payment == null && json.TryGetProperty("orderId", out var oid) && Guid.TryParse(oid.GetString(), out var parsedOrderId))
        {
            payment = _db.Payments.FirstOrDefault(p => p.OrderId == parsedOrderId);
            if (payment != null)
            {
                Console.WriteLine($"Found payment by OrderId={parsedOrderId} (PaymentId={payment.PaymentId})");
                // If webhook uses a different provider id, sync it to the existing payment so FE polling by provider id will match
                if (!string.IsNullOrEmpty(orderCode) && payment.ProviderPaymentId != orderCode)
                {
                    Console.WriteLine($"Syncing ProviderPaymentId for PaymentId={payment.PaymentId}: {payment.ProviderPaymentId} -> {orderCode}");
                    payment.ProviderPaymentId = orderCode;
                    _db.Payments.Update(payment);
                }
            }
        }

        if (payment == null)
        {
            // Create new payment if order exists
            if (!string.IsNullOrEmpty(orderCode) && json.TryGetProperty("orderId", out var oid2) && Guid.TryParse(oid2.GetString(), out var parsedOrderId2))
            {
                payment = new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    OrderId = parsedOrderId2,
                    Method = "ONLINE",
                    Status = status == "paid" ? "Paid" : "Pending",
                    ProviderPaymentId = orderCode,
                    ProviderData = body,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Payments.Add(payment);
            }
            else
            {
                Console.WriteLine($"Cannot process webhook: orderCode={orderCode}, hasOrderId=false");
                return false; // Cannot process
            }
        }
        else
        {
            payment.ProviderData = body;
            if (!string.IsNullOrEmpty(status))
            {
                payment.Status = status.Equals("paid", StringComparison.OrdinalIgnoreCase) ? "Paid" :
                                 status.Equals("failed", StringComparison.OrdinalIgnoreCase) ? "Failed" : payment.Status;
            }
            _db.Payments.Update(payment);
        }

        // Update order status if paid
        if (payment != null && string.Equals(payment.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == payment.OrderId);
            if (order != null)
            {
                order.Status = "Paid";
                _db.Orders.Update(order);

                try
                {
                    // Clear user's cart now that payment is confirmed
                    var cleared = await _cartService.ClearCartAsync(order.UserId);
                    Console.WriteLine($"Cleared cart for user {order.UserId}: {cleared}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to clear cart for user {order.UserId}: {ex.Message}");
                }

                try
                {
                    // Send socket notification for successful payment
                    var notification = new SocketNotificationDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = "Thanh toán thành công",
                        Body = $"Đơn hàng {order.OrderId} đã được thanh toán.",
                        Url = $"/payment/success?orderId={order.OrderId}",
                        Read = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _notificationService.AddNotificationAsync(order.UserId.ToString(), notification);
                    Console.WriteLine($"Socket notification sent for successful payment of order {order.OrderId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send socket notification for user {order.UserId}: {ex.Message}");
                }            }
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public PaymentStatusResponse? GetPaymentStatus(string orderCode)
    {
        if (string.IsNullOrEmpty(orderCode)) return null;

        var payment = _db.Payments.FirstOrDefault(p => p.ProviderPaymentId == orderCode);
        if (payment == null) return null;

        return new PaymentStatusResponse
        {
            Status = payment.Status,
            OrderId = payment.OrderId,
            ProviderData = payment.ProviderData
        };
    }

    public PaymentStatusResponse? GetPaymentStatusByOrderId(Guid orderId)
    {
        var payments = _db.Payments.Where(p => p.OrderId == orderId).OrderByDescending(p => p.CreatedAt).ToList();
        if (payments == null || payments.Count == 0) return null;

        // Prefer any payment that is already Paid
        var paid = payments.FirstOrDefault(p => !string.IsNullOrEmpty(p.Status) && p.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase));
        var selected = paid ?? payments.First();

        return new PaymentStatusResponse
        {
            Status = selected.Status,
            OrderId = selected.OrderId,
            ProviderData = selected.ProviderData
        };
    }

    public async Task<bool> MarkPaymentAsPaidAsync(Guid paymentId, string? collectedBy)
    {
        var payment = _db.Payments.FirstOrDefault(p => p.PaymentId == paymentId);
        if (payment == null) return false;

        payment.Status = "Paid";
        // add a simple audit note to ProviderData
        var note = $"CollectedBy:{collectedBy ?? "system"} at {DateTime.UtcNow:o}";
        payment.ProviderData = string.IsNullOrEmpty(payment.ProviderData) ? note : payment.ProviderData + "\n" + note;

        _db.Payments.Update(payment);

        var order = _db.Orders.FirstOrDefault(o => o.OrderId == payment.OrderId);
        if (order != null)
        {
            order.Status = "Paid";
            _db.Orders.Update(order);

            try
            {
                var cleared = await _cartService.ClearCartAsync(order.UserId);
                Console.WriteLine($"Cleared cart for user {order.UserId} after manual collection: {cleared}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to clear cart for user {order.UserId}: {ex.Message}");
            }
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PaymentLinkResponse> CreatePaymentForOrderAsync(Guid orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Variant)
            .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order == null) throw new Exception("Order not found");

        // Generate unique OrderCode (long)
        var orderCode = Math.Abs(Guid.NewGuid().GetHashCode()) % 1000000000L; // Use Guid hash for uniqueness

        var payosRequest = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = (int)order.TotalAmount, // Assuming TotalAmount is decimal, cast to int
            Description = "Thanh toan don hang", // Max 25 chars
            BuyerName = order.BuyerName,
            BuyerPhone = order.BuyerPhone,
            BuyerAddress = order.ShippingAddress,
            // Items = order.Items.Select(oi => new PayOS.ItemData
            // {
            //     Name = oi.Variant?.Product?.Name ?? "Product",
            //     Quantity = oi.Quantity,
            //     Price = (int)oi.UnitPrice
            // }).ToList(),
            CancelUrl = $"http://localhost:3000/payment/cancel?orderId={orderId}", // Configure URLs
            ReturnUrl = $"http://localhost:3000/payment/success?orderId={orderId}",
            ExpiredAt = DateTimeOffset.Now.AddMinutes(15).ToUnixTimeSeconds(), // 15 min expiry
            // Signature will be handled by PayOS client
        };

        var result = await _payosClient.PaymentRequests.CreateAsync(payosRequest);
        var resultJson = JsonSerializer.Serialize(result);
        Console.WriteLine($"PayOS result: {resultJson}");

        // Prefer provider-returned identifier (orderCode/paymentId) when available to avoid mismatches
        string? providerAssignedId = null;
        try
        {
            using var doc = JsonDocument.Parse(resultJson);
            if (doc.RootElement.TryGetProperty("data", out var data))
            {
                if (data.TryGetProperty("orderCode", out var oc)) providerAssignedId = oc.ValueKind == JsonValueKind.Number ? oc.GetInt64().ToString() : oc.GetString();
                else if (data.TryGetProperty("paymentId", out var pid)) providerAssignedId = pid.ValueKind == JsonValueKind.Number ? pid.GetInt64().ToString() : pid.GetString();
            }
            if (string.IsNullOrEmpty(providerAssignedId))
            {
                if (doc.RootElement.TryGetProperty("orderCode", out var ocRoot)) providerAssignedId = ocRoot.ValueKind == JsonValueKind.Number ? ocRoot.GetInt64().ToString() : ocRoot.GetString();
                else if (doc.RootElement.TryGetProperty("paymentId", out var pidRoot)) providerAssignedId = pidRoot.ValueKind == JsonValueKind.Number ? pidRoot.GetInt64().ToString() : pidRoot.GetString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse PayOS result for provider id: {ex.Message}");
        }

        if (string.IsNullOrEmpty(providerAssignedId)) providerAssignedId = orderCode.ToString();

        // Save payment record
        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = orderId,
            Method = "ONLINE",
            Status = "Pending",
            ProviderPaymentId = providerAssignedId,
            ProviderData = resultJson,
            CreatedAt = DateTime.UtcNow
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        Console.WriteLine($"Saved payment with ProviderPaymentId={providerAssignedId}");

        var qrUrl = !string.IsNullOrEmpty(result.QrCode)
            ? $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={Uri.EscapeDataString(result.QrCode)}"
            : null;

        // Ensure we have a usable checkout URL. If PayOS didn't return one, try to build it from paymentLinkId.
        string? checkoutUrl = result.CheckoutUrl;
        if (string.IsNullOrEmpty(checkoutUrl))
        {
            // Try common property names via reflection for robustness
            var type = result.GetType();
            var prop = type.GetProperty("PaymentLinkId") ?? type.GetProperty("paymentLinkId") ?? type.GetProperty("paymentLink") ?? type.GetProperty("paymentLinkId");
            var paymentLinkId = prop?.GetValue(result)?.ToString();
            if (!string.IsNullOrEmpty(paymentLinkId))
            {
                checkoutUrl = $"https://pay.payos.vn/web/{paymentLinkId}";
            }
        }

        Console.WriteLine($"Payment link created. QR: {qrUrl}, Checkout: {checkoutUrl}");

        return new PaymentLinkResponse
        {
            QrCodeUrl = qrUrl,
            CheckoutUrl = checkoutUrl,
            ProviderData = JsonSerializer.Serialize(result),
            OrderCode = providerAssignedId
        };
    }
}