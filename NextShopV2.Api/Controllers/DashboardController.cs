using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.Interfaces;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
        private readonly INotificationHistoryRepository _notificationHistoryRepository;
        private readonly ITrackingEventRepository _trackingEventRepository;

        public DashboardController(
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IProductRepository productRepository,
            IInventoryTransactionRepository inventoryTransactionRepository,
            INotificationHistoryRepository notificationHistoryRepository,
            ITrackingEventRepository trackingEventRepository)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _productRepository = productRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _notificationHistoryRepository = notificationHistoryRepository;
            _trackingEventRepository = trackingEventRepository;
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            var now = DateTime.UtcNow;
            var startOfDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // User metrics
            var totalUsers = await _userRepository.GetTotalCountAsync();
            var newUsersToday = await _userRepository.GetCountByDateRangeAsync(startOfDay, now);
            var newUsersThisMonth = await _userRepository.GetCountByDateRangeAsync(startOfMonth, now);

            // Order metrics
            var totalOrders = await _orderRepository.GetTotalCountAsync();
            var ordersToday = await _orderRepository.GetCountByDateRangeAsync(startOfDay, now);
            var ordersThisMonth = await _orderRepository.GetCountByDateRangeAsync(startOfMonth, now);
            var pendingOrders = await _orderRepository.GetCountByStatusAsync("Pending");
            var completedOrders = await _orderRepository.GetCountByStatusAsync("Completed");

            // Revenue metrics
            var totalRevenue = await _paymentRepository.GetTotalRevenueAsync();
            var revenueToday = await _paymentRepository.GetRevenueByDateRangeAsync(startOfDay, now);
            var revenueThisMonth = await _paymentRepository.GetRevenueByDateRangeAsync(startOfMonth, now);

            // Product metrics
            var totalProducts = await _productRepository.GetTotalCountAsync();
            var lowStockProducts = await _productRepository.GetLowStockCountAsync(10); // Assuming threshold

            // Inventory transactions
            var inventoryChangesToday = await _inventoryTransactionRepository.GetCountByDateRangeAsync(startOfDay, now);

            // Notifications
            var notificationsSentToday = await _notificationHistoryRepository.GetCountByDateRangeAsync(startOfDay, now);

            return Ok(new
            {
                Users = new { Total = totalUsers, NewToday = newUsersToday, NewThisMonth = newUsersThisMonth },
                Orders = new { Total = totalOrders, Today = ordersToday, ThisMonth = ordersThisMonth, Pending = pendingOrders, Completed = completedOrders },
                Revenue = new { Total = totalRevenue, Today = revenueToday, ThisMonth = revenueThisMonth },
                Products = new { Total = totalProducts, LowStock = lowStockProducts },
                Inventory = new { ChangesToday = inventoryChangesToday },
                Notifications = new { SentToday = notificationsSentToday }
            });
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int limit = 50)
        {
            var logs = new
            {
                InventoryTransactions = await _inventoryTransactionRepository.GetRecentAsync(limit),
                NotificationHistory = await _notificationHistoryRepository.GetRecentAsync(limit),
                TrackingEvents = await _trackingEventRepository.GetRecentAsync(limit)
            };

            return Ok(logs);
        }

        [HttpGet("performance")]
        public IActionResult GetPerformanceMetrics()
        {
            // Placeholder for performance metrics - integrate with actual monitoring
            // In a real implementation, use metrics from ILogger or APM tools
            return Ok(new
            {
                ResponseTimeAvg = 150, // ms
                ErrorRate = 0.02, // 2%
                DatabaseConnections = 10,
                CacheHitRate = 0.85
            });
        }
    }
}