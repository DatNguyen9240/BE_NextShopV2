using NextShopV2.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace NextShopV2.Shared.Services
{
    /// <summary>
    /// Centralized logging service with consistent patterns and structured logging
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        #region Generic Logging Methods
        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
        }
        #endregion

        #region Entity Operations
        public void LogEntityCreated(string entityType, object entityId, object? additionalData = null)
        {
            if (additionalData != null)
                _logger.LogInformation("{EntityType} {EntityId} created successfully. Data: {@AdditionalData}", entityType, entityId, additionalData);
            else
                _logger.LogInformation("{EntityType} {EntityId} created successfully", entityType, entityId);
        }

        public void LogEntityUpdated(string entityType, object entityId, object? additionalData = null)
        {
            if (additionalData != null)
                _logger.LogInformation("{EntityType} {EntityId} updated successfully. Data: {@AdditionalData}", entityType, entityId, additionalData);
            else
                _logger.LogInformation("{EntityType} {EntityId} updated successfully", entityType, entityId);
        }

        public void LogEntityDeleted(string entityType, object entityId, object? additionalData = null)
        {
            if (additionalData != null)
                _logger.LogInformation("{EntityType} {EntityId} deleted successfully. Data: {@AdditionalData}", entityType, entityId, additionalData);
            else
                _logger.LogInformation("{EntityType} {EntityId} deleted successfully", entityType, entityId);
        }

        public void LogEntityNotFound(string entityType, object entityId)
        {
            _logger.LogWarning("{EntityType} {EntityId} not found", entityType, entityId);
        }
        #endregion

        #region Operation Patterns
        public void LogOperationStarted(string operationType, object? context = null)
        {
            if (context != null)
                _logger.LogInformation("{OperationType} operation started. Context: {@Context}", operationType, context);
            else
                _logger.LogInformation("{OperationType} operation started", operationType);
        }

        public void LogOperationCompleted(string operationType, object? result = null)
        {
            if (result != null)
                _logger.LogInformation("{OperationType} operation completed successfully. Result: {@Result}", operationType, result);
            else
                _logger.LogInformation("{OperationType} operation completed successfully", operationType);
        }

        public void LogOperationFailed(string operationType, Exception exception, object? context = null)
        {
            if (context != null)
                _logger.LogError(exception, "{OperationType} operation failed. Context: {@Context}", operationType, context);
            else
                _logger.LogError(exception, "{OperationType} operation failed", operationType);
        }
        #endregion

        #region Image/File Operations
        public void LogImageDeleted(string publicId, string entityType, object entityId)
        {
            _logger.LogInformation("Image {PublicId} deleted successfully for {EntityType} {EntityId}", publicId, entityType, entityId);
        }

        public void LogImageDeletionFailed(string publicId, string entityType, object entityId, Exception exception)
        {
            _logger.LogWarning(exception, "Failed to delete image {PublicId} for {EntityType} {EntityId}", publicId, entityType, entityId);
        }

        public void LogBulkOperation(string operationType, int totalCount, int successCount, int failureCount)
        {
            _logger.LogInformation("{OperationType} bulk operation completed. Total: {TotalCount}, Success: {SuccessCount}, Failed: {FailureCount}", 
                operationType, totalCount, successCount, failureCount);
        }
        #endregion

        #region Business Logic
        public void LogValidationFailure(string validationType, object context)
        {
            _logger.LogWarning("{ValidationType} validation failed. Context: {@Context}", validationType, context);
        }

        public void LogConflictResolved(string conflictType, object oldValue, object newValue, object context)
        {
            _logger.LogInformation("{ConflictType} conflict resolved. Changed from {OldValue} to {NewValue}. Context: {@Context}", 
                conflictType, oldValue, newValue, context);
        }
        #endregion
    }
}