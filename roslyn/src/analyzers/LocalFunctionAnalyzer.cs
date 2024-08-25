public class LocalFunctionAnalyzer : BaseAnalyzer
{
    public List<SyntaxTongueSentence> Analyze(LocalFunctionStatementSyntax localFunction)
    {
        return new List<SyntaxTongueSentence>
        {
            new SyntaxTongueSentence
            {
                OriginalCode = localFunction.ToString(),
                SyntaxTongue = $"LOCAL_FUNCTION {GeneralizeType(localFunction.ReturnType)} IDENTIFIER {GeneralizeParameterList(localFunction.ParameterList)} {(localFunction.ExpressionBody != null ? $"LAMBDA_ARROW {GeneralizeExpression(localFunction.ExpressionBody.Expression)}" : "")}"
            }
        };
    }
}