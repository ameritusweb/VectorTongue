using System;
using System.Collections.Generic;
using System.Linq;
using Vectorization.IntermediateRepresentation.BranchFocused;
using NLog;

namespace Vectorization.Optimization
{
    public class AdvancedInputGroupingStrategy
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly int _maxParallelism;

        public AdvancedInputGroupingStrategy(int maxParallelism)
        {
            _maxParallelism = maxParallelism;
        }

        public List<List<BranchVirExpression>> GroupInputsForParallelExecution(List<BranchVirExpression> inputs, string operationName)
        {
            Logger.Debug($"Grouping inputs for parallel execution of {operationName}");

            var dependencies = AnalyzeDependencies(inputs);
            var executionTimes = EstimateExecutionTimes(inputs);

            return operationName switch
            {
                "Add" or "Multiply" => GroupAssociativeOperation(inputs, dependencies, executionTimes),
                "MatMul" => GroupMatrixMultiplication(inputs, dependencies, executionTimes),
                _ => GroupGenericOperation(inputs, dependencies, executionTimes)
            };
        }

        private Dictionary<BranchVirExpression, HashSet<BranchVirExpression>> AnalyzeDependencies(List<BranchVirExpression> inputs)
        {
            var dependencies = new Dictionary<BranchVirExpression, HashSet<BranchVirExpression>>();

            foreach (var input in inputs)
            {
                dependencies[input] = new HashSet<BranchVirExpression>();
                CollectDependencies(input, dependencies[input]);
            }

            return dependencies;
        }

        private void CollectDependencies(BranchVirExpression expression, HashSet<BranchVirExpression> dependencies)
        {
            switch (expression)
            {
                case BranchVirOperation operation:
                    foreach (var input in operation.Inputs)
                    {
                        dependencies.Add(input);
                        CollectDependencies(input, dependencies);
                    }
                    break;
                case BranchVirVariable:
                    dependencies.Add(expression);
                    break;
                // Add cases for other expression types if needed
            }
        }

        private Dictionary<BranchVirExpression, double> EstimateExecutionTimes(List<BranchVirExpression> inputs)
        {
            var executionTimes = new Dictionary<BranchVirExpression, double>();

            foreach (var input in inputs)
            {
                executionTimes[input] = EstimateExecutionTime(input);
            }

            return executionTimes;
        }

        private double EstimateExecutionTime(BranchVirExpression expression)
        {
            // This is a simplified estimation. In a real-world scenario, you would need
            // a more sophisticated model based on operation complexity and possibly profiling data.
            return expression switch
            {
                BranchVirOperation operation => operation.Inputs.Sum(EstimateExecutionTime) + 1,
                BranchVirVariable => 0.1,
                BranchVirConstant => 0.05,
                _ => 1 // Default case
            };
        }

        private List<List<BranchVirExpression>> GroupAssociativeOperation(
            List<BranchVirExpression> inputs,
            Dictionary<BranchVirExpression, HashSet<BranchVirExpression>> dependencies,
            Dictionary<BranchVirExpression, double> executionTimes)
        {
            var groups = new List<List<BranchVirExpression>>();
            var remainingInputs = new HashSet<BranchVirExpression>(inputs);

            while (remainingInputs.Any())
            {
                var group = new List<BranchVirExpression>();
                var candidatesForGroup = remainingInputs.Where(input => 
                    !dependencies[input].Intersect(remainingInputs).Except(new[] { input }).Any()).ToList();

                while (group.Count < _maxParallelism && candidatesForGroup.Any())
                {
                    var bestCandidate = candidatesForGroup.OrderByDescending(c => executionTimes[c]).First();
                    group.Add(bestCandidate);
                    remainingInputs.Remove(bestCandidate);
                    candidatesForGroup.Remove(bestCandidate);
                }

                groups.Add(group);
            }

            return groups;
        }

        private List<List<BranchVirExpression>> GroupMatrixMultiplication(
            List<BranchVirExpression> inputs,
            Dictionary<BranchVirExpression, HashSet<BranchVirExpression>> dependencies,
            Dictionary<BranchVirExpression, double> executionTimes)
        {
            // For matrix multiplication, we can parallelize the computation of different blocks
            // of the result matrix. This is a simplified version assuming 2x2 block division.
            if (inputs.Count != 2)
            {
                Logger.Warn("Matrix multiplication expects exactly 2 inputs");
                return new List<List<BranchVirExpression>> { inputs };
            }

            var groups = new List<List<BranchVirExpression>>();
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    groups.Add(new List<BranchVirExpression> 
                    { 
                        new BranchVirOperation 
                        { 
                            OperationName = "MatMulBlock", 
                            Inputs = new List<BranchVirExpression> { inputs[0], inputs[1] },
                            Attributes = new Dictionary<string, object> 
                            { 
                                { "BlockRow", i }, 
                                { "BlockCol", j } 
                            }
                        } 
                    });
                }
            }

            return groups;
        }

        private List<List<BranchVirExpression>> GroupGenericOperation(
            List<BranchVirExpression> inputs,
            Dictionary<BranchVirExpression, HashSet<BranchVirExpression>> dependencies,
            Dictionary<BranchVirExpression, double> executionTimes)
        {
            // For generic operations, we group inputs that don't depend on each other
            var groups = new List<List<BranchVirExpression>>();
            var remainingInputs = new HashSet<BranchVirExpression>(inputs);

            while (remainingInputs.Any())
            {
                var group = new List<BranchVirExpression>();
                var candidatesForGroup = new HashSet<BranchVirExpression>(remainingInputs);

                while (group.Count < _maxParallelism && candidatesForGroup.Any())
                {
                    var bestCandidate = candidatesForGroup.OrderByDescending(c => executionTimes[c]).First();
                    group.Add(bestCandidate);
                    remainingInputs.Remove(bestCandidate);
                    candidatesForGroup.RemoveWhere(c => dependencies[c].Intersect(group).Any() || dependencies[bestCandidate].Contains(c));
                }

                groups.Add(group);
            }

            return groups;
        }
    }
}