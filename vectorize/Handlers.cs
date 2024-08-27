using System;
using System.Collections.Generic;
using System.Linq;
using Vectorization.IntermediateRepresentation.BranchFocused;
using Vectorization.Operations;
using NLog;

namespace Vectorization.Operations.Handlers
{
        public class SubtractOperationHandler : IOperationHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string OperationName => "Subtract";

        public BranchVirExpression Optimize(BranchVirOperation operation)
        {
            Logger.Debug($"Optimizing Subtract operation with {operation.Inputs.Count} inputs");

            if (operation.Inputs.Count != 2)
            {
                Logger.Warn("Subtract operation expects exactly 2 inputs");
                return operation;
            }

            // If subtracting 0, return the first operand
            if (operation.Inputs[1] is BranchVirConstant c && (double)c.Value == 0)
                return operation.Inputs[0];

            // If both inputs are constants, compute the result
            if (operation.Inputs[0] is BranchVirConstant c1 && operation.Inputs[1] is BranchVirConstant c2)
            {
                return new BranchVirConstant { Value = (double)c1.Value - (double)c2.Value };
            }

            return operation;
        }

        public double CalculateCost(BranchVirOperation operation) => 1; // Assume subtraction has the same cost as addition
    }

    public class DivideOperationHandler : IOperationHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string OperationName => "Divide";

        public BranchVirExpression Optimize(BranchVirOperation operation)
        {
            Logger.Debug($"Optimizing Divide operation with {operation.Inputs.Count} inputs");

            if (operation.Inputs.Count != 2)
            {
                Logger.Warn("Divide operation expects exactly 2 inputs");
                return operation;
            }

            // If dividing by 1, return the first operand
            if (operation.Inputs[1] is BranchVirConstant c && (double)c.Value == 1)
                return operation.Inputs[0];

            // If both inputs are constants, compute the result
            if (operation.Inputs[0] is BranchVirConstant c1 && operation.Inputs[1] is BranchVirConstant c2)
            {
                if ((double)c2.Value == 0)
                {
                    Logger.Warn("Division by zero detected");
                    return operation; // Or handle division by zero as appropriate for your system
                }
                return new BranchVirConstant { Value = (double)c1.Value / (double)c2.Value };
            }

            return operation;
        }

        public double CalculateCost(BranchVirOperation operation) => 4; // Assume division is more expensive than multiplication
    }

    public class PowerOperationHandler : IOperationHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string OperationName => "Power";

        public BranchVirExpression Optimize(BranchVirOperation operation)
        {
            Logger.Debug($"Optimizing Power operation with {operation.Inputs.Count} inputs");

            if (operation.Inputs.Count != 2)
            {
                Logger.Warn("Power operation expects exactly 2 inputs");
                return operation;
            }

            // If raising to the power of 1, return the base
            if (operation.Inputs[1] is BranchVirConstant c && (double)c.Value == 1)
                return operation.Inputs[0];

            // If raising to the power of 0, return 1
            if (operation.Inputs[1] is BranchVirConstant c2 && (double)c2.Value == 0)
                return new BranchVirConstant { Value = 1 };

            // If both inputs are constants, compute the result
            if (operation.Inputs[0] is BranchVirConstant c1 && operation.Inputs[1] is BranchVirConstant c3)
            {
                return new BranchVirConstant { Value = Math.Pow((double)c1.Value, (double)c3.Value) };
            }

            return operation;
        }

        public double CalculateCost(BranchVirOperation operation) => 10; // Assume exponentiation is relatively expensive
    }

    public class SinOperationHandler : IOperationHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string OperationName => "Sin";

        public BranchVirExpression Optimize(BranchVirOperation operation)
        {
            Logger.Debug($"Optimizing Sin operation with {operation.Inputs.Count} inputs");

            if (operation.Inputs.Count != 1)
            {
                Logger.Warn("Sin operation expects exactly 1 input");
                return operation;
            }

            // If input is constant, compute the result
            if (operation.Inputs[0] is BranchVirConstant c)
            {
                return new BranchVirConstant { Value = Math.Sin((double)c.Value) };
            }

            return operation;
        }

        public double CalculateCost(BranchVirOperation operation) => 5; // Assume trigonometric functions are moderately expensive
    }

    public class CosOperationHandler : IOperationHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string OperationName => "Cos";

        public BranchVirExpression Optimize(BranchVirOperation operation)
        {
            Logger.Debug($"Optimizing Cos operation with {operation.Inputs.Count} inputs");

            if (operation.Inputs.Count != 1)
            {
                Logger.Warn("Cos operation expects exactly 1 input");
                return operation;
            }

            // If input is constant, compute the result
            if (operation.Inputs[0] is BranchVirConstant c)
            {
                return new BranchVirConstant { Value = Math.Cos((double)c.Value) };
            }

            return operation;
        }

        public double CalculateCost(BranchVirOperation operation) => 5; // Assume trigonometric functions are moderately expensive
    }
    
    public class MatMulBlockOperationHandler : IOperationHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string OperationName => "MatMulBlock";

        public BranchVirExpression Optimize(BranchVirOperation operation)
        {
            Logger.Debug($"Optimizing MatMulBlock operation");

            if (operation.Inputs.Count != 2)
            {
                Logger.Warn("MatMulBlock operation expects exactly 2 inputs");
                return operation;
            }

            if (!operation.Attributes.ContainsKey("BlockRow") || !operation.Attributes.ContainsKey("BlockCol"))
            {
                Logger.Warn("MatMulBlock operation is missing BlockRow or BlockCol attributes");
                return operation;
            }

            // In a real implementation, you might perform further optimizations here
            // For now, we'll just return the operation as-is
            return operation;
        }

        public double CalculateCost(BranchVirOperation operation)
        {
            // Assume the cost is proportional to the size of the matrices
            // In a real implementation, you would consider the actual dimensions of the matrices
            return 25; // Arbitrary cost, adjust based on your needs
        }
    }

    public class MatMulCombineOperationHandler : IOperationHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string OperationName => "MatMulCombine";

        public BranchVirExpression Optimize(BranchVirOperation operation)
        {
            Logger.Debug($"Optimizing MatMulCombine operation");

            if (operation.Inputs.Count != 4)
            {
                Logger.Warn("MatMulCombine operation expects exactly 4 inputs (2x2 blocks)");
                return operation;
            }

            // In a real implementation, you might perform further optimizations here
            // For now, we'll just return the operation as-is
            return operation;
        }

        public double CalculateCost(BranchVirOperation operation)
        {
            // The cost of combining should be relatively low compared to the block multiplications
            return 5; // Arbitrary cost, adjust based on your needs
        }
    }
}
