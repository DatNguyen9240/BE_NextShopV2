using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using NextShopV2.Shared.Interfaces;
using NextShopV2.Application.DTOs.Request.CreateDto;
using NextShopV2.Api.Attributes;

namespace NextShopV2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PayOSClient _payosClient;
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _config;

    public PaymentsController(PayOSClient payosClient, IPaymentService paymentService, IConfiguration config)
    {
        _payosClient = payosClient;
        _paymentService = paymentService;
        _config = config;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.OrderCode) || request.Amount <= 0)
        {
            return BadRequest("Invalid request data");
        }

        try
        {
            var payosRequest = new CreatePaymentLinkRequest
            {
                OrderCode = long.Parse(request.OrderCode),
                Amount = request.Amount,
                Description = request.Description,
                BuyerName = request.BuyerName,
                BuyerEmail = request.BuyerEmail,
                BuyerPhone = request.BuyerPhone,
                BuyerAddress = request.BuyerAddress,
                // Items = request.Items.Select(i => new PayOS.ItemData { Name = i.Name, Quantity = i.Quantity, Price = i.Price }).ToList(),
                CancelUrl = request.CancelUrl!,
                ReturnUrl = request.ReturnUrl!,
                ExpiredAt = request.ExpiredAt,
                Signature = request.Signature
            };

            var result = await _payosClient.PaymentRequests.CreateAsync(payosRequest);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create payment link", error = ex.Message });
        }
    }

    [HttpGet("{paymentLinkId}")]
    public async Task<IActionResult> GetPayment(string paymentLinkId)
    {
        if (string.IsNullOrWhiteSpace(paymentLinkId))
        {
            return BadRequest("paymentLinkId is required");
        }

        try
        {
            var result = await _payosClient.PaymentRequests.GetAsync(paymentLinkId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to retrieve payment link", error = ex.Message });
        }
    }

    // Webhook endpoint for PayOS notifications
    [HttpPost("webhook/payos")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOSWebhook()
    {
        // Read raw request body
        using var reader = new System.IO.StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        Console.WriteLine($"PayOS Webhook received: {body}");

        // Optional: validate checksum header if provided
        var checksumKey = _config["PayOS:ChecksumKey"] ?? Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY");
        string? signature = null;
        if (Request.Headers.ContainsKey("X-Checksum")) signature = Request.Headers["X-Checksum"].ToString();
        if (Request.Headers.ContainsKey("X-Signature")) signature ??= Request.Headers["X-Signature"].ToString();

        var success = await _paymentService.HandlePayOSWebhookAsync(body, signature, checksumKey ?? string.Empty);
        Console.WriteLine($"Webhook processing success: {success}");
        // Return 200 only if processing succeeded (so provider won't assume success when DB wasn't updated)
        if (success)
        {
            return Ok(new { success = true, message = "Payment updated successfully" });
        }
        else
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Failed to process webhook" });
        }
    }

    [HttpPost("create-order-payment")]
    public async Task<IActionResult> CreateOrderPayment([FromBody] CreateOrderPaymentRequest request)
    {
        Console.WriteLine($"CreateOrderPayment called with OrderId: {request?.OrderId}");
        if (request == null || string.IsNullOrEmpty(request.OrderId) || !Guid.TryParse(request.OrderId, out var orderId))
        {
            Console.WriteLine("Invalid request: request is null or OrderId is invalid");
            return BadRequest("Invalid request");
        }

        try
        {
            var paymentLink = await _paymentService.CreatePaymentForOrderAsync(orderId);
            Console.WriteLine($"CreateOrderPayment result: qrCodeUrl={paymentLink.QrCodeUrl}, checkoutUrl={paymentLink.CheckoutUrl}, orderCode={paymentLink.OrderCode}");
            return Ok(new { qrCodeUrl = paymentLink.QrCodeUrl, checkoutUrl = paymentLink.CheckoutUrl, orderCode = paymentLink.OrderCode });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in CreateOrderPayment: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("status/{orderId}")]
    public IActionResult GetPaymentStatusByOrderId(string orderId)
    {
        if (!Guid.TryParse(orderId, out var id))
        {
            return BadRequest("Invalid orderId");
        }

        var status = _paymentService.GetPaymentStatusByOrderId(id);
        if (status == null)
        {
            return NotFound("Payment not found");
        }

        // Prevent caching of status responses
        Response.Headers["Cache-Control"] = "no-store";
        return Ok(status);
    }

    public class CollectPaymentRequest { public string? CollectedBy { get; set; } }

    [HttpPost("{paymentId}/collect")]
    [AdminOnly]
    public async Task<IActionResult> CollectPayment(Guid paymentId, [FromBody] CollectPaymentRequest request)
    {
        var success = await _paymentService.MarkPaymentAsPaidAsync(paymentId, request?.CollectedBy);
        if (success)
            return Ok(new { success = true, message = "Payment marked as collected" });
        return NotFound(new { success = false, message = "Payment not found" });
    }

    [HttpGet("status")]
    public IActionResult GetPaymentStatusByOrderCode([FromQuery] string orderCode)
    {
        if (string.IsNullOrEmpty(orderCode)) return BadRequest("orderCode is required");

        var status = _paymentService.GetPaymentStatus(orderCode);
        if (status == null) return NotFound("Payment not found");

        // Prevent caching of status responses
        Response.Headers["Cache-Control"] = "no-store";
        return Ok(status);
    }
}
