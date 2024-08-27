using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Vectorization.IntermediateRepresentation.BranchFocused;
using Vectorization.Optimization;
using Vectorization.Operations;
using Moq;

namespace Vectorization.Tests
{
    [TestFixture]
    public class GroupingAndParallelizationTests
    {
        private AdvancedInputGroupingStrategy _groupingStrategy;
        private CostModelIntegratedParallelismAwareBranchVirOptimizer _optimizer;
        private Mock<ParallelizationCostModel> _mockCostModel;

        [SetUp]
        public void Setup()
        {
            _groupingStrategy = new AdvancedInputGroupingStrategy(4); // Max parallelism of 4
            _mockCostModel = new Mock<ParallelizationCostModel>();
            _optimizer = new CostModelIntegratedParallelismAwareBranchVirOptimizer(_mockCostModel.Object, 4);
        }

        [Test]
        public void TestGroupingStrategyForAssociativeOperation()
        {
            var inputs = new List<BranchVirExpression>
            {
                new BranchVirVariable { Name = "a" },
                new BranchVirVariable { Name = "b" },
                new BranchVirVariable { Name = "c" },
                new BranchVirVariable { Name = "d" },
                new BranchVirVariable { Name = "e" }
            };

            var groups = _groupingStrategy.GroupInputsForParallelExecution(inputs, "Add");

            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(4, groups[0].Count);
            Assert.AreEqual(1, groups[1].Count);
        }

        [Test]
        public void TestGroupingStrategyForMatrixMultiplication()
        {
            var inputs = new List<BranchVirExpression>
            {
                new BranchVirVariable { Name = "A" },
                new BranchVirVariable { Name = "B" }
            };

            var groups = _groupingStrategy.GroupInputsForParallelExecution(inputs, "MatMul");

            Assert.AreEqual(4, groups.Count); // 2x2 block division
            Assert.IsTrue(groups.All(g => g.Count == 1 && g[0] is BranchVirOperation));
        }

        [Test]
        public void TestParallelOperationCreationForAddition()
        {
            var operation = new BranchVirOperation
            {
                OperationName = "Add",
                Inputs = new List<BranchVirExpression>
                {
                    new BranchVirVariable { Name = "a" },
                    new BranchVirVariable { Name = "b" },
                    new BranchVirVariable { Name = "c" },
                    new BranchVirVariable { Name = "d" }
                }
            };

            _mockCostModel.Setup(m => m.IsParallelizationBeneficial(It.IsAny<BranchVirOperation>())).Returns(true);

            var result = _optimizer.Optimize(new BranchVirFunction { Body = operation });

            Assert.IsInstanceOf<BranchVirOperation>(result.Body);
            var resultOp = (BranchVirOperation)result.Body;
            Assert.AreEqual("SequentialComposition", resultOp.OperationName);
            Assert.AreEqual(2, resultOp.Inputs.Count);
            Assert.AreEqual("Parallel", ((BranchVirOperation)resultOp.Inputs[0]).OperationName);
            Assert.AreEqual("Add", ((BranchVirOperation)resultOp.Inputs[1]).OperationName);
        }

        [Test]
        public void TestParallelOperationCreationForMatrixMultiplication()
        {
            var operation = new BranchVirOperation
            {
                OperationName = "MatMul",
                Inputs = new List<BranchVirExpression>
                {
                    new BranchVirVariable { Name = "A" },
                    new BranchVirVariable { Name = "B" }
                }
            };

            _mockCostModel.Setup(m => m.IsParallelizationBeneficial(It.IsAny<BranchVirOperation>())).Returns(true);

            var result = _optimizer.Optimize(new BranchVirFunction { Body = operation });

            Assert.IsInstanceOf<BranchVirOperation>(result.Body);
            var resultOp = (BranchVirOperation)result.Body;
            Assert.AreEqual("SequentialComposition", resultOp.OperationName);
            Assert.AreEqual(2, resultOp.Inputs.Count);
            Assert.AreEqual("Parallel", ((BranchVirOperation)resultOp.Inputs[0]).OperationName);
            Assert.AreEqual("MatMulCombine", ((BranchVirOperation)resultOp.Inputs[1]).OperationName);
            Assert.AreEqual(4, ((BranchVirOperation)resultOp.Inputs[0]).Inputs.Count);
        }
    }
}