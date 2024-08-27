using System.Collections.Generic;
using System.Linq;
using Vectorization.IntermediateRepresentation.BranchFocused;

public partial class ParallelizationOptimizationTests
{
    private VirToBranchVirConverter _virToBranchVirConverter;
    private CostModelIntegratedParallelismAwareBranchVirOptimizer _optimizer;
    private ParallelismAwareBranchVirToPradOpConverter _pradOpConverter;

    [SetUp]
    public void Setup()
    {
        _virToBranchVirConverter = new VirToBranchVirConverter();
        _optimizer = new CostModelIntegratedParallelismAwareBranchVirOptimizer();
        _pradOpConverter = new ParallelismAwareBranchVirToPradOpConverter();
    }

    [Test]
    public void TestSimpleParallelization()
    {
        var virFunction = CreateSimpleParallelizableFunction();
        var branchVirFunction = _virToBranchVirConverter.Convert(virFunction);
        var optimizedFunction = _optimizer.Optimize(branchVirFunction);

        Assert.IsInstanceOf<BranchVirOperation>(optimizedFunction.Body);
        var bodyOp = (BranchVirOperation)optimizedFunction.Body;
        Assert.AreEqual("Parallel", bodyOp.OperationName);
    }

    [Test]
    public void TestComplexParallelization()
    {
        var virFunction = CreateComplexParallelizableFunction();
        var branchVirFunction = _virToBranchVirConverter.Convert(virFunction);
        var optimizedFunction = _optimizer.Optimize(branchVirFunction);

        Assert.IsInstanceOf<BranchVirOperation>(optimizedFunction.Body);
        var bodyOp = (BranchVirOperation)optimizedFunction.Body;
        Assert.AreEqual("Parallel", bodyOp.OperationName);
        Assert.AreEqual(2, bodyOp.Inputs.Count); // Expecting two parallel groups
    }

    [Test]
    public void TestNonParallelizableFunction()
    {
        var virFunction = CreateNonParallelizableFunction();
        var branchVirFunction = _virToBranchVirConverter.Convert(virFunction);
        var optimizedFunction = _optimizer.Optimize(branchVirFunction);

        Assert.IsNotInstanceOf<BranchVirOperation>(optimizedFunction.Body);
        // or if it's wrapped in a "Serial" operation:
        // Assert.AreEqual("Serial", ((BranchVirOperation)optimizedFunction.Body).OperationName);
    }

    [Test]
    public void TestCostModelDecision()
    {
        var virFunction = CreateFunctionWithMarginalParallelizationBenefit();
        var branchVirFunction = _virToBranchVirConverter.Convert(virFunction);
        var optimizedFunction = _optimizer.Optimize(branchVirFunction);

        // The actual assertion here will depend on your cost model thresholds
        // You might need to adjust this based on your specific implementation
        Assert.IsInstanceOf<BranchVirOperation>(optimizedFunction.Body);
        var bodyOp = (BranchVirOperation)optimizedFunction.Body;
        Assert.AreEqual("Serial", bodyOp.OperationName);
    }

    [Test]
    public void TestPradOpCodeGeneration()
    {
        var virFunction = CreateSimpleParallelizableFunction();
        var branchVirFunction = _virToBranchVirConverter.Convert(virFunction);
        var optimizedFunction = _optimizer.Optimize(branchVirFunction);
        var pradOpCode = _pradOpConverter.Convert(optimizedFunction);

        Assert.IsTrue(pradOpCode.Contains("DoParallel"));
        Assert.IsTrue(pradOpCode.Contains("ThenParallel"));
    }

    [Test]
    public void TestEndToEndOptimization()
    {
        var virFunction = CreateComplexMixedFunction();
        var branchVirFunction = _virToBranchVirConverter.Convert(virFunction);
        var optimizedFunction = _optimizer.Optimize(branchVirFunction);
        var pradOpCode = _pradOpConverter.Convert(optimizedFunction);

        Assert.IsTrue(pradOpCode.Contains("DoParallel"));
        Assert.IsTrue(pradOpCode.Contains("ThenParallel"));
        Assert.IsTrue(pradOpCode.Contains("Add"));
        Assert.IsTrue(pradOpCode.Contains("Multiply"));
    }

