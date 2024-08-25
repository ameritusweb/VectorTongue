public class MethodAnalyzer : BaseAnalyzer
{
    public List<SyntaxTongueSentence> Analyze(MethodDeclarationSyntax methodDeclaration)
    {
        var sentences = new List<SyntaxTongueSentence>();

        sentences.AddRange(AnalyzeAttributes(methodDeclaration.AttributeLists));

        var modifiers = GeneralizeModifiers(methodDeclaration.Modifiers);
        var returnType = GeneralizeType(methodDeclaration.ReturnType);
        var parameters = GeneralizeParameterList(methodDeclaration.ParameterList);

        sentences.Add(new SyntaxTongueSentence
        {
            OriginalCode = methodDeclaration.ToString(),
            SyntaxTongue = $"{modifiers} METHOD {returnType} IDENTIFIER {parameters}"
        });

        if (methodDeclaration.ExpressionBody != null)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = methodDeclaration.ExpressionBody.ToString(),
                SyntaxTongue = $"EXPRESSION_BODY LAMBDA_ARROW {GeneralizeExpression(methodDeclaration.ExpressionBody.Expression)}"
            });
        }
        else if (methodDeclaration.Body != null)
        {
            sentences.AddRange(AnalyzeSyntax(methodDeclaration.Body));
        }

        return sentences;
    }

    protected override List<SyntaxTongueSentence> AnalyzeChildNode(SyntaxNode childNode)
    {
        return childNode switch
        {
            LocalDeclarationStatementSyntax localDeclaration => new List<SyntaxTongueSentence>
            {
                new SyntaxTongueSentence
                {
                    OriginalCode = localDeclaration.ToString(),
                    SyntaxTongue = $"VAR IDENTIFIER ASSIGN {GeneralizeExpression(localDeclaration.Declaration.Variables.First().Initializer.Value)}"
                }
            },
            IfStatementSyntax ifStatement => new List<SyntaxTongueSentence>
            {
                new SyntaxTongueSentence
                {
                    OriginalCode = ifStatement.Condition.ToString(),
                    SyntaxTongue = $"IF LPAREN {GeneralizeExpression(ifStatement.Condition)} RPAREN"
                }
            },
            ForEachStatementSyntax forEachStatement => new List<SyntaxTongueSentence>
            {
                new SyntaxTongueSentence
                {
                    OriginalCode = forEachStatement.ToString(),
                    SyntaxTongue = $"FOREACH LPAREN {GeneralizeType(forEachStatement.Type)} IDENTIFIER IN {GeneralizeExpression(forEachStatement.Expression)} RPAREN"
                }
            },
            _ => new List<SyntaxTongueSentence>()
        };
    }
}