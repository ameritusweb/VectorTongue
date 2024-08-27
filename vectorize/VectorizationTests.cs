
using NUnit.Framework;
using Vectorization.IntermediateRepresentation;
using Vectorization.Converters;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Vectorization.Tests
{
    [TestFixture]
    public class VectorizationTests
    {
        private RoslynToVirConverter _roslynToVirConverter;
        private VirToMklnetConverter _virToMklnetConverter;

        [SetUp]
        public void Setup()
        {
            _roslynToVirConverter = new RoslynToVirConverter();
            _virToMklnetConverter = new VirToMklnetConverter();
        }

        [Test]
        public void TestSumReduction()
        {
            var code = @"
public double SumSquares(int n)
{
    double sum = 0;
    for (int i = 0; i < n; i++)
    {
        sum += i * i;
    }
    return sum;
}";

            var expected = @"public static Tensor SumSquares(Tensor n)
{
    Tensor i = MKLNET.Arange(0, n, 1);
    return MKLNET.Sum(MKLNET.Mul(i, i));
}";

            AssertReductionVectorization(code, expected);
        }

        [Test]
        public void TestProductReduction()
        {
            var code = @"
public double Factorial(int n)
{
    double product = 1;
    for (int i = 1; i <= n; i++)
    {
        product *= i;
    }
    return product;
}";

            var expected = @"public static Tensor Factorial(Tensor n)
{
    Tensor i = MKLNET.Arange(1, MKLNET.Add(n, 1), 1);
    return MKLNET.Prod(i);
}";

            AssertReductionVectorization(code, expected);
        }

        private void AssertReductionVectorization(string code, string expected)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();
            var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

            var virFunction = _roslynToVirConverter.ConvertMethod(method);
            var mklnetCode = _virToMklnetConverter.ConvertToMklnetCode(virFunction);

            Assert.AreEqual(expected.Trim(), mklnetCode.Trim());
        }

        [Test]
        public void TestLoopVectorization()
        {
            var code = @"
public double SumSquares(int n)
{
    double sum = 0;
    for (int i = 0; i < n; i++)
    {
        sum += i * i;
    }
    return sum;
}";

            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();
            var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

            var virFunction = _roslynToVirConverter.ConvertMethod(method);
            var mklnetCode = _virToMklnetConverter.ConvertToMklnetCode(virFunction);

            var expected = @"public static Tensor SumSquares(Tensor n)
{
    Tensor i = MKLNET.Arange(0, n, 1);
    Tensor result = MKLNET.ZerosLike(i);
    for (int _i = 0; _i < i.Shape[0]; _i++)
    {
        Tensor currenti = i[_i];
        result[_i] = MKLNET.Mul(currenti, currenti);
    }
    return MKLNET.Sum(result);
}";

            Assert.AreEqual(expected.Trim(), mklnetCode.Trim());
        }

        [Test]
        public void TestConditional()
        {
            var virFunction = new VirFunction
            {
                Name = "Conditional",
                Parameters = new List<VirParameter>
                {
                    new VirParameter { Name = "x", Type = new VirType { TypeName = "Tensor", IsScalar = false } }
                },
                Body = new VirConditional
                {
                    Condition = new VirBinaryOperation
                    {
                        Left = new VirVariable { Name = "x" },
                        Right = new VirConstant { Value = 0 },
                        Operator = VirOperator.GreaterThan
                    },
                    TrueBranch = new VirConstant { Value = 1 },
                    FalseBranch = new VirConstant { Value = -1 }
                }
            };

            var result = _converter.ConvertToMklnetCode(virFunction);

            var expected = @"public static Tensor Conditional(
    Tensor x
)
{
    return MKLNET.Where(MKLNET.Greater(x, MKLNET.Full(new[] { 1 }, 0)), MKLNET.Full(new[] { 1 }, 1), MKLNET.Full(new[] { 1 }, -1));
}";

            Assert.AreEqual(expected, result.Trim());
        }

        [Test]
        public void TestMathFunctions()
        {
            var virFunction = new VirFunction
            {
                Name = "MathFunctions",
                Parameters = new List<VirParameter>
                {
                    new VirParameter { Name = "x", Type = new VirType { TypeName = "Tensor", IsScalar = false } }
                },
                Body = new VirBinaryOperation
                {
                    Left = new VirMethodCall
                    {
                        MethodName = "Math.Sin",
                        Arguments = new List<VirExpression> { new VirVariable { Name = "x" } }
                    },
                    Right = new VirMethodCall
                    {
                        MethodName = "Math.Sqrt",
                        Arguments = new List<VirExpression>
                        {
                            new VirMethodCall
                            {
                                MethodName = "Math.Abs",
                                Arguments = new List<VirExpression> { new VirVariable { Name = "x" } }
                            }
                        }
                    },
                    Operator = VirOperator.Add
                }
            };

            var result = _converter.ConvertToMklnetCode(virFunction);

            var expected = @"public static Tensor MathFunctions(
    Tensor x
)
{
    return MKLNET.Add(MKLNET.Sin(x), MKLNET.Sqrt(MKLNET.Abs(x)));
}";

            Assert.AreEqual(expected, result.Trim());
        }

        [Test]
        public void TestBooleanConstant()
        {
            var virFunction = new VirFunction
            {
                Name = "BooleanConstant",
                Parameters = new List<VirParameter>
                {
                    new VirParameter { Name = "x", Type = new VirType { TypeName = "Tensor", IsScalar = false } }
                },
                Body = new VirConditional
                {
                    Condition = new VirBinaryOperation
                    {
                        Left = new VirVariable { Name = "x" },
                        Right = new VirConstant { Value = 0 },
                        Operator = VirOperator.Equal
                    },
                    TrueBranch = new VirConstant { Value = true },
                    FalseBranch = new VirConstant { Value = false }
                }
            };

            var result = _converter.ConvertToMklnetCode(virFunction);

            var expected = @"public static Tensor BooleanConstant(
    Tensor x
)
{
    return MKLNET.Where(MKLNET.Equal(x, MKLNET.Full(new[] { 1 }, 0)), MKLNET.OnesLike(MKLNET.Ones()), MKLNET.ZerosLike(MKLNET.Ones()));
}";

            Assert.AreEqual(expected, result.Trim());
        }

         [Test]
        public void TestVirFunctionToString()
        {
            var function = new VirFunction
            {
                Name = "TestFunction",
                Parameters = new List<VirParameter>
                {
                    new VirParameter { Name = "x", Type = new VirType { TypeName = "double", IsScalar = true } },
                    new VirParameter { Name = "y", Type = new VirType { TypeName = "double", IsScalar = true } }
                },
                Body = new VirBinaryOperation
                {
                    Left = new VirVariable { Name = "x" },
                    Right = new VirVariable { Name = "y" },
                    Operator = VirOperator.Add
                }
            };

            var expected = @"Function TestFunction:
  Parameters:
    double (Scalar) x
    double (Scalar) y
  Body:
    BinaryOperation:
      Operator: Add
      Left:
        Variable: x
      Right:
        Variable: y";

            Assert.AreEqual(expected, function.ToString().Trim());
        }

        [Test]
        public void TestRoslynToVirConverter_SimpleAddition()
        {
            var code = @"
public double Add(double x, double y)
{
    return x + y;
}";

            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();
            var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

            var converter = new RoslynToVirConverter();
            var virFunction = converter.ConvertMethod(method);

            Assert.AreEqual("Add", virFunction.Name);
            Assert.AreEqual(2, virFunction.Parameters.Count);
            Assert.IsInstanceOf<VirBinaryOperation>(virFunction.Body);

            var binaryOp = (VirBinaryOperation)virFunction.Body;
            Assert.AreEqual(VirOperator.Add, binaryOp.Operator);
            Assert.IsInstanceOf<VirVariable>(binaryOp.Left);
            Assert.IsInstanceOf<VirVariable>(binaryOp.Right);
            Assert.AreEqual("x", ((VirVariable)binaryOp.Left).Name);
            Assert.AreEqual("y", ((VirVariable)binaryOp.Right).Name);
        }

        [Test]
        public void TestVirToMklnetConverter_SimpleAddition()
        {
            var virFunction = new VirFunction
            {
                Name = "Add",
                Parameters = new List<VirParameter>
                {
                    new VirParameter { Name = "x", Type = new VirType { TypeName = "Tensor", IsScalar = false } },
                    new VirParameter { Name = "y", Type = new VirType { TypeName = "Tensor", IsScalar = false } }
                },
                Body = new VirBinaryOperation
                {
                    Left = new VirVariable { Name = "x" },
                    Right = new VirVariable { Name = "y" },
                    Operator = VirOperator.Add
                }
            };

            var converter = new VirToMklnetConverter();
            var result = converter.ConvertToMklnetCode(virFunction);

            var expected = @"public static Tensor Add(
    Tensor x,
    Tensor y
)
{
    return MKLNET.Add(x, y);
}";

            Assert.AreEqual(expected, result.Trim());
        }

        [Test]
        public void TestRoslynToVirConverter_ComplexExpression()
        {
            var code = @"
public double ComplexCalculation(double a, double b, double c)
{
    return (a + b) * c - Math.Pow(a, 2);
}";

            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();
            var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

            var converter = new RoslynToVirConverter();
            var virFunction = converter.ConvertMethod(method);

            Assert.AreEqual("ComplexCalculation", virFunction.Name);
            Assert.AreEqual(3, virFunction.Parameters.Count);
            Assert.IsInstanceOf<VirBinaryOperation>(virFunction.Body);

            var binaryOp = (VirBinaryOperation)virFunction.Body;
            Assert.AreEqual(VirOperator.Subtract, binaryOp.Operator);
            Assert.IsInstanceOf<VirBinaryOperation>(binaryOp.Left);
            Assert.IsInstanceOf<VirMethodCall>(binaryOp.Right);

            var multiplyOp = (VirBinaryOperation)binaryOp.Left;
            Assert.AreEqual(VirOperator.Multiply, multiplyOp.Operator);
            Assert.IsInstanceOf<VirBinaryOperation>(multiplyOp.Left);
            Assert.IsInstanceOf<VirVariable>(multiplyOp.Right);

            var addOp = (VirBinaryOperation)multiplyOp.Left;
            Assert.AreEqual(VirOperator.Add, addOp.Operator);
            Assert.IsInstanceOf<VirVariable>(addOp.Left);
            Assert.IsInstanceOf<VirVariable>(addOp.Right);

            var powCall = (VirMethodCall)binaryOp.Right;
            Assert.AreEqual("Math.Pow", powCall.MethodName);
            Assert.AreEqual(2, powCall.Arguments.Count);
        }

        [Test]
        public void TestVirToMklnetConverter_ComplexExpression()
        {
            var virFunction = new VirFunction
            {
                Name = "ComplexCalculation",
                Parameters = new List<VirParameter>
                {
                    new VirParameter { Name = "a", Type = new VirType { TypeName = "Tensor", IsScalar = false } },
                    new VirParameter { Name = "b", Type = new VirType { TypeName = "Tensor", IsScalar = false } },
                    new VirParameter { Name = "c", Type = new VirType { TypeName = "Tensor", IsScalar = false } }
                },
                Body = new VirBinaryOperation
                {
                    Left = new VirBinaryOperation
                    {
                        Left = new VirBinaryOperation
                        {
                            Left = new VirVariable { Name = "a" },
                            Right = new VirVariable { Name = "b" },
                            Operator = VirOperator.Add
                        },
                        Right = new VirVariable { Name = "c" },
                        Operator = VirOperator.Multiply
                    },
                    Right = new VirMethodCall
                    {
                        MethodName = "Math.Pow",
                        Arguments = new List<VirExpression>
                        {
                            new VirVariable { Name = "a" },
                            new VirConstant { Value = 2 }
                        }
                    },
                    Operator = VirOperator.Subtract
                }
            };

            var converter = new VirToMklnetConverter();
            var result = converter.ConvertToMklnetCode(virFunction);

            var expected = @"public static Tensor ComplexCalculation(
    Tensor a,
    Tensor b,
    Tensor c
)
{
    return MKLNET.Sub(MKLNET.Mul(MKLNET.Add(a, b), c), Math.Pow(a, MKLNET.Full(new[] { 1 }, 2)));
}";

            Assert.AreEqual(expected, result.Trim());
        }

        [Test]
        public void TestRoslynToVirConverter_IfStatement()
        {
            var code = @"
public double AbsoluteValue(double x)
{
    if (x < 0)
        return -x;
    else
        return x;
}";

            var virFunction = ConvertMethodToVir(code);

            Assert.AreEqual("AbsoluteValue", virFunction.Name);
            Assert.AreEqual(1, virFunction.Parameters.Count);
            Assert.IsInstanceOf<VirConditional>(virFunction.Body);

            var conditional = (VirConditional)virFunction.Body;
            Assert.IsInstanceOf<VirBinaryOperation>(conditional.Condition);
            Assert.IsInstanceOf<VirUnaryOperation>(conditional.TrueBranch);
            Assert.IsInstanceOf<VirVariable>(conditional.FalseBranch);
        }

        [Test]
        public void TestRoslynToVirConverter_UnaryOperation()
        {
            var code = @"
public double Negate(double x)
{
    return -x;
}";

            var virFunction = ConvertMethodToVir(code);

            Assert.AreEqual("Negate", virFunction.Name);
            Assert.IsInstanceOf<VirUnaryOperation>(virFunction.Body);

            var unaryOp = (VirUnaryOperation)virFunction.Body;
            Assert.AreEqual(VirUnaryOperator.Negate, unaryOp.Operator);
            Assert.IsInstanceOf<VirVariable>(unaryOp.Operand);
        }

        [Test]
        public void TestRoslynToVirConverter_MethodCall()
        {
            var code = @"
public double SquareRoot(double x)
{
    return Math.Sqrt(x);
}";

            var virFunction = ConvertMethodToVir(code);

            Assert.AreEqual("SquareRoot", virFunction.Name);
            Assert.IsInstanceOf<VirMethodCall>(virFunction.Body);

            var methodCall = (VirMethodCall)virFunction.Body;
            Assert.AreEqual("Math.Sqrt", methodCall.MethodName);
            Assert.AreEqual(1, methodCall.Arguments.Count);
            Assert.IsInstanceOf<VirVariable>(methodCall.Arguments[0]);
        }

        [Test]
        public void TestRoslynToVirConverter_ComplexExpression()
        {
            var code = @"
public double ComplexCalculation(double a, double b, double c)
{
    return Math.Pow(a, 2) + b * c - Math.Abs(a - b);
}";

            var virFunction = ConvertMethodToVir(code);

            Assert.AreEqual("ComplexCalculation", virFunction.Name);
            Assert.AreEqual(3, virFunction.Parameters.Count);
            Assert.IsInstanceOf<VirBinaryOperation>(virFunction.Body);

            var binaryOp = (VirBinaryOperation)virFunction.Body;
            Assert.AreEqual(VirOperator.Subtract, binaryOp.Operator);
            Assert.IsInstanceOf<VirBinaryOperation>(binaryOp.Left);
            Assert.IsInstanceOf<VirMethodCall>(binaryOp.Right);
        }

        [Test]
        public void TestVirToMklnetConverter_IfStatement()
        {
            var virFunction = new VirFunction
            {
                Name = "AbsoluteValue",
                Parameters = new List<VirParameter>
                {
                    new VirParameter { Name = "x", Type = new VirType { TypeName = "Tensor", IsScalar = false } }
                },
                Body = new VirConditional
                {
                    Condition = new VirBinaryOperation
                    {
                        Left = new VirVariable { Name = "x" },
                        Right = new VirConstant { Value = 0 },
                        Operator = VirOperator.LessThan
                    },
                    TrueBranch = new VirUnaryOperation
                    {
                        Operand = new VirVariable { Name = "x" },
                        Operator = VirUnaryOperator.Negate
                    },
                    FalseBranch = new VirVariable { Name = "x" }
                }
            };

            var result = _virToMklnetConverter.ConvertToMklnetCode(virFunction);

            var expected = @"public static Tensor AbsoluteValue(
    Tensor x
)
{
    return MKLNET.Where(MKLNET.Less(x, MKLNET.Full(new[] { 1 }, 0)), MKLNET.Neg(x), x);
}";

            Assert.AreEqual(expected, result.Trim());
        }

        [Test]
        public void TestVirToMklnetConverter_UnaryOperation()
        {
            var virFunction = new VirFunction
            {
                Name = "Negate",
                Parameters = new List<VirParameter>
                {
                    new VirParameter { Name = "x", Type = new VirType { TypeName = "Tensor", IsScalar = false } }
                },
                Body = new VirUnaryOperation
                {
                    Operand = new VirVariable { Name = "x" },
                    Operator = VirUnaryOperator.Negate
                }
            };

            var result = _virToMklnetConverter.ConvertToMklnetCode(virFunction);

            var expected = @"public static Tensor Negate(
    Tensor x
)
{
    return MKLNET.Neg(x);
}";

            Assert.AreEqual(expected, result.Trim());
        }

        [Test]
        public void TestVirToMklnetConverter_MethodCall()
        {
            var virFunction = new VirFunction
            {
                Name = "SquareRoot",
                Parameters = new List<VirParameter>
                {
                    new VirParameter { Name = "x", Type = new VirType { TypeName = "Tensor", IsScalar = false } }
                },
                Body = new VirMethodCall
                {
                    MethodName = "Math.Sqrt",
                    Arguments = new List<VirExpression> { new VirVariable { Name = "x" } }
                }
            };

            var result = _virToMklnetConverter.ConvertToMklnetCode(virFunction);

            var expected = @"public static Tensor SquareRoot(
    Tensor x
)
{
    return MKLNET.Sqrt(x);
}";

            Assert.AreEqual(expected, result.Trim());
        }

        [Test]
        public void TestVirToMklnetConverter_ComplexExpression()
        {
            var virFunction = new VirFunction
            {
                Name = "ComplexCalculation",
                Parameters = new List<VirParameter>
                {
                    new VirParameter { Name = "a", Type = new VirType { TypeName = "Tensor", IsScalar = false } },
                    new VirParameter { Name = "b", Type = new VirType { TypeName = "Tensor", IsScalar = false } },
                    new VirParameter { Name = "c", Type = new VirType { TypeName = "Tensor", IsScalar = false } }
                },
                Body = new VirBinaryOperation
                {
                    Left = new VirBinaryOperation
                    {
                        Left = new VirMethodCall
                        {
                            MethodName = "Math.Pow",
                            Arguments = new List<VirExpression>
                            {
                                new VirVariable { Name = "a" },
                                new VirConstant { Value = 2 }
                            }
                        },
                        Right = new VirBinaryOperation
                        {
                            Left = new VirVariable { Name = "b" },
                            Right = new VirVariable { Name = "c" },
                            Operator = VirOperator.Multiply
                        },
                        Operator = VirOperator.Add
                    },
                    Right = new VirMethodCall
                    {
                        MethodName = "Math.Abs",
                        Arguments = new List<VirExpression>
                        {
                            new VirBinaryOperation
                            {
                                Left = new VirVariable { Name = "a" },
                                Right = new VirVariable { Name = "b" },
                                Operator = VirOperator.Subtract
                            }
                        }
                    },
                    Operator = VirOperator.Subtract
                }
            };

            var result = _virToMklnetConverter.ConvertToMklnetCode(virFunction);

            var expected = @"public static Tensor ComplexCalculation(
    Tensor a,
    Tensor b,
    Tensor c
)
{
    return MKLNET.Sub(MKLNET.Add(MKLNET.Pow(a, MKLNET.Full(new[] { 1 }, 2)), MKLNET.Mul(b, c)), MKLNET.Abs(MKLNET.Sub(a, b)));
}";

            Assert.AreEqual(expected, result.Trim());
        }

        private VirFunction ConvertMethodToVir(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();
            var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
            return _roslynToVirConverter.ConvertMethod(method);
        }
    }
}
