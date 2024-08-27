using System;
using System.Collections.Generic;
using System.Linq;
using Vectorization.IntermediateRepresentation.BranchFocused;

namespace Vectorization.Optimization
{
    public class EnhancedParallelismAwareBranchVirOptimizer : BranchVirOptimizer
    {
        private class DependencyGraph
        {
            public Dictionary<BranchVirExpression, HashSet<BranchVirExpression>> Dependencies { get; } = new Dictionary<BranchVirExpression, HashSet<BranchVirExpression>>();

            public void AddDependency(BranchVirExpression dependent, BranchVirExpression dependency)
            {
                if (!Dependencies.ContainsKey(dependent))
                {
                    Dependencies[dependent] = new HashSet<BranchVirExpression>();
                }
                Dependencies[dependent].Add(dependency);
            }

            public bool HasDependency(BranchVirExpression dependent, BranchVirExpression dependency)
            {
                return Dependencies.ContainsKey(dependent) && Dependencies[dependent].Contains(dependency);
            }
        }

        protected override BranchVirExpression OptimizeBranch(BranchVirBranch branch)
        {
            var optimizedBranch = base.OptimizeBranch(branch);
            
            if (optimizedBranch is BranchVirBranch optimizedBranchVirBranch)
            {
                var dependencyGraph = BuildDependencyGraph(optimizedBranchVirBranch.Branches);
                var parallelGroups = IdentifyParallelGroups(optimizedBranchVirBranch.Branches, dependencyGraph);

                if (parallelGroups.Count > 1)
                {
                    return CreateParallelOperation(parallelGroups, optimizedBranchVirBranch.Combination);
                }
            }

            return optimizedBranch;
        }

        private DependencyGraph BuildDependencyGraph(List<BranchVirExpression> expressions)
        {
            var graph = new DependencyGraph();
            var variables = new Dictionary<string, BranchVirExpression>();

            foreach (var expr in expressions)
            {
                AnalyzeExpression(expr, graph, variables);
            }

            return graph;
        }

        private void AnalyzeExpression(BranchVirExpression expr, DependencyGraph graph, Dictionary<string, BranchVirExpression> variables)
        {
            switch (expr)
            {
                case BranchVirOperation op:
                    foreach (var input in op.Inputs)
                    {
                        graph.AddDependency(expr, input);
                        AnalyzeExpression(input, graph, variables);
                    }
                    break;
                case BranchVirVariable var:
                    if (variables.TryGetValue(var.Name, out var dependencyExpr))
                    {
                        graph.AddDependency(expr, dependencyExpr);
                    }
                    variables[var.Name] = expr;
                    break;
            }
        }

        private List<List<BranchVirExpression>> IdentifyParallelGroups(List<BranchVirExpression> expressions, DependencyGraph graph)
        {
            var groups = new List<List<BranchVirExpression>>();
            var remainingExpressions = new HashSet<BranchVirExpression>(expressions);

            while (remainingExpressions.Any())
            {
                var group = new List<BranchVirExpression>();
                var currentExpr = remainingExpressions.First();
                group.Add(currentExpr);
                remainingExpressions.Remove(currentExpr);

                foreach (var expr in remainingExpressions.ToList())
                {
                    if (!HasDependencyWithGroup(expr, group, graph))
                    {
                        group.Add(expr);
                        remainingExpressions.Remove(expr);
                    }
                }

                groups.Add(group);
            }

            return groups;
        }

        private bool HasDependencyWithGroup(BranchVirExpression expr, List<BranchVirExpression> group, DependencyGraph graph)
        {
            return group.Any(groupExpr => 
                graph.HasDependency(expr, groupExpr) || graph.HasDependency(groupExpr, expr));
        }

        private BranchVirOperation CreateParallelOperation(List<List<BranchVirExpression>> parallelGroups, BranchVirExpression combination)
        {
            var parallelOps = parallelGroups.Select(group => 
                new BranchVirOperation
                {
                    OperationName = "ParallelGroup",
                    Inputs = group
                }).ToList();

            return new BranchVirOperation
            {
                OperationName = "Parallel",
                Inputs = parallelOps,
                Attributes = new Dictionary<string, object>
                {
                    { "Combination", combination }
                }
            };
        }
    }
}