    // Helper methods to create test VIR functions
    private VirFunction CreateSimpleParallelizableFunction()
    {
        // Create a VIR function that represents: f(x, y) = sin(x) + cos(y)
        return new VirFunction
        {
            Name = "SimpleParallelizable",
            Parameters = new List<VirParameter>
            {
                new VirParameter { Name = "x", Type = new VirType { TypeName = "double", IsScalar = false } },
                new VirParameter { Name = "y", Type = new VirType { TypeName = "double", IsScalar = false } }
            },
            Body = new VirBinaryOperation
            {
                Operator = VirOperator.Add,
                Left = new VirUnaryOperation
                {
                    Operator = VirUnaryOperator.Sin,
                    Operand = new VirVariable { Name = "x" }
                },
                Right = new VirUnaryOperation
                {
                    Operator = VirUnaryOperator.Cos,
                    Operand = new VirVariable { Name = "y" }
                }
            }
        };
    }

    private VirFunction CreateComplexParallelizableFunction()
    {
        // f(x, y, z) = (sin(x) + cos(y)) * (exp(z) - log(x^2 + y^2))
        return new VirFunction
        {
            Name = "ComplexParallelizable",
            Parameters = new List<VirParameter>
            {
                new VirParameter { Name = "x", Type = new VirType { TypeName = "double", IsScalar = false } },
                new VirParameter { Name = "y", Type = new VirType { TypeName = "double", IsScalar = false } },
                new VirParameter { Name = "z", Type = new VirType { TypeName = "double", IsScalar = false } }
            },
            Body = new VirBinaryOperation
            {
                Operator = VirOperator.Multiply,
                Left = new VirBinaryOperation
                {
                    Operator = VirOperator.Add,
                    Left = new VirUnaryOperation
                    {
                        Operator = VirUnaryOperator.Sin,
                        Operand = new VirVariable { Name = "x" }
                    },
                    Right = new VirUnaryOperation
                    {
                        Operator = VirUnaryOperator.Cos,
                        Operand = new VirVariable { Name = "y" }
                    }
                },
                Right = new VirBinaryOperation
                {
                    Operator = VirOperator.Subtract,
                    Left = new VirUnaryOperation
                    {
                        Operator = VirUnaryOperator.Exp,
                        Operand = new VirVariable { Name = "z" }
                    },
                    Right = new VirUnaryOperation
                    {
                        Operator = VirUnaryOperator.Log,
                        Operand = new VirBinaryOperation
                        {
                            Operator = VirOperator.Add,
                            Left = new VirBinaryOperation
                            {
                                Operator = VirOperator.Power,
                                Left = new VirVariable { Name = "x" },
                                Right = new VirConstant { Value = 2 }
                            },
                            Right = new VirBinaryOperation
                            {
                                Operator = VirOperator.Power,
                                Left = new VirVariable { Name = "y" },
                                Right = new VirConstant { Value = 2 }
                            }
                        }
                    }
                }
            }
        };
    }

    private VirFunction CreateNonParallelizableFunction()
    {
        // f(x) = x + (x + 1) + (x + 2) + ... + (x + 9)
        // This function has dependencies between each step, making it hard to parallelize
        var body = new VirVariable { Name = "x" };
        for (int i = 1; i <= 9; i++)
        {
            body = new VirBinaryOperation
            {
                Operator = VirOperator.Add,
                Left = body,
                Right = new VirBinaryOperation
                {
                    Operator = VirOperator.Add,
                    Left = new VirVariable { Name = "x" },
                    Right = new VirConstant { Value = i }
                }
            };
        }

        return new VirFunction
        {
            Name = "NonParallelizable",
            Parameters = new List<VirParameter>
            {
                new VirParameter { Name = "x", Type = new VirType { TypeName = "double", IsScalar = false } }
            },
            Body = body
        };
    }

