using System;
using System.Collections.Generic;

namespace NextShopV2.Shared.Interfaces
{
    /// <summary>
    /// Centralized logging service with structured logging patterns
    /// </summary>
    public interface ILoggingService
    {
        // Generic logging methods
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
        void LogDebug(string message, params object[] args);

        // Entity-specific logging patterns
        void LogEntityCreated(string entityType, object entityId, object? additionalData = null);
        void LogEntityUpdated(string entityType, object entityId, object? additionalData = null);
        void LogEntityDeleted(string entityType, object entityId, object? additionalData = null);
        void LogEntityNotFound(string entityType, object entityId);

        // Operation-specific logging patterns
        void LogOperationStarted(string operationType, object? context = null);
        void LogOperationCompleted(string operationType, object? result = null);
        void LogOperationFailed(string operationType, Exception exception, object? context = null);

        // Image/File operations
        void LogImageDeleted(string publicId, string entityType, object entityId);
        void LogImageDeletionFailed(string publicId, string entityType, object entityId, Exception exception);
        void LogBulkOperation(string operationType, int totalCount, int successCount, int failureCount);

        // Validation/Business rule logging
        void LogValidationFailure(string validationType, object context);
        void LogConflictResolved(string conflictType, object oldValue, object newValue, object context);
    }
}