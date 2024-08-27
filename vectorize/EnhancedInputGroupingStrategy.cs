using System;
using System.Collections.Generic;
using System.Linq;
using Vectorization.IntermediateRepresentation.BranchFocused;
using NLog;

namespace Vectorization.Optimization
{
    public class EnhancedInputGroupingStrategy
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly int _maxParallelism;

        public EnhancedInputGroupingStrategy(int maxParallelism)
        {
            _maxParallelism = maxParallelism;
        }

        public List<List<BranchVirExpression>> GroupInputsForParallelExecution(List<BranchVirExpression> inputs, string operationName)
        {
            Logger.Debug($"Grouping inputs for parallel execution of {operationName}");

            var dependencies = AnalyzeDependencies(inputs);
            var executionTimes = EstimateExecutionTimes(inputs);
            var sortedInputs = TopologicalSort(inputs, dependencies);

            return operationName switch
            {
                "Add" or "Multiply" => GroupAssociativeOperation(sortedInputs, dependencies, executionTimes),
                "MatMul" => GroupMatrixMultiplication(sortedInputs, dependencies, executionTimes),
                _ => GroupGenericOperation(sortedInputs, dependencies, executionTimes)
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
            return expression switch
            {
                BranchVirOperation operation => operation.Inputs.Sum(EstimateExecutionTime) + 1,
                BranchVirVariable => 0.1,
                BranchVirConstant => 0.05,
                _ => 1
            };
        }

        private List<BranchVirExpression> TopologicalSort(List<BranchVirExpression> inputs, Dictionary<BranchVirExpression, HashSet<BranchVirExpression>> dependencies)
        {
            var sorted = new List<BranchVirExpression>();
            var visited = new HashSet<BranchVirExpression>();

            foreach (var input in inputs)
            {
                if (!visited.Contains(input))
                {
                    TopologicalSortUtil(input, visited, sorted, dependencies);
                }
            }

            sorted.Reverse();
            return sorted;
        }

        private void TopologicalSortUtil(BranchVirExpression expression, HashSet<BranchVirExpression> visited, List<BranchVirExpression> sorted, Dictionary<BranchVirExpression, HashSet<BranchVirExpression>> dependencies)
        {
            visited.Add(expression);

            if (dependencies.ContainsKey(expression))
            {
                foreach (var dep in dependencies[expression])
                {
                    if (!visited.Contains(dep))
                    {
                        TopologicalSortUtil(dep, visited, sorted, dependencies);
                    }
                }
            }

            sorted.Add(expression);
        }

        private List<List<BranchVirExpression>> GroupAssociativeOperation(List<BranchVirExpression> sortedInputs, Dictionary<BranchVirExpression, HashSet<BranchVirExpression>> dependencies, Dictionary<BranchVirExpression, double> executionTimes)
        {
            var groups = new List<List<BranchVirExpression>>();
            var currentGroup = new List<BranchVirExpression>();
            var processedInputs = new HashSet<BranchVirExpression>();

            foreach (var input in sortedInputs)
            {
                if (processedInputs.Contains(input)) continue;

                if (currentGroup.Count >= _maxParallelism || (currentGroup.Any() && dependencies[input].Intersect(currentGroup).Any()))
                {
                    groups.Add(currentGroup);
                    currentGroup = new List<BranchVirExpression>();
                }

                currentGroup.Add(input);
                processedInputs.Add(input);
            }

            if (currentGroup.Any())
            {
                groups.Add(currentGroup);
            }

            return groups;
        }

        private List<List<BranchVirExpression>> GroupMatrixMultiplication(List<BranchVirExpression> sortedInputs, Dictionary<BranchVirExpression, HashSet<BranchVirExpression>> dependencies, Dictionary<BranchVirExpression, double> executionTimes)
        {
            // For simplicity, we'll stick with the 2x2 block division
            // In a real-world scenario, you might want to determine the optimal block size based on matrix dimensions and system capabilities
            if (sortedInputs.Count != 2)
            {
                Logger.Warn("Matrix multiplication expects exactly 2 inputs");
                return new List<List<BranchVirExpression>> { sortedInputs };
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
                            Inputs = new List<BranchVirExpression> { sortedInputs[0], sortedInputs[1] },
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

        private List<List<BranchVirExpression>> GroupGenericOperation(List<BranchVirExpression> sortedInputs, Dictionary<BranchVirExpression, HashSet<BranchVirExpression>> dependencies, Dictionary<BranchVirExpression, double> executionTimes)
        {
            var groups = new List<List<BranchVirExpression>>();
            var currentGroup = new List<BranchVirExpression>();
            var processedInputs = new HashSet<BranchVirExpression>();

            foreach (var input in sortedInputs)
            {
                if (processedInputs.Contains(input)) continue;

                if (currentGroup.Count >= _maxParallelism || (currentGroup.Any() && dependencies[input].Intersect(currentGroup).Any()))
                {
                    groups.Add(currentGroup);
                    currentGroup = new List<BranchVirExpression>();
                }

                currentGroup.Add(input);
                processedInputs.Add(input);

                // Add independent siblings to the same group
                var siblings = sortedInputs.Where(sibling => 
                    !processedInputs.Contains(sibling) && 
                    !dependencies[sibling].Intersect(currentGroup).Any() && 
                    !dependencies[input].Contains(sibling) &&
                    !dependencies[sibling].Contains(input)).ToList();

                foreach (var sibling in siblings.Take(_maxParallelism - currentGroup.Count))
                {
                    currentGroup.Add(sibling);
                    processedInputs.Add(sibling);
                }
            }

            if (currentGroup.Any())
            {
                groups.Add(currentGroup);
            }

            return groups;
        }
    }
}