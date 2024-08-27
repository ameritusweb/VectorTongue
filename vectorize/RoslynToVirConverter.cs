using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Vectorization.IntermediateRepresentation;

namespace Vectorization.Converters
{
    public class RoslynToVirConverter
    {
        public VirFunction ConvertMethod(MethodDeclarationSyntax method)
        {
            var virFunction = new VirFunction
            {
                Name = method.Identifier.Text,
                Parameters = method.ParameterList.Parameters.Select(ConvertParameter).ToList(),
                Body = ConvertStatement(method.Body)
            };

            return virFunction;
        }

        private VirParameter ConvertParameter(ParameterSyntax parameter)
        {
            return new VirParameter
            {
                Name = parameter.Identifier.Text,
                Type = new VirType
                {
                    TypeName = parameter.Type.ToString(),
                    IsScalar = true // Assume scalar for now, we'll refine this later
                }
            };
        }

        private VirExpression ConvertForStatement(ForStatementSyntax forStatement)
        {
            // ... (existing for loop conversion logic)

            // Check if the loop body is a reduction
            if (TryConvertReduction(forStatement, out var reduction))
            {
                return reduction;
            }

            // If not a reduction, return the regular VirLoop
            return new VirLoop
            {
                LoopVariable = new VirVariable { Name = initializer.Identifier.Text },
                StartValue = ConvertExpression(initializer.Initializer.Value),
                EndValue = ConvertExpression(condition.Right),
                StepValue = ConvertExpression(((BinaryExpressionSyntax)incrementor.Right).Right),
                Body = ConvertStatement(forStatement.Statement)
            };
        }

        private bool TryConvertReduction(ForStatementSyntax forStatement, out VirReduction reduction)
        {
            reduction = null;
            if (forStatement.Statement is BlockSyntax block && block.Statements.Count == 1)
            {
                var statement = block.Statements[0];
                if (statement is ExpressionStatementSyntax expressionStatement)
                {
                    var expression = expressionStatement.Expression;
                    if (expression is AssignmentExpressionSyntax assignment)
                    {
                        var left = assignment.Left as IdentifierNameSyntax;
                        if (left != null)
                        {
                            switch (assignment.OperatorToken.ValueText)
                            {
                                case "+=":
                                    reduction = new VirReduction
                                    {
                                        Type = ReductionType.Sum,
                                        Expression = ConvertExpression(assignment.Right)
                                    };
                                    return true;
                                case "*=":
                                    reduction = new VirReduction
                                    {
                                        Type = ReductionType.Product,
                                        Expression = ConvertExpression(assignment.Right)
                                    };
                                    return true;
                                // Add cases for max and min if needed
                            }
                        }
                    }
                }
            }
            return false;
        }

        private VirExpression ConvertStatement(StatementSyntax statement)
        {
            switch (statement)
            {
                case BlockSyntax block:
                    // For simplicity, we'll just convert the last statement in the block
                    return ConvertStatement(block.Statements.Last());
                case ReturnStatementSyntax returnStatement:
                    return ConvertExpression(returnStatement.Expression);
                case IfStatementSyntax ifStatement:
                    return ConvertIfStatement(ifStatement);
                case ExpressionStatementSyntax expressionStatement:
                    return ConvertExpression(expressionStatement.Expression);
                case ForStatementSyntax forStatement:
                    return ConvertForStatement(forStatement);
                default:
                    throw new NotImplementedException($"Conversion for {statement.GetType()} is not implemented yet.");
            }
        }

        private VirExpression ConvertIfStatement(IfStatementSyntax ifStatement)
        {
            return new VirConditional
            {
                Condition = ConvertExpression(ifStatement.Condition),
                TrueBranch = ConvertStatement(ifStatement.Statement),
                FalseBranch = ifStatement.Else != null ? ConvertStatement(ifStatement.Else.Statement) : null
            };
        }

