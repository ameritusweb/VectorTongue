using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace SyntaxTongue.Analyzers
{
    public abstract class BaseAnalyzer
    {
        protected string GeneralizeType(TypeSyntax type)
        {
            if (type == null) return "TYPE";

            return type switch
            {
                PredefinedTypeSyntax _ => "TYPE",
                IdentifierNameSyntax _ => "TYPE",
                GenericNameSyntax genericName => $"TYPE GENERIC_START {string.Join(" ", genericName.TypeArgumentList.Arguments.Select(GeneralizeType))} GENERIC_END",
                QualifiedNameSyntax qualifiedName => $"{GeneralizeType(qualifiedName.Left)} DOT {GeneralizeType(qualifiedName.Right)}",
                ArrayTypeSyntax arrayType => $"{GeneralizeType(arrayType.ElementType)} ARRAY",
                NullableTypeSyntax nullableType => $"{GeneralizeType(nullableType.ElementType)} QUESTION",
                _ => "TYPE"
            };
        }

        protected string GeneralizeParameterList(ParameterListSyntax parameterList)
        {
            return $"LPAREN {string.Join(" ", parameterList.Parameters.Select(p => $"{GeneralizeType(p.Type)} IDENTIFIER"))} RPAREN";
        }

        protected List<SyntaxTongueSentence> AnalyzeSyntax(SyntaxNode node)
        {
            var sentences = new List<SyntaxTongueSentence>();

            foreach (var childNode in node.ChildNodes())
            {
                sentences.AddRange(AnalyzeChildNode(childNode));
            }

            return sentences;
        }

        protected virtual List<SyntaxTongueSentence> AnalyzeChildNode(SyntaxNode childNode)
        {
            return new List<SyntaxTongueSentence>();
        }

        protected string GeneralizeExpression(ExpressionSyntax expression)
        {
            if (expression == null) return "NULL";

            return expression switch
            {
                LiteralExpressionSyntax literal => "LITERAL",
                IdentifierNameSyntax identifier => "IDENTIFIER",
                BinaryExpressionSyntax binary => $"{GeneralizeExpression(binary.Left)} OPERATOR {GeneralizeExpression(binary.Right)}",
                InvocationExpressionSyntax invocation => GeneralizeInvocation(invocation),
                MemberAccessExpressionSyntax memberAccess => $"{GeneralizeExpression(memberAccess.Expression)} DOT {memberAccess.Name}",
                ObjectCreationExpressionSyntax objectCreation => $"NEW {GeneralizeType(objectCreation.Type)} {(objectCreation.Initializer != null ? "INITIALIZER" : "")}",
                ImplicitObjectCreationExpressionSyntax implicitObjectCreation => $"NEW LPAREN {string.Join(" ", implicitObjectCreation.ArgumentList.Arguments.Select(GeneralizeExpression))} RPAREN",
                ConditionalAccessExpressionSyntax conditionalAccess => $"{GeneralizeExpression(conditionalAccess.Expression)} QUESTION_DOT {GeneralizeExpression(conditionalAccess.WhenNotNull)}",
                PostfixUnaryExpressionSyntax postfixUnary when postfixUnary.OperatorToken.IsKind(SyntaxKind.ExclamationToken) => $"{GeneralizeExpression(postfixUnary.Operand)} BANG",
                IsPatternExpressionSyntax isPattern => $"{GeneralizeExpression(isPattern.Expression)} IS {GeneralizePattern(isPattern.Pattern)}",
                ConditionalExpressionSyntax conditional => $"{GeneralizeExpression(conditional.Condition)} QUESTION {GeneralizeExpression(conditional.WhenTrue)} COLON {GeneralizeExpression(conditional.WhenFalse)}",
                LambdaExpressionSyntax lambda => GeneralizeLambda(lambda),
                SwitchExpressionSyntax switchExpression => GeneralizeSwitchExpression(switchExpression),
                WithExpressionSyntax withExpression => $"{GeneralizeExpression(withExpression.Expression)} WITH {GeneralizeExpression(withExpression.Initializer)}",
                AnonymousObjectCreationExpressionSyntax anonymousObject => $"NEW ANONYMOUS_OBJECT LBRACE {string.Join(" ", anonymousObject.Initializers.Select(i => $"{i.NameEquals?.Name ?? "IDENTIFIER"} ASSIGN {GeneralizeExpression(i.Expression)}"))} RBRACE",
                TupleExpressionSyntax tuple => $"TUPLE LPAREN {string.Join(" ", tuple.Arguments.Select(a => GeneralizeExpression(a.Expression)))} RPAREN",
                _ => "EXPRESSION"
            };
        }

        protected string GeneralizePattern(PatternSyntax pattern)
        {
            return pattern switch
            {
                ConstantPatternSyntax _ => "CONSTANT_PATTERN",
                DeclarationPatternSyntax declarationPattern => $"DECLARATION_PATTERN {GeneralizeType(declarationPattern.Type)}",
                DiscardPatternSyntax _ => "DISCARD_PATTERN",
                VarPatternSyntax _ => "VAR_PATTERN",
                RecursivePatternSyntax recursivePattern => $"RECURSIVE_PATTERN {GeneralizeType(recursivePattern.Type)} {string.Join(" ", recursivePattern.PropertyPatterns.Select(p => $"{p.NameColon.Name} COLON {GeneralizePattern(p.Pattern)}"))}",
                RelationalPatternSyntax relationalPattern => $"RELATIONAL_PATTERN {relationalPattern.OperatorToken.Text} {GeneralizeExpression(relationalPattern.Expression)}",
                OrPatternSyntax orPattern => $"OR_PATTERN {GeneralizePattern(orPattern.Left)} OR {GeneralizePattern(orPattern.Right)}",
                AndPatternSyntax andPattern => $"AND_PATTERN {GeneralizePattern(andPattern.Left)} AND {GeneralizePattern(andPattern.Right)}",
                NotPatternSyntax notPattern => $"NOT_PATTERN NOT {GeneralizePattern(notPattern.Pattern)}",
                ListPatternSyntax listPattern => $"LIST_PATTERN LBRACKET {string.Join(" ", listPattern.Patterns.Select(GeneralizePattern))} RBRACKET",
                _ => "PATTERN"
            };
        }

        protected string GeneralizeTypeConstraints(TypeParameterConstraintClauseSyntax constraintClause)
        {
            if (constraintClause == null) return string.Empty;

            var constraints = new List<string>();
            foreach (var constraint in constraintClause.Constraints)
            {
                constraints.Add(constraint switch
                {
                    ClassOrStructConstraintSyntax classOrStruct => classOrStruct.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) ? "CLASS" : "STRUCT",
                    TypeConstraintSyntax typeConstraint => $"TYPE {GeneralizeType(typeConstraint.Type)}",
                    ConstructorConstraintSyntax _ => "NEW",
                    _ => "CONSTRAINT"
                });
            }

            return $"WHERE {constraintClause.Name} COLON {string.Join(" ", constraints)}";
        }

        protected List<SyntaxTongueSentence> AnalyzeAttributes(SyntaxList<AttributeListSyntax> attributeLists)
        {
            var sentences = new List<SyntaxTongueSentence>();

            foreach (var attributeList in attributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    sentences.Add(new SyntaxTongueSentence
                    {
                        OriginalCode = attribute.ToString(),
                        SyntaxTongue = $"ATTRIBUTE {attribute.Name} {(attribute.ArgumentList != null ? $"LPAREN {string.Join(" ", attribute.ArgumentList.Arguments.Select(a => GeneralizeExpression(a.Expression)))} RPAREN" : "")}"
                    });
                }
            }

            return sentences;
        }

        protected string GeneralizeLinqClause(QueryClauseSyntax clause)
        {
            return clause switch
            {
                FromClauseSyntax fromClause => $"FROM {GeneralizeType(fromClause.Type)} IDENTIFIER IN {GeneralizeExpression(fromClause.Expression)}",
                WhereClauseSyntax whereClause => $"WHERE {GeneralizeExpression(whereClause.Condition)}",
                SelectClauseSyntax selectClause => $"SELECT {GeneralizeExpression(selectClause.Expression)}",
                GroupClauseSyntax groupClause => $"GROUP {GeneralizeExpression(groupClause.GroupExpression)} BY {GeneralizeExpression(groupClause.ByExpression)}",
                OrderByClauseSyntax orderByClause => $"ORDERBY {string.Join(", ", orderByClause.Orderings.Select(o => $"{GeneralizeExpression(o.Expression)} {(o.AscendingOrDescendingKeyword.Kind() == SyntaxKind.AscendingKeyword ? "ASC" : "DESC")}"))}",
                JoinClauseSyntax joinClause => $"JOIN {GeneralizeType(joinClause.Type)} IDENTIFIER IN {GeneralizeExpression(joinClause.InExpression)} ON {GeneralizeExpression(joinClause.LeftExpression)} EQUALS {GeneralizeExpression(joinClause.RightExpression)}",
                LetClauseSyntax letClause => $"LET IDENTIFIER ASSIGN {GeneralizeExpression(letClause.Expression)}",
                _ => "UNKNOWN_CLAUSE"
            };
        }

        protected string GeneralizeLambda(LambdaExpressionSyntax lambda)
        {
            var parameters = lambda.ParameterList?.Parameters.Select(p => $"{GeneralizeType(p.Type)} IDENTIFIER") ?? new[] { "IDENTIFIER" };
            var body = lambda.ExpressionBody != null
                ? GeneralizeExpression(lambda.ExpressionBody)
                : "BLOCK";
            var modifiers = lambda.Modifiers.Any(SyntaxKind.StaticKeyword) ? "STATIC " : "";
            return $"{modifiers}LAMBDA LPAREN {string.Join(" ", parameters)} RPAREN LAMBDA_ARROW {body}";
        }

        protected string GeneralizeSwitchExpression(SwitchExpressionSyntax switchExpression)
        {
            var arms = switchExpression.Arms.Select(arm => $"{GeneralizePattern(arm.Pattern)} LAMBDA_ARROW {GeneralizeExpression(arm.Expression)}");
            return $"SWITCH {GeneralizeExpression(switchExpression.GoverningExpression)} LBRACE {string.Join(" ", arms)} RBRACE";
        }

        protected string GeneralizeInvocation(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is IdentifierNameSyntax identifier)
            {
                return $"INVOKE_METHOD {identifier.Identifier.Text} LPAREN {string.Join(" ", invocation.ArgumentList.Arguments.Select(GeneralizeExpression))} RPAREN";
            }
            else if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                return $"{GeneralizeExpression(memberAccess.Expression)} DOT INVOKE_METHOD {memberAccess.Name} LPAREN {string.Join(" ", invocation.ArgumentList.Arguments.Select(GeneralizeExpression))} RPAREN";
            }
            return $"INVOKE {GeneralizeExpression(invocation.Expression)} LPAREN {string.Join(" ", invocation.ArgumentList.Arguments.Select(GeneralizeExpression))} RPAREN";
        }
    }
}