    private VirFunction CreateFunctionWithMarginalParallelizationBenefit()
    {
        // f(x, y) = x + y + (x * y)
        // This function has some parallelizable parts, but the benefit might be marginal due to its simplicity
        return new VirFunction
        {
            Name = "MarginalBenefit",
            Parameters = new List<VirParameter>
            {
                new VirParameter { Name = "x", Type = new VirType { TypeName = "double", IsScalar = false } },
                new VirParameter { Name = "y", Type = new VirType { TypeName = "double", IsScalar = false } }
            },
            Body = new VirBinaryOperation
            {
                Operator = VirOperator.Add,
                Left = new VirBinaryOperation
                {
                    Operator = VirOperator.Add,
                    Left = new VirVariable { Name = "x" },
                    Right = new VirVariable { Name = "y" }
                },
                Right = new VirBinaryOperation
                {
                    Operator = VirOperator.Multiply,
                    Left = new VirVariable { Name = "x" },
                    Right = new VirVariable { Name = "y" }
                }
            }
        };
    }

    private VirFunction CreateComplexMixedFunction()
    {
        // f(x, y, z) = (sin(x) + cos(y)) * exp(z) + (x + y + z)^3
        return new VirFunction
        {
            Name = "ComplexMixed",
            Parameters = new List<VirParameter>
            {
                new VirParameter { Name = "x", Type = new VirType { TypeName = "double", IsScalar = false } },
                new VirParameter { Name = "y", Type = new VirType { TypeName = "double", IsScalar = false } },
                new VirParameter { Name = "z", Type = new VirType { TypeName = "double", IsScalar = false } }
            },
            Body = new VirBinaryOperation
            {
                Operator = VirOperator.Add,
                Left = new VirBinaryOperation
                {
                    Operator = VirOperator.Multiply,
                    Left = new VirBinaryOperation
                    {
                        Operator = VirOperator.Add,
                        Left = new VirUnaryOperation
                        {
                            Operator = VirUnaryOperator.Sin,
                            Operand = new VirVariable { Name = "x" }
                        },
                        Right = new VirUnaryOperation
                        {
                            Operator = VirUnaryOperator.Cos,
                            Operand = new VirVariable { Name = "y" }
                        }
                    },
                    Right = new VirUnaryOperation
                    {
                        Operator = VirUnaryOperator.Exp,
                        Operand = new VirVariable { Name = "z" }
                    }
                },
                Right = new VirBinaryOperation
                {
                    Operator = VirOperator.Power,
                    Left = new VirBinaryOperation
                    {
                        Operator = VirOperator.Add,
                        Left = new VirBinaryOperation
                        {
                            Operator = VirOperator.Add,
                            Left = new VirVariable { Name = "x" },
                            Right = new VirVariable { Name = "y" }
                        },
                        Right = new VirVariable { Name = "z" }
                    },
                    Right = new VirConstant { Value = 3 }
                }
            }
        };
    }

    private Gen<VirFunction> ArbVirFunction()
    {
        return Gen.Fresh(() =>
        {
            var paramCount = Gen.Choose(1, 5).Sample(0, 1)[0];
            var parameters = Enumerable.Range(0, paramCount)
                .Select(i => new VirParameter { Name = $"x{i}", Type = new VirType { TypeName = "double", IsScalar = false } })
                .ToList();

            return new VirFunction
            {
                Name = $"RandomFunction_{Guid.NewGuid()}",
                Parameters = parameters,
                Body = ArbVirExpression(parameters.Select(p => p.Name).ToList(), 5).Sample(0, 1)[0]
            };
        });
    }

