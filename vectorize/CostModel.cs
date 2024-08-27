using System;
using System.Collections.Generic;
using System.Linq;
using Vectorization.IntermediateRepresentation.BranchFocused;

namespace Vectorization.Optimization
{
    public class CostModelIntegratedParallelismAwareBranchVirOptimizer : EnhancedParallelismAwareBranchVirOptimizer
    {
        private readonly ParallelizationCostModel _costModel = new ParallelizationCostModel();

        protected override BranchVirExpression OptimizeBranch(BranchVirBranch branch)
        {
            var optimizedBranch = base.OptimizeBranch(branch);
            
            if (optimizedBranch is BranchVirOperation parallelOp && parallelOp.OperationName == "Parallel")
            {
                if (_costModel.IsParallelizationBeneficial(parallelOp))
                {
                    return parallelOp;
                }
                else
                {
                    // If parallelization is not beneficial, revert to serial execution
                    return new BranchVirOperation
                    {
                        OperationName = "Serial",
                        Inputs = parallelOp.Inputs.SelectMany(group => ((BranchVirOperation)group).Inputs).ToList(),
                        Attributes = parallelOp.Attributes
                    };
                }
            }

            return optimizedBranch;
        }
    }
}