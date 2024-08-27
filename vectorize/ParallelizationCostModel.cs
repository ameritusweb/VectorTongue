using System;
using System.Collections.Generic;
using System.Linq;
using Vectorization.IntermediateRepresentation.BranchFocused;

namespace Vectorization.Optimization
{
    public class ParallelizationCostModel
    {
        private readonly Dictionary<string, double> _operationCosts = new Dictionary<string, double>
        {
            { "Add", 1 },
            { "Subtract", 1 },
            { "Multiply", 2 },
            { "Divide", 4 },
            { "MatMul", 10 },
            { "Exp", 5 },
            { "Log", 5 },
            { "Sin", 6 },
            { "Cos", 6 },
            { "Tan", 7 },
            // Add more operations and their relative costs
        };

        private const double ParallelizationOverhead = 5;
        private const double CommunicationCost = 2;

        public bool IsParallelizationBeneficial(BranchVirOperation parallelOp)
        {
            if (parallelOp.OperationName != "Parallel")
            {
                throw new ArgumentException("Operation must be a Parallel operation");
            }

            double serialCost = CalculateSerialCost(parallelOp.Inputs);
            double parallelCost = CalculateParallelCost(parallelOp);

            return parallelCost < serialCost;
        }

        private double CalculateSerialCost(List<BranchVirExpression> expressions)
        {
            return expressions.Sum(expr => CalculateExpressionCost(expr));
        }

        private double CalculateParallelCost(BranchVirOperation parallelOp)
        {
            double maxGroupCost = parallelOp.Inputs
                .Cast<BranchVirOperation>()
                .Where(op => op.OperationName == "ParallelGroup")
                .Max(group => CalculateSerialCost(group.Inputs));

            double combinationCost = CalculateExpressionCost((BranchVirExpression)parallelOp.Attributes["Combination"]);

            return maxGroupCost + ParallelizationOverhead + CommunicationCost + combinationCost;
        }

        private double CalculateExpressionCost(BranchVirExpression expr)
        {
            switch (expr)
            {
                case BranchVirOperation op:
                    return _operationCosts.GetValueOrDefault(op.OperationName, 1) + op.Inputs.Sum(CalculateExpressionCost);
                case BranchVirVariable _:
                case BranchVirConstant _:
                    return 0;
                default:
                    return 1;
            }
        }
    }
}