    private Gen<VirExpression> ArbVirExpression(List<string> variables, int depth)
    {
        if (depth <= 0 || variables.Count == 0)
        {
            return Gen.OneOf(
                Gen.Constant<VirExpression>(new VirConstant { Value = Gen.Choose(-100.0, 100.0).Sample(0, 1)[0] }),
                Gen.Elements(variables).Select(v => new VirVariable { Name = v } as VirExpression)
            );
        }

        return Gen.Frequency(
            Tuple.Create(1, Gen.Constant<VirExpression>(new VirConstant { Value = Gen.Choose(-100.0, 100.0).Sample(0, 1)[0] })),
            Tuple.Create(1, Gen.Elements(variables).Select(v => new VirVariable { Name = v } as VirExpression)),
            Tuple.Create(3, Gen.OneOf(
                Gen.Map2((VirExpression left, VirExpression right) => new VirBinaryOperation
                {
                    Operator = Gen.Elements(Enum.GetValues(typeof(VirOperator)).Cast<VirOperator>()).Sample(0, 1)[0],
                    Left = left,
                    Right = right
                } as VirExpression,
                ArbVirExpression(variables, depth - 1),
                ArbVirExpression(variables, depth - 1)),
                Gen.Map((VirExpression operand) => new VirUnaryOperation
                {
                    Operator = Gen.Elements(Enum.GetValues(typeof(VirUnaryOperator)).Cast<VirUnaryOperator>()).Sample(0, 1)[0],
                    Operand = operand
                } as VirExpression,
                ArbVirExpression(variables, depth - 1))
            ))
        );
    }

    private double[] EvaluateVirFunction(VirFunction function)
    {
        var inputValues = GenerateRandomInputs(function.Parameters);
        return EvaluateVirExpression(function.Body, inputValues);
    }

    private double[] EvaluateBranchVirFunction(BranchVirFunction function)
    {
        var inputValues = GenerateRandomInputs(function.Parameters);
        return EvaluateBranchVirExpression(function.Body, inputValues);
    }

    private Dictionary<string, double[]> GenerateRandomInputs(List<VirParameter> parameters)
    {
        var random = new Random();
        return parameters.ToDictionary(
            p => p.Name,
            p => Enumerable.Range(0, 100).Select(_ => random.NextDouble() * 200 - 100).ToArray()
        );
    }

    private double[] EvaluateVirExpression(VirExpression expression, Dictionary<string, double[]> inputs)
    {
        switch (expression)
        {
            case VirConstant constant:
                return Enumerable.Repeat((double)constant.Value, 100).ToArray();
            case VirVariable variable:
                return inputs[variable.Name];
            case VirBinaryOperation binaryOp:
                var left = EvaluateVirExpression(binaryOp.Left, inputs);
                var right = EvaluateVirExpression(binaryOp.Right, inputs);
                return ApplyBinaryOperation(binaryOp.Operator, left, right);
            case VirUnaryOperation unaryOp:
                var operand = EvaluateVirExpression(unaryOp.Operand, inputs);
                return ApplyUnaryOperation(unaryOp.Operator, operand);
            default:
                throw new NotImplementedException($"Evaluation for {expression.GetType()} is not implemented.");
        }
    }

    private double[] EvaluateBranchVirExpression(BranchVirExpression expression, Dictionary<string, double[]> inputs)
    {
        switch (expression)
        {
            case BranchVirConstant constant:
                return Enumerable.Repeat((double)constant.Value, 100).ToArray();
            case BranchVirVariable variable:
                return inputs[variable.Name];
            case BranchVirOperation operation:
                var evaluatedInputs = operation.Inputs.Select(input => EvaluateBranchVirExpression(input, inputs)).ToList();
                return ApplyOperation(operation.OperationName, evaluatedInputs);
            default:
                throw new NotImplementedException($"Evaluation for {expression.GetType()} is not implemented.");
        }
    }

