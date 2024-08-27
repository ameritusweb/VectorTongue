using System;
using System.Collections.Generic;
using System.Linq;
using Vectorization.IntermediateRepresentation.BranchFocused;

namespace Vectorization.Optimization
{
    public class BranchVirOptimizer
    {
        private Dictionary<string, BranchVirExpression> _expressionCache = new Dictionary<string, BranchVirExpression>();
        private int _tempVarCounter = 0;

        public BranchVirFunction Optimize(BranchVirFunction function)
        {
            _expressionCache.Clear();
            _tempVarCounter = 0;

            var optimizedBody = OptimizeExpression(function.Body);
            return new BranchVirFunction
            {
                Name = function.Name,
                Parameters = function.Parameters,
                Body = optimizedBody
            };
        }

        private BranchVirExpression OptimizeExpression(BranchVirExpression expression)
        {
            switch (expression)
            {
                case BranchVirOperation operation:
                    return OptimizeOperation(operation);
                case BranchVirBranch branch:
                    return OptimizeBranch(branch);
                case BranchVirVariable variable:
                case BranchVirConstant constant:
                    return expression;
                default:
                    throw new NotImplementedException($"Optimization for {expression.GetType()} is not implemented.");
            }
        }

        private BranchVirExpression OptimizeOperation(BranchVirOperation operation)
        {
            var optimizedInputs = operation.Inputs.Select(OptimizeExpression).ToList();
            var newOperation = new BranchVirOperation
            {
                OperationName = operation.OperationName,
                Inputs = optimizedInputs
            };

            // Common Subexpression Elimination
            var key = GetExpressionKey(newOperation);
            if (_expressionCache.TryGetValue(key, out var cachedExpression))
            {
                return cachedExpression;
            }

            _expressionCache[key] = newOperation;
            return newOperation;
        }

        private BranchVirExpression OptimizeBranch(BranchVirBranch branch)
        {
            var optimizedSource = OptimizeExpression(branch.Source);
            var optimizedBranches = branch.Branches.Select(OptimizeExpression).ToList();
            var optimizedCombination = OptimizeExpression(branch.Combination);

            // Check if we can merge this branch with a parent branch
            if (optimizedSource is BranchVirBranch parentBranch)
            {
                return MergeBranches(parentBranch, optimizedBranches, optimizedCombination);
            }

            // Check for common operations across branches
            var commonOperation = FindCommonOperation(optimizedBranches);
            if (commonOperation != null)
            {
                return ExtractCommonOperation(optimizedSource, commonOperation, optimizedBranches, optimizedCombination);
            }

            // Check for similar branch structures
            var mergedBranches = MergeSimilarBranches(optimizedBranches);
            if (mergedBranches.Count < optimizedBranches.Count)
            {
                return new BranchVirBranch
                {
                    Source = optimizedSource,
                    Branches = mergedBranches,
                    Combination = optimizedCombination
                };
            }

            return new BranchVirBranch
            {
                Source = optimizedSource,
                Branches = optimizedBranches,
                Combination = optimizedCombination
            };
        }

        private BranchVirBranch MergeBranches(BranchVirBranch parentBranch, List<BranchVirExpression> childBranches, BranchVirExpression childCombination)
        {
            var mergedBranches = new List<BranchVirExpression>();
            foreach (var parentBranchExpression in parentBranch.Branches)
            {
                mergedBranches.AddRange(childBranches.Select(childBranch =>
                    new BranchVirOperation
                    {
                        OperationName = "Compose",
                        Inputs = new List<BranchVirExpression> { parentBranchExpression, childBranch }
                    }));
            }

            return new BranchVirBranch
            {
                Source = parentBranch.Source,
                Branches = mergedBranches,
                Combination = new BranchVirOperation
                {
                    OperationName = "Compose",
                    Inputs = new List<BranchVirExpression> { parentBranch.Combination, childCombination }
                }
            };
        }

        private BranchVirOperation FindCommonOperation(List<BranchVirExpression> branches)
        {
            if (branches.All(b => b is BranchVirOperation))
            {
                var operations = branches.Cast<BranchVirOperation>().ToList();
                var firstOp = operations.First();
                if (operations.All(op => op.OperationName == firstOp.OperationName))
                {
                    return new BranchVirOperation
                    {
                        OperationName = firstOp.OperationName,
                        Inputs = new List<BranchVirExpression> { new BranchVirVariable { Name = "x" } }
                    };
                }
            }
            return null;
        }

        private BranchVirExpression ExtractCommonOperation(BranchVirExpression source, BranchVirOperation commonOp, List<BranchVirExpression> branches, BranchVirExpression combination)
        {
            var innerBranches = branches.Cast<BranchVirOperation>().Select(op => op.Inputs[0]).ToList();
            return new BranchVirOperation
            {
                OperationName = commonOp.OperationName,
                Inputs = new List<BranchVirExpression>
                {
                    new BranchVirBranch
                    {
                        Source = source,
                        Branches = innerBranches,
                        Combination = combination
                    }
                }
            };
        }

        private List<BranchVirExpression> MergeSimilarBranches(List<BranchVirExpression> branches)
        {
            var mergedBranches = new List<BranchVirExpression>();
            var groupedBranches = branches.GroupBy(GetExpressionKey).ToList();

            foreach (var group in groupedBranches)
            {
                if (group.Count() > 1)
                {
                    mergedBranches.Add(new BranchVirOperation
                    {
                        OperationName = "MergedBranch",
                        Inputs = group.ToList()
                    });
                }
                else
                {
                    mergedBranches.Add(group.First());
                }
            }

            return mergedBranches;
        }

        private string GetExpressionKey(BranchVirExpression expression)
        {
            // Implement a method to generate a unique key for each expression
            // This could be a hash of the expression's structure and values
            // For simplicity, we'll use ToString() here, but you might want to implement a more robust solution
            return expression.ToString();
        }

        private string GetTempVarName() => $"temp{_tempVarCounter++}";
    }
}