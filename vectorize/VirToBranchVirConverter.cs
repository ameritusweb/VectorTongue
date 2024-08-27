using System;
using System.Collections.Generic;
using System.Linq;
using Vectorization.IntermediateRepresentation;
using Vectorization.IntermediateRepresentation.BranchFocused;

namespace Vectorization.Converters
{
    public class VirToBranchVirConverter
    {
        private Dictionary<VirExpression, BranchVirExpression> _expressionMap = new Dictionary<VirExpression, BranchVirExpression>();

        public BranchVirFunction Convert(VirFunction function)
        {
            return new BranchVirFunction
            {
                Name = function.Name,
                Parameters = function.Parameters.Select(ConvertParameter).ToList(),
                Body = ConvertExpression(function.Body)
            };
        }

        private BranchVirParameter ConvertParameter(VirParameter parameter)
        {
            return new BranchVirParameter
            {
                Name = parameter.Name,
                Type = new BranchVirType
                {
                    TypeName = parameter.Type.TypeName,
                    IsScalar = parameter.Type.IsScalar
                }
            };
        }

        private BranchVirExpression ConvertExpression(VirExpression expression)
        {
            if (_expressionMap.TryGetValue(expression, out var cachedExpression))
            {
                return cachedExpression;
            }

            BranchVirExpression result = expression switch
            {
                VirBinaryOperation binaryOp => ConvertBinaryOperation(binaryOp),
                VirUnaryOperation unaryOp => ConvertUnaryOperation(unaryOp),
                VirVariable variable => new BranchVirVariable { Name = variable.Name },
                VirConstant constant => new BranchVirConstant { Value = constant.Value },
                VirMethodCall methodCall => ConvertMethodCall(methodCall),
                VirConditional conditional => ConvertConditional(conditional),
                VirLoop loop => ConvertLoop(loop),
                _ => throw new NotImplementedException($"Conversion for {expression.GetType()} is not implemented.")
            };

            _expressionMap[expression] = result;
            return result;
        }

        private BranchVirOperation ConvertBinaryOperation(VirBinaryOperation binaryOp)
        {
            return new BranchVirOperation
            {
                OperationName = binaryOp.Operator.ToString(),
                Inputs = new List<BranchVirExpression>
                {
                    ConvertExpression(binaryOp.Left),
                    ConvertExpression(binaryOp.Right)
                }
            };
        }

        private BranchVirOperation ConvertUnaryOperation(VirUnaryOperation unaryOp)
        {
            return new BranchVirOperation
            {
                OperationName = unaryOp.Operator.ToString(),
                Inputs = new List<BranchVirExpression> { ConvertExpression(unaryOp.Operand) }
            };
        }

        private BranchVirOperation ConvertMethodCall(VirMethodCall methodCall)
        {
            return new BranchVirOperation
            {
                OperationName = methodCall.MethodName,
                Inputs = methodCall.Arguments.Select(ConvertExpression).ToList()
            };
        }

        private BranchVirBranch ConvertConditional(VirConditional conditional)
        {
            return new BranchVirBranch
            {
                Source = ConvertExpression(conditional.Condition),
                Branches = new List<BranchVirExpression>
                {
                    ConvertExpression(conditional.TrueBranch),
                    ConvertExpression(conditional.FalseBranch)
                },
                Combination = new BranchVirOperation
                {
                    OperationName = "Select",
                    Inputs = new List<BranchVirExpression> { new BranchVirVariable { Name = "condition" } }
                }
            };
        }

        private BranchVirOperation ConvertLoop(VirLoop loop)
        {
            // This is a simplified conversion. You might need to adjust this based on how you want to represent loops in your branch-focused VIR.
            return new BranchVirOperation
            {
                OperationName = "Loop",
                Inputs = new List<BranchVirExpression>
                {
                    ConvertExpression(loop.StartValue),
                    ConvertExpression(loop.EndValue),
                    ConvertExpression(loop.StepValue),
                    ConvertExpression(loop.Body)
                }
            };
        }
    }
}