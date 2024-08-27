using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vectorization.IntermediateRepresentation.BranchFocused;

namespace Vectorization.Operations
{
    public interface IOperationHandler
    {
        string OperationName { get; }
        BranchVirExpression Optimize(BranchVirOperation operation);
        double CalculateCost(BranchVirOperation operation);
    }

    public class OperationRegistry
    {
        private static readonly Dictionary<string, IOperationHandler> _handlers = new Dictionary<string, IOperationHandler>();

        public static void RegisterHandler(IOperationHandler handler)
        {
            _handlers[handler.OperationName] = handler;
        }

        public static IOperationHandler GetHandler(string operationName)
        {
            if (_handlers.TryGetValue(operationName, out var handler))
            {
                return handler;
            }
            throw new ArgumentException($"No handler registered for operation: {operationName}");
        }

        public static void DiscoverHandlers()
        {
            var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(IOperationHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var handlerType in handlerTypes)
            {
                var handler = (IOperationHandler)Activator.CreateInstance(handlerType);
                RegisterHandler(handler);
            }
        }
    }
}

// Example usage in the optimizer:
public class CostModelIntegratedParallelismAwareBranchVirOptimizer
{
    // ... other methods ...

    private BranchVirExpression OptimizeOperation(BranchVirOperation operation)
    {
        var handler = OperationRegistry.GetHandler(operation.OperationName);
        return handler.Optimize(operation);
    }

    // ... other methods ...
}

// Example of a new operation handler:
public class MatMulOperationHandler : IOperationHandler
{
    public string OperationName => "MatMul";

    public BranchVirExpression Optimize(BranchVirOperation operation)
    {
        // Implement optimization logic for matrix multiplication
        // ...
    }

    public double CalculateCost(BranchVirOperation operation)
    {
        // Implement cost calculation for matrix multiplication
        // ...
    }
}