    private double[] ApplyBinaryOperation(VirOperator op, double[] left, double[] right)
    {
        return op switch
        {
            VirOperator.Add => left.Zip(right, (l, r) => l + r).ToArray(),
            VirOperator.Subtract => left.Zip(right, (l, r) => l - r).ToArray(),
            VirOperator.Multiply => left.Zip(right, (l, r) => l * r).ToArray(),
            VirOperator.Divide => left.Zip(right, (l, r) => r != 0 ? l / r : double.NaN).ToArray(),
            VirOperator.Power => left.Zip(right, Math.Pow).ToArray(),
            _ => throw new NotImplementedException($"Binary operation {op} is not implemented.")
        };
    }

    private double[] ApplyUnaryOperation(VirUnaryOperator op, double[] operand)
    {
        return op switch
        {
            VirUnaryOperator.Negate => operand.Select(x => -x).ToArray(),
            VirUnaryOperator.Abs => operand.Select(Math.Abs).ToArray(),
            VirUnaryOperator.Sqrt => operand.Select(Math.Sqrt).ToArray(),
            VirUnaryOperator.Sin => operand.Select(Math.Sin).ToArray(),
            VirUnaryOperator.Cos => operand.Select(Math.Cos).ToArray(),
            VirUnaryOperator.Tan => operand.Select(Math.Tan).ToArray(),
            VirUnaryOperator.Exp => operand.Select(Math.Exp).ToArray(),
            VirUnaryOperator.Log => operand.Select(Math.Log).ToArray(),
            _ => throw new NotImplementedException($"Unary operation {op} is not implemented.")
        };
    }

    private int CalculateComplexity(VirFunction function)
    {
        return CalculateVirExpressionComplexity(function.Body);
    }

    private int CalculateComplexity(BranchVirFunction function)
    {
        return CalculateBranchVirExpressionComplexity(function.Body);
    }

    private int CalculateVirExpressionComplexity(VirExpression expression)
    {
        return expression switch
        {
            VirConstant _ => 1,
            VirVariable _ => 1,
            VirBinaryOperation binaryOp => 1 + CalculateVirExpressionComplexity(binaryOp.Left) + CalculateVirExpressionComplexity(binaryOp.Right),
            VirUnaryOperation unaryOp => 1 + CalculateVirExpressionComplexity(unaryOp.Operand),
            _ => throw new NotImplementedException($"Complexity calculation for {expression.GetType()} is not implemented.")
        };
    }

    private int CalculateBranchVirExpressionComplexity(BranchVirExpression expression)
    {
        return expression switch
        {
            BranchVirConstant _ => 1,
            BranchVirVariable _ => 1,
            BranchVirOperation operation => 1 + operation.Inputs.Sum(CalculateBranchVirExpressionComplexity),
            _ => throw new NotImplementedException($"Complexity calculation for {expression.GetType()} is not implemented.")
        };
    }

    [Property(MaxTest = 100)]
    public Property OptimizationPreservesSemantics()
    {
        return Prop.ForAll(ArbVirFunction(), virFunction =>
        {
            var branchVirFunction = _virToBranchVirConverter.Convert(virFunction);
            var optimizedFunction = _optimizer.Optimize(branchVirFunction);
            var originalResult = EvaluateVirFunction(virFunction);
            var optimizedResult = EvaluateBranchVirFunction(optimizedFunction);

            return originalResult.Zip(optimizedResult, (o, p) => Math.Abs(o - p) < 1e-6).All(x => x);
        });
    }

    [Property(MaxTest = 100)]
    public Property OptimizationNeverIncreasesComplexity()
    {
        return Prop.ForAll(ArbVirFunction(), virFunction =>
        {
            var branchVirFunction = _virToBranchVirConverter.Convert(virFunction);
            var optimizedFunction = _optimizer.Optimize(branchVirFunction);
            var originalComplexity = CalculateComplexity(virFunction);
            var optimizedComplexity = CalculateComplexity(optimizedFunction);

            return optimizedComplexity <= originalComplexity;
        });
    }

