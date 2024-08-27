using System;
using System.Linq;
using System.Text;
using Vectorization.IntermediateRepresentation;

namespace Vectorization.Converters
{
    public class VirToMklnetConverter
    {
        public string ConvertToMklnetCode(VirFunction function)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"public static Tensor {function.Name}(");
            sb.AppendLine(string.Join(",\n", function.Parameters.Select(p => $"    Tensor {p.Name}")));
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLine($"    return {ConvertExpression(function.Body)};");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string ConvertExpression(VirExpression expression)
        {
            return expression switch
            {
                VirBinaryOperation binaryOperation => ConvertBinaryOperation(binaryOperation),
                VirUnaryOperation unaryOperation => ConvertUnaryOperation(unaryOperation),
                VirConstant constant => $"MKLNET.Full(new[] {{ 1 }}, {constant.Value})",
                VirVariable variable => variable.Name,
                VirLoop loop => ConvertLoop(loop),
                VirMethodCall methodCall => ConvertMethodCall(methodCall),
                VirConditional conditional => ConvertConditional(conditional),
                _ => throw new NotImplementedException($"Conversion for {expression.GetType()} is not implemented yet.")
            };
        }

        private string ConvertReduction(VirReduction reduction)
        {
            string expression = ConvertExpression(reduction.Expression);
            return reduction.Type switch
            {
                ReductionType.Sum => $"MKLNET.Sum({expression})",
                ReductionType.Product => $"MKLNET.Prod({expression})",
                ReductionType.Max => $"MKLNET.Max({expression})",
                ReductionType.Min => $"MKLNET.Min({expression})",
                _ => throw new System.NotImplementedException($"Reduction type {reduction.Type} is not implemented.")
            };
        }

        private string ConvertLoop(VirLoop loop)
        {
            var sb = new StringBuilder();

            string start = ConvertExpression(loop.StartValue);
            string end = ConvertExpression(loop.EndValue);
            string step = ConvertExpression(loop.StepValue);

            sb.AppendLine($"Tensor {loop.LoopVariable.Name} = MKLNET.Arange({start}, {end}, {step});");

            // Check if the loop body is a reduction
            if (loop.Body is VirReduction reduction)
            {
                sb.AppendLine($"return {ConvertReduction(reduction)};");
            }
            else
            {
                // Existing loop conversion logic
                sb.AppendLine($"Tensor result = MKLNET.ZerosLike({loop.LoopVariable.Name});");
                sb.AppendLine($"for (int i = 0; i < {loop.LoopVariable.Name}.Shape[0]; i++)");
                sb.AppendLine("{");
                sb.AppendLine($"    Tensor current{loop.LoopVariable.Name} = {loop.LoopVariable.Name}[i];");
                sb.AppendLine($"    result[i] = {ConvertExpression(loop.Body)};");
                sb.AppendLine("}");
                sb.AppendLine("return result;");
            }

            return sb.ToString();
        }

        private string ConvertBinaryOperation(VirBinaryOperation binaryOperation)
        {
            var left = ConvertExpression(binaryOperation.Left);
            var right = ConvertExpression(binaryOperation.Right);

            return binaryOperation.Operator switch
            {
                VirOperator.Add => $"MKLNET.Add({left}, {right})",
                VirOperator.Subtract => $"MKLNET.Sub({left}, {right})",
                VirOperator.Multiply => $"MKLNET.Mul({left}, {right})",
                VirOperator.Divide => $"MKLNET.Div({left}, {right})",
                VirOperator.Power => $"MKLNET.Pow({left}, {right})",
                VirOperator.Equal => $"MKLNET.Equal({left}, {right})",
                VirOperator.NotEqual => $"MKLNET.NotEqual({left}, {right})",
                VirOperator.LessThan => $"MKLNET.Less({left}, {right})",
                VirOperator.LessThanOrEqual => $"MKLNET.LessEqual({left}, {right})",
                VirOperator.GreaterThan => $"MKLNET.Greater({left}, {right})",
                VirOperator.GreaterThanOrEqual => $"MKLNET.GreaterEqual({left}, {right})",
                _ => throw new NotImplementedException($"Conversion for operator {binaryOperation.Operator} is not implemented yet.")
            };
        }

        private string ConvertUnaryOperation(VirUnaryOperation unaryOperation)
        {
            var operand = ConvertExpression(unaryOperation.Operand);

            return unaryOperation.Operator switch
            {
                VirUnaryOperator.Negate => $"MKLNET.Neg({operand})",
                VirUnaryOperator.Abs => $"MKLNET.Abs({operand})",
                VirUnaryOperator.Sqrt => $"MKLNET.Sqrt({operand})",
                VirUnaryOperator.Plus => operand, // Unary plus doesn't change the value
                VirUnaryOperator.LogicalNot => $"MKLNET.LogicalNot({operand})",
                _ => throw new NotImplementedException($"Conversion for unary operator {unaryOperation.Operator} is not implemented yet.")
            };
        }

        private string ConvertConstant(VirConstant constant)
        {
            if (constant.Value is bool boolValue)
            {
                return boolValue ? "MKLNET.OnesLike(MKLNET.Ones())" : "MKLNET.ZerosLike(MKLNET.Ones())";
            }
            return $"MKLNET.Full(new[] {{ 1 }}, {constant.Value})";
        }

        private string ConvertMethodCall(VirMethodCall methodCall)
        {
            var arguments = string.Join(", ", methodCall.Arguments.Select(ConvertExpression));

            return methodCall.MethodName switch
            {
                "Math.Pow" => $"MKLNET.Pow({arguments})",
                "Math.Sqrt" => $"MKLNET.Sqrt({arguments})",
                "Math.Abs" => $"MKLNET.Abs({arguments})",
                "Math.Sin" => $"MKLNET.Sin({arguments})",
                "Math.Cos" => $"MKLNET.Cos({arguments})",
                "Math.Tan" => $"MKLNET.Tan({arguments})",
                "Math.Log" => $"MKLNET.Log({arguments})",
                "Math.Log10" => $"MKLNET.Log10({arguments})",
                "Math.Exp" => $"MKLNET.Exp({arguments})",
                "Math.Floor" => $"MKLNET.Floor({arguments})",
                "Math.Ceiling" => $"MKLNET.Ceil({arguments})",
                "Math.Round" => $"MKLNET.Round({arguments})",
                "Math.Max" => $"MKLNET.Maximum({arguments})",
                "Math.Min" => $"MKLNET.Minimum({arguments})",
                _ => $"{methodCall.MethodName}({arguments})" // For unknown methods, pass through
            };
        }

        private string ConvertConditional(VirConditional conditional)
        {
            var condition = ConvertExpression(conditional.Condition);
            var trueBranch = ConvertExpression(conditional.TrueBranch);
            var falseBranch = ConvertExpression(conditional.FalseBranch);

            return $"MKLNET.Where({condition}, {trueBranch}, {falseBranch})";
        }
    }
}