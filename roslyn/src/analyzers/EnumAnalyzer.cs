public class EnumAnalyzer : BaseAnalyzer
{
    public List<SyntaxTongueSentence> Analyze(EnumDeclarationSyntax enumDeclaration)
    {
        var sentences = new List<SyntaxTongueSentence>();

        sentences.AddRange(AnalyzeAttributes(enumDeclaration.AttributeLists));

        sentences.Add(new SyntaxTongueSentence
        {
            OriginalCode = enumDeclaration.Identifier.ToString(),
            SyntaxTongue = $"ENUM IDENTIFIER"
        });

        foreach (var member in enumDeclaration.Members)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = member.ToString(),
                SyntaxTongue = $"ENUM_MEMBER IDENTIFIER {(member.EqualsValue != null ? $"ASSIGN {GeneralizeExpression(member.EqualsValue.Value)}" : "")}"
            });
        }

        return sentences;
    }
}