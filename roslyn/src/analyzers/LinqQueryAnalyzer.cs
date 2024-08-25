public class LinqQueryAnalyzer : BaseAnalyzer
{
    public List<SyntaxTongueSentence> Analyze(QueryExpressionSyntax queryExpression)
    {
        var sentences = new List<SyntaxTongueSentence>();

        sentences.Add(new SyntaxTongueSentence
        {
            OriginalCode = queryExpression.ToString(),
            SyntaxTongue = "LINQ_QUERY_START"
        });

        foreach (var clause in queryExpression.Body.Clauses)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = clause.ToString(),
                SyntaxTongue = GeneralizeLinqClause(clause)
            });
        }

        if (queryExpression.Body.SelectOrGroup is SelectClauseSyntax selectClause)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = selectClause.ToString(),
                SyntaxTongue = GeneralizeLinqClause(selectClause)
            });
        }
        else if (queryExpression.Body.SelectOrGroup is GroupClauseSyntax groupClause)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = groupClause.ToString(),
                SyntaxTongue = GeneralizeLinqClause(groupClause)
            });
        }

        sentences.Add(new SyntaxTongueSentence
        {
            OriginalCode = queryExpression.ToString(),
            SyntaxTongue = "LINQ_QUERY_END"
        });

        return sentences;
    }

    public List<SyntaxTongueSentence> AnalyzeMethodChain(InvocationExpressionSyntax invocation)
        {
            var sentences = new List<SyntaxTongueSentence>();
            var currentExpression = invocation;

            while (currentExpression != null)
            {
                if (currentExpression.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    sentences.Add(new SyntaxTongueSentence
                    {
                        OriginalCode = currentExpression.ToString(),
                        SyntaxTongue = $"LINQ_METHOD {memberAccess.Name} LPAREN {AnalyzeLinqMethodArguments(currentExpression.ArgumentList)} RPAREN"
                    });

                    currentExpression = memberAccess.Expression as InvocationExpressionSyntax;
                }
                else
                {
                    sentences.Add(new SyntaxTongueSentence
                    {
                        OriginalCode = currentExpression.ToString(),
                        SyntaxTongue = GeneralizeExpression(currentExpression)
                    });
                    break;
                }
            }

            sentences.Reverse(); // Reverse to get the correct order of method calls
            return sentences;
        }

        private string AnalyzeLinqMethodArguments(ArgumentListSyntax argumentList)
        {
            return string.Join(" ", argumentList.Arguments.Select(arg =>
            {
                if (arg.Expression is LambdaExpressionSyntax lambda)
                {
                    return GeneralizeLambda(lambda);
                }
                else if (arg.Expression is SimpleLambdaExpressionSyntax simpleLambda)
                {
                    return $"LAMBDA LPAREN IDENTIFIER RPAREN LAMBDA_ARROW {GeneralizeExpression(simpleLambda.Body)}";
                }
                else if (arg.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    return $"METHOD_GROUP {GeneralizeExpression(memberAccess.Expression)} DOT {memberAccess.Name}";
                }
                return GeneralizeExpression(arg.Expression);
            }));
        }
}