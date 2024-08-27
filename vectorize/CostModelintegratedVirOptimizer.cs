using System;
using System.Collections.Generic;
using System.Linq;
using Vectorization.IntermediateRepresentation.BranchFocused;
using Vectorization.Operations;
using NLog;

namespace Vectorization.Optimization
{
    public partial class CostModelIntegratedParallelismAwareBranchVirOptimizer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AdvancedInputGroupingStrategy _groupingStrategy;

        public CostModelIntegratedParallelismAwareBranchVirOptimizer(ParallelizationCostModel costModel, int maxParallelism)
        {
            _costModel = costModel;
            _groupingStrategy = new AdvancedInputGroupingStrategy(maxParallelism);
        }

        private BranchVirExpression CreateParallelOperation(BranchVirOperation operation)
        {
            Logger.Info($"Creating parallel operation for {operation.OperationName}");

            var parallelGroups = _groupingStrategy.GroupInputsForParallelExecution(operation.Inputs, operation.OperationName);

            if (parallelGroups.Count <= 1)
            {
                Logger.Info("No parallelization opportunity found, returning original operation");
                return operation;
            }

            var parallelOperations = parallelGroups.Select(group => new BranchVirOperation
            {
                OperationName = operation.OperationName,
                Inputs = group,
                Attributes = operation.Attributes // Preserve any attributes, like block indices for MatMul
            }).ToList();

            var parallelOperation = new BranchVirOperation
            {
                OperationName = "Parallel",
                Inputs = parallelOperations
            };

            // Create a combining operation to merge results of parallel operations
            var combiningOperation = CreateCombiningOperation(operation.OperationName, parallelGroups.Count);

            return new BranchVirOperation
            {
                OperationName = "SequentialComposition",
                Inputs = new List<BranchVirExpression> { parallelOperation, combiningOperation }
            };
        }

        private BranchVirExpression CreateCombiningOperation(string originalOperationName, int groupCount)
        {
            switch (originalOperationName)
            {
                case "Add":
                case "Multiply":
                    // For associative operations, we can simply apply the operation to all results
                    return new BranchVirOperation
                    {
                        OperationName = originalOperationName,
                        Inputs = Enumerable.Range(0, groupCount)
                            .Select(i => new BranchVirVariable { Name = $"parallelResult_{i}" })
                            .Cast<BranchVirExpression>()
                            .ToList()
                    };

                case "MatMul":
                    // For matrix multiplication, we need to combine the block results
                    return new BranchVirOperation
                    {
                        OperationName = "MatMulCombine",
                        Inputs = Enumerable.Range(0, groupCount)
                            .Select(i => new BranchVirVariable { Name = $"parallelResult_{i}" })
                            .Cast<BranchVirExpression>()
                            .ToList()
                    };

                case "Subtract":
                case "Divide":
                    // For non-associative operations, we need to apply the operation sequentially
                    return CreateSequentialCombiningOperation(originalOperationName, groupCount);

                case "Sin":
                case "Cos":
                case "Exp":
                case "Log":
                    // For element-wise operations, we can concatenate the results
                    return new BranchVirOperation
                    {
                        OperationName = "Concatenate",
                        Inputs = Enumerable.Range(0, groupCount)
                            .Select(i => new BranchVirVariable { Name = $"parallelResult_{i}" })
                            .Cast<BranchVirExpression>()
                            .ToList()
                    };

                default:
                    Logger.Warn($"No specific combining operation defined for {originalOperationName}. Using default concatenation.");
                    return new BranchVirOperation
                    {
                        OperationName = "Concatenate",
                        Inputs = Enumerable.Range(0, groupCount)
                            .Select(i => new BranchVirVariable { Name = $"parallelResult_{i}" })
                            .Cast<BranchVirExpression>()
                            .ToList()
                    };
            }
        }

        private BranchVirExpression CreateSequentialCombiningOperation(string operationName, int groupCount)
        {
            var result = new BranchVirVariable { Name = "parallelResult_0" };
            for (int i = 1; i < groupCount; i++)
            {
                result = new BranchVirOperation
                {
                    OperationName = operationName,
                    Inputs = new List<BranchVirExpression>
                    {
                        result,
                        new BranchVirVariable { Name = $"parallelResult_{i}" }
                    }
                };
            }
            return result;
        }
    }
}