        private VirExpression ConvertExpression(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case BinaryExpressionSyntax binaryExpression:
                    return ConvertBinaryExpression(binaryExpression);
                case LiteralExpressionSyntax literalExpression:
                    return ConvertLiteralExpression(literalExpression);
                case IdentifierNameSyntax identifierName:
                    return new VirVariable { Name = identifierName.Identifier.Text };
                case InvocationExpressionSyntax invocationExpression:
                    return ConvertInvocationExpression(invocationExpression);
                case PrefixUnaryExpressionSyntax prefixUnaryExpression:
                    return ConvertPrefixUnaryExpression(prefixUnaryExpression);
                case ParenthesizedExpressionSyntax parenthesizedExpression:
                    return ConvertExpression(parenthesizedExpression.Expression);
                case ConditionalExpressionSyntax conditionalExpression:
                    return ConvertConditionalExpression(conditionalExpression);
                default:
                    throw new NotImplementedException($"Conversion for {expression.GetType()} is not implemented yet.");
            }
        }

        private VirExpression ConvertBinaryExpression(BinaryExpressionSyntax binaryExpression)
        {
            return new VirBinaryOperation
            {
                Left = ConvertExpression(binaryExpression.Left),
                Right = ConvertExpression(binaryExpression.Right),
                Operator = ConvertBinaryOperator(binaryExpression.OperatorToken)
            };
        }

        private VirOperator ConvertBinaryOperator(SyntaxToken operatorToken)
        {
            switch (operatorToken.Kind())
            {
                case SyntaxKind.PlusToken:
                    return VirOperator.Add;
                case SyntaxKind.MinusToken:
                    return VirOperator.Subtract;
                case SyntaxKind.AsteriskToken:
                    return VirOperator.Multiply;
                case SyntaxKind.SlashToken:
                    return VirOperator.Divide;
                case SyntaxKind.CaretToken:
                    return VirOperator.Power;
                case SyntaxKind.EqualsEqualsToken:
                    return VirOperator.Equal;
                case SyntaxKind.ExclamationEqualsToken:
                    return VirOperator.NotEqual;
                case SyntaxKind.LessThanToken:
                    return VirOperator.LessThan;
                case SyntaxKind.LessThanEqualsToken:
                    return VirOperator.LessThanOrEqual;
                case SyntaxKind.GreaterThanToken:
                    return VirOperator.GreaterThan;
                case SyntaxKind.GreaterThanEqualsToken:
                    return VirOperator.GreaterThanOrEqual;
                default:
                    throw new NotImplementedException($"Conversion for operator {operatorToken.Kind()} is not implemented yet.");
            }
        }

        private VirConstant ConvertLiteralExpression(LiteralExpressionSyntax literalExpression)
        {
            return new VirConstant { Value = literalExpression.Token.Value };
        }

        private VirExpression ConvertInvocationExpression(InvocationExpressionSyntax invocationExpression)
        {
            var methodName = invocationExpression.Expression switch
            {
                IdentifierNameSyntax ins => ins.Identifier.Text,
                MemberAccessExpressionSyntax maes => maes.Name.Identifier.Text,
                _ => throw new NotImplementedException($"Unsupported method invocation expression: {invocationExpression.Expression.GetType()}")
            };

            return new VirMethodCall
            {
                MethodName = methodName,
                Arguments = invocationExpression.ArgumentList.Arguments
                    .Select(arg => ConvertExpression(arg.Expression))
                    .ToList()
            };
        }

        private VirExpression ConvertPrefixUnaryExpression(PrefixUnaryExpressionSyntax prefixUnaryExpression)
        {
            return new VirUnaryOperation
            {
                Operand = ConvertExpression(prefixUnaryExpression.Operand),
                Operator = ConvertUnaryOperator(prefixUnaryExpression.OperatorToken)
            };
        }

        private VirUnaryOperator ConvertUnaryOperator(SyntaxToken operatorToken)
        {
            switch (operatorToken.Kind())
            {
                case SyntaxKind.MinusToken:
                    return VirUnaryOperator.Negate;
                case SyntaxKind.PlusToken:
                    return VirUnaryOperator.Plus;
                case SyntaxKind.ExclamationToken:
                    return VirUnaryOperator.LogicalNot;
                default:
                    throw new NotImplementedException($"Conversion for unary operator {operatorToken.Kind()} is not implemented yet.");
            }
        }

        private VirExpression ConvertConditionalExpression(ConditionalExpressionSyntax conditionalExpression)
        {
            return new VirConditional
            {
                Condition = ConvertExpression(conditionalExpression.Condition),
                TrueBranch = ConvertExpression(conditionalExpression.WhenTrue),
                FalseBranch = ConvertExpression(conditionalExpression.WhenFalse)
            };
        }
    }
}