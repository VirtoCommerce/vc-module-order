using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrdersModule.Web.Validation;

/// <summary>
/// Validates that the total number of child documents (operations) per order 
/// does not exceed the configured maximum limit.
/// Uses the IOperation.ChildrenOperations tree structure for generic traversal.
/// This ensures system performance, storage optimization, and data consistency.
/// 
/// This validator works with IOperation interface, making it applicable to any operation type,
/// though it's primarily designed for root-level operations like CustomerOrder.
/// </summary>
public class OrderDocumentCountValidator : AbstractValidator<IOperation>
{
    private readonly ISettingsManager _settingsManager;

    public OrderDocumentCountValidator(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;

        RuleFor(operation => operation)
            .CustomAsync(async (operation, context, cancellationToken) =>
            {
                // Only validate root-level operations (like CustomerOrder) that have children
                // Skip validation for child operations to avoid redundant checks
                if (!string.IsNullOrEmpty(operation.ParentOperationId))
                {
                    return;
                }

                var maxDocumentCount = await _settingsManager.GetValueAsync<int>(
                    ModuleConstants.Settings.General.MaxOrderDocumentCount);

                // Get all operations in the tree (excluding the root operation itself)
                var allOperations = operation.GetFlatObjectsListWithInterface<IOperation>().ToList();
                var childOperations = allOperations.Where(op => op.Id != operation.Id).ToList();

                // Total child documents count
                var totalDocumentCount = childOperations.Count;

                if (totalDocumentCount > maxDocumentCount)
                {
                    var operationBreakdown = GetOperationBreakdown(childOperations);
                    
                    context.AddFailure(
                        operation.OperationType,
                        $"{operation.OperationType} document count ({totalDocumentCount}) exceeds the maximum allowed limit of {maxDocumentCount}. " +
                        $"Documents breakdown: {operationBreakdown}");
                }

                // Validate each operation that has children
                ValidateOperationChildren(childOperations, maxDocumentCount, context);
            });
    }

    /// <summary>
    /// Creates a human-readable breakdown of operations by type
    /// </summary>
    private static string GetOperationBreakdown(IList<IOperation> operations)
    {
        var grouped = operations
            .GroupBy(op => op.OperationType)
            .Select(g => $"{g.Key}={g.Count()}")
            .OrderBy(s => s);

        return string.Join(", ", grouped);
    }

    /// <summary>
    /// Validates that individual operations do not have too many child operations
    /// </summary>
    private static void ValidateOperationChildren(
        IList<IOperation> allOperations, 
        int maxDocumentCount, 
        ValidationContext<IOperation> context)
    {
        // Group operations by their parent to count children per operation
        var operationsByParent = allOperations
            .Where(op => !string.IsNullOrEmpty(op.ParentOperationId))
            .GroupBy(op => op.ParentOperationId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Check each operation's child count
        foreach (var operation in allOperations)
        {
            if (operationsByParent.TryGetValue(operation.Id, out var children))
            {
                var childCount = children.Count;
                
                if (childCount > maxDocumentCount)
                {
                    var childBreakdown = GetOperationBreakdown(children);
                    
                    context.AddFailure(
                        operation.OperationType,
                        $"{operation.OperationType} '{operation.Number}' has {childCount} child documents ({childBreakdown}), " +
                        $"which exceeds the maximum allowed limit of {maxDocumentCount}.");
                }
            }
        }
    }
}

