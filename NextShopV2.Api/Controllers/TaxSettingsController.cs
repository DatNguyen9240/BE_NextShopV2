using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxSettingsController : ControllerBase
    {
        private readonly ITaxSettingService _settingService;

        public TaxSettingsController(ITaxSettingService settingService)
        {
            _settingService = settingService;
        }

        /// <summary>
        /// Get all tax settings (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var settings = await _settingService.GetAllAsync();
            return ResponseHelper.Success(settings);
        }

        /// <summary>
        /// Get a specific setting value
        /// </summary>
        [HttpGet("{key}")]
        [Authorize]
        public async Task<IActionResult> GetByKey(string key)
        {
            var value = await _settingService.GetValueAsync(key);
            if (value == null)
            {
                return ResponseHelper.NotFound($"Setting '{key}' not found");
            }
            return ResponseHelper.Success(new { key, value });
        }

        /// <summary>
        /// Get current tax rate
        /// </summary>
        [HttpGet("tax-rate")]
        [Authorize]
        public async Task<IActionResult> GetTaxRate()
        {
            var rate = await _settingService.GetTaxRateAsync();
            return ResponseHelper.Success(new { 
                key = "TaxRate", 
                value = rate.ToString("0.####", CultureInfo.InvariantCulture),
                percentage = $"{rate * 100}%"
            });
        }

        /// <summary>
        /// Update or create a setting (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SetSetting([FromBody] SetSettingRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Key))
            {
                return ResponseHelper.BadRequest("Key is required");
            }

            if (string.IsNullOrWhiteSpace(request.Value))
            {
                return ResponseHelper.BadRequest("Value is required");
            }

            // Validate TaxRate if that's what's being set
            if (request.Key.Equals("TaxRate", StringComparison.OrdinalIgnoreCase))
            {
                if (!decimal.TryParse(request.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
                {
                    return ResponseHelper.BadRequest("Tax rate must be a valid decimal number");
                }

                if (rate < 0 || rate > 1)
                {
                    return ResponseHelper.BadRequest("Tax rate must be between 0 and 1 (e.g., 0.10 for 10%)");
                }
            }

            await _settingService.SetValueAsync(request.Key, request.Value, request.Description);
            return ResponseHelper.Success(new { key = request.Key, value = request.Value }, $"Setting '{request.Key}' updated successfully");
        }

        /// <summary>
        /// Update tax rate (Admin only) - convenient endpoint
        /// </summary>
        [HttpPost("tax-rate")]
        [Authorize]
        public async Task<IActionResult> SetTaxRate([FromBody] SetTaxRateRequest request)
        {
            if (request.Rate < 0 || request.Rate > 1)
            {
                return ResponseHelper.BadRequest("Tax rate must be between 0 and 1 (e.g., 0.10 for 10%)");
            }

            await _settingService.SetValueAsync(
                "TaxRate", 
                request.Rate.ToString("0.####", CultureInfo.InvariantCulture),
                "Default tax rate for orders (VAT)"
            );

            return ResponseHelper.Success(new { 
                rate = request.Rate,
                percentage = $"{request.Rate * 100}%"
            }, "Tax rate updated successfully");
        }

        /// <summary>
        /// Delete a setting (Admin only)
        /// </summary>
        [HttpDelete("{key}")]
        [Authorize]
        public async Task<IActionResult> Delete(string key)
        {
            await _settingService.DeleteAsync(key);
            return ResponseHelper.Success($"Setting '{key}' deleted successfully");
        }
    }

    public class SetSettingRequest
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class SetTaxRateRequest
    {
        public decimal Rate { get; set; }
    }
}