    [Property(MaxTest = 100)]
    public Property OptimizationProducesValidBranchVirFunction()
    {
        return Prop.ForAll(ArbVirFunction(), virFunction =>
        {
            var branchVirFunction = _virToBranchVirConverter.Convert(virFunction);
            var optimizedFunction = _optimizer.Optimize(branchVirFunction);

            return IsValidBranchVirFunction(optimizedFunction);
        });
    }

    private double[] ApplyOperation(string operationName, List<double[]> inputs)
    {
        switch (operationName)
        {
            case BranchVirOperations.Add:
                return inputs.Aggregate((a, b) => a.Zip(b, (x, y) => x + y).ToArray());
            case BranchVirOperations.Subtract:
                return inputs.Aggregate((a, b) => a.Zip(b, (x, y) => x - y).ToArray());
            case BranchVirOperations.Multiply:
                return inputs.Aggregate((a, b) => a.Zip(b, (x, y) => x * y).ToArray());
            case BranchVirOperations.Divide:
                return inputs.Aggregate((a, b) => a.Zip(b, (x, y) => y != 0 ? x / y : double.NaN).ToArray());
            case BranchVirOperations.Power:
                return inputs.Aggregate((a, b) => a.Zip(b, Math.Pow).ToArray());
            case BranchVirOperations.Sin:
                return inputs[0].Select(Math.Sin).ToArray();
            case BranchVirOperations.Cos:
                return inputs[0].Select(Math.Cos).ToArray();
            case BranchVirOperations.Tan:
                return inputs[0].Select(Math.Tan).ToArray();
            case BranchVirOperations.Exp:
                return inputs[0].Select(Math.Exp).ToArray();
            case BranchVirOperations.Log:
                return inputs[0].Select(Math.Log).ToArray();
            case BranchVirOperations.Sqrt:
                return inputs[0].Select(Math.Sqrt).ToArray();
            case BranchVirOperations.Abs:
                return inputs[0].Select(Math.Abs).ToArray();
            case BranchVirOperations.Negate:
                return inputs[0].Select(x => -x).ToArray();
            case BranchVirOperations.MatMul:
                // This is a simplified matrix multiplication for 1D arrays
                // You might need to implement a more complex version for actual matrices
                return inputs[0].Select(x => inputs[1].Sum(y => x * y)).ToArray();
            case BranchVirOperations.Parallel:
            case BranchVirOperations.MergedBranch:
                // For parallel operations, we just concatenate the results
                // This is a simplification; in reality, you'd need to handle these specially
                return inputs.SelectMany(x => x).ToArray();
            default:
                throw new NotImplementedException($"Operation {operationName} is not implemented.");
        }
    }

    private bool IsValidBranchVirFunction(BranchVirFunction function)
    {
        // Check if the function has a name and parameters
        if (string.IsNullOrEmpty(function.Name) || function.Parameters == null || function.Parameters.Count == 0)
        {
            return false;
        }

        // Check if all parameters have valid names and types
        if (function.Parameters.Any(p => string.IsNullOrEmpty(p.Name) || p.Type == null || string.IsNullOrEmpty(p.Type.TypeName)))
        {
            return false;
        }

        // Check if the function body is valid
        return IsValidBranchVirExpression(function.Body, new HashSet<string>(function.Parameters.Select(p => p.Name)));
    }

    private bool IsValidBranchVirExpression(BranchVirExpression expression, HashSet<string> validVariables)
    {
        switch (expression)
        {
            case BranchVirConstant _:
                return true;
            case BranchVirVariable variable:
                return validVariables.Contains(variable.Name);
            case BranchVirOperation operation:
                if (!BranchVirOperations.IsValidOperation(operation.OperationName))
                {
                    return false;
                }
                return operation.Inputs.All(input => IsValidBranchVirExpression(input, validVariables));
            default:
                return false;
        }
    }
}