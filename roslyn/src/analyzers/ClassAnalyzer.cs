public class ClassAnalyzer : BaseAnalyzer
{
    public List<SyntaxTongueSentence> Analyze(ClassDeclarationSyntax classDeclaration)
    {
        var sentences = new List<SyntaxTongueSentence>();

        sentences.AddRange(AnalyzeAttributes(classDeclaration.AttributeLists));

        sentences.Add(new SyntaxTongueSentence
        {
            OriginalCode = classDeclaration.Identifier.ToString(),
            SyntaxTongue = $"{(classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)) ? "PARTIAL " : "")}CLASS IDENTIFIER {(classDeclaration.Parent is ClassDeclarationSyntax ? "NESTED" : "")}"
        });

        if (classDeclaration.TypeParameterList != null)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = classDeclaration.TypeParameterList.ToString(),
                SyntaxTongue = $"GENERIC_START {string.Join(" ", classDeclaration.TypeParameterList.Parameters.Select(p => "TYPE_PARAMETER"))} GENERIC_END"
            });
        }

        foreach (var constraintClause in classDeclaration.ConstraintClauses)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = constraintClause.ToString(),
                SyntaxTongue = GeneralizeTypeConstraints(constraintClause)
            });
        }

        var methodAnalyzer = new MethodAnalyzer();
        var propertyAnalyzer = new PropertyAnalyzer();

        foreach (var member in classDeclaration.Members)
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