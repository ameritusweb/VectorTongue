using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Vectorization.IntermediateRepresentation.BranchFocused;

namespace Vectorization.Optimization
{
    public static class ExpressionKeyGenerator
    {
        public static string GenerateKey(BranchVirExpression expression)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashInput = GenerateHashInput(expression);
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
                return Convert.ToBase64String(hashBytes);
            }
        }

        private static string GenerateHashInput(BranchVirExpression expression)
        {
            return expression switch
            {
                BranchVirConstant constant => $"C:{constant.Value}",
                BranchVirVariable variable => $"V:{variable.Name}",
                BranchVirOperation operation => GenerateOperationHashInput(operation),
                _ => throw new ArgumentException($"Unsupported expression type: {expression.GetType()}")
            };
        }

        private static string GenerateOperationHashInput(BranchVirOperation operation)
        {
            var inputHashes = operation.Inputs.Select(GenerateHashInput);
            return $"O:{operation.OperationName}({string.Join(",", inputHashes)})";
        }
    }
}

// Usage in the optimizer:
public class CostModelIntegratedParallelismAwareBranchVirOptimizer
{
    private Dictionary<string, BranchVirExpression> _expressionCache = new Dictionary<string, BranchVirExpression>();

    // ... other methods ...

    private BranchVirExpression OptimizeExpression(BranchVirExpression expression)
    {
        var key = ExpressionKeyGenerator.GenerateKey(expression);
        if (_expressionCache.TryGetValue(key, out var cachedExpression))
        {
            return cachedExpression;
        }

        var optimizedExpression = expression switch
        {
            BranchVirOperation operation => OptimizeOperation(operation),
            // ... other cases ...
        };

        _expressionCache[key] = optimizedExpression;
        return optimizedExpression;
    }

    // ... other methods ...
}