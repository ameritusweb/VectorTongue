public class StructAnalyzer : BaseAnalyzer
{
    public List<SyntaxTongueSentence> Analyze(StructDeclarationSyntax structDeclaration)
    {
        var sentences = new List<SyntaxTongueSentence>();

        sentences.AddRange(AnalyzeAttributes(structDeclaration.AttributeLists));

        sentences.Add(new SyntaxTongueSentence
        {
            OriginalCode = structDeclaration.Identifier.ToString(),
            SyntaxTongue = $"STRUCT IDENTIFIER"
        });

        if (structDeclaration.TypeParameterList != null)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = structDeclaration.TypeParameterList.ToString(),
                SyntaxTongue = $"GENERIC_START {string.Join(" ", structDeclaration.TypeParameterList.Parameters.Select(p => "TYPE_PARAMETER"))} GENERIC_END"
            });
        }

        foreach (var constraintClause in structDeclaration.ConstraintClauses)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = constraintClause.ToString(),
                SyntaxTongue = GeneralizeTypeConstraints(constraintClause)
            });
        }

        var methodAnalyzer = new MethodAnalyzer();
        var propertyAnalyzer = new PropertyAnalyzer();

        foreach (var member in structDeclaration.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax method:
                    sentences.AddRange(methodAnalyzer.Analyze(method));
                    break;
                case PropertyDeclarationSyntax property:
                    sentences.AddRange(propertyAnalyzer.Analyze(property));
                    break;
            }
        }

        return sentences;
    }
}