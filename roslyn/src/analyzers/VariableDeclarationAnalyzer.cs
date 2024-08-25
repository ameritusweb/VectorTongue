public class VariableDeclarationAnalyzer : BaseAnalyzer
{
    public List<SyntaxTongueSentence> Analyze(VariableDeclarationSyntax variableDeclaration)
    {
        var sentences = new List<SyntaxTongueSentence>();
        foreach (var variable in variableDeclaration.Variables)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = variable.ToString(),
                SyntaxTongue = $"VAR IDENTIFIER {(variable.Initializer != null ? $"ASSIGN {GeneralizeExpression(variable.Initializer.Value)}" : "")}"
            });
        }
        return sentences;
    }
}