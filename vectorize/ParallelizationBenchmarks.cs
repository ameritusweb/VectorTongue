using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Vectorization.IntermediateRepresentation.BranchFocused;
using Vectorization.Optimization;
using Vectorization.Operations;

namespace Vectorization.Benchmarks
{
    public class ParallelizationBenchmarks
    {
        private CostModelIntegratedParallelismAwareBranchVirOptimizer _parallelOptimizer;
        private CostModelIntegratedParallelismAwareBranchVirOptimizer _serialOptimizer;
        private List<BranchVirExpression> _largeAdditionInputs;
        private List<BranchVirExpression> _matrixMultiplicationInputs;

        [GlobalSetup]
        public void Setup()
        {
            var costModel = new ParallelizationCostModel(); // Implement this based on your needs
            _parallelOptimizer = new CostModelIntegratedParallelismAwareBranchVirOptimizer(costModel, Environment.ProcessorCount);
            _serialOptimizer = new CostModelIntegratedParallelismAwareBranchVirOptimizer(costModel, 1); // Force serial execution

            _largeAdditionInputs = Enumerable.Range(0, 1000)
                .Select(i => (BranchVirExpression)new BranchVirVariable { Name = $"v{i}" })
                .ToList();

            _matrixMultiplicationInputs = new List<BranchVirExpression>
            {
                new BranchVirVariable { Name = "A" },
                new BranchVirVariable { Name = "B" }
            };
        }

        [Benchmark]
        public BranchVirFunction LargeAdditionParallel()
        {
            var operation = new BranchVirOperation
            {
                OperationName = "Add",
                Inputs = _largeAdditionInputs
            };
            return _parallelOptimizer.Optimize(new BranchVirFunction { Body = operation });
        }

        [Benchmark]
        public BranchVirFunction LargeAdditionSerial()
        {
            var operation = new BranchVirOperation
            {
                OperationName = "Add",
                Inputs = _largeAdditionInputs
            };
            return _serialOptimizer.Optimize(new BranchVirFunction { Body = operation });
        }

        [Benchmark]
        public BranchVirFunction MatrixMultiplicationParallel()
        {
            var operation = new BranchVirOperation
            {
                OperationName = "MatMul",
                Inputs = _matrixMultiplicationInputs
            };
            return _parallelOptimizer.Optimize(new BranchVirFunction { Body = operation });
        }

        [Benchmark]
        public BranchVirFunction MatrixMultiplicationSerial()
        {
            var operation = new BranchVirOperation
            {
                OperationName = "MatMul",
                Inputs = _matrixMultiplicationInputs
            };
            return _serialOptimizer.Optimize(new BranchVirFunction { Body = operation });
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ParallelizationBenchmarks>();
        }
    }
}