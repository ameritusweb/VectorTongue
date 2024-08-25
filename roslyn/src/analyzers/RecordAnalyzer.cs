public class RecordAnalyzer : BaseAnalyzer
{
    public List<SyntaxTongueSentence> Analyze(RecordDeclarationSyntax recordDeclaration)
    {
        var sentences = new List<SyntaxTongueSentence>();

        sentences.AddRange(AnalyzeAttributes(recordDeclaration.AttributeLists));

        var recordKind = recordDeclaration.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) ? "CLASS" : "STRUCT";

        sentences.Add(new SyntaxTongueSentence
        {
            OriginalCode = recordDeclaration.Identifier.ToString(),
            SyntaxTongue = $"RECORD {recordKind} IDENTIFIER"
        });

        if (recordDeclaration.ParameterList != null)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = recordDeclaration.ParameterList.ToString(),
                SyntaxTongue = $"LPAREN {string.Join(" ", recordDeclaration.ParameterList.Parameters.Select(p => $"{GeneralizeType(p.Type)} IDENTIFIER"))} RPAREN"
            });
        }

        if (recordDeclaration.BaseList != null)
        {
            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = recordDeclaration.BaseList.ToString(),
                SyntaxTongue = $"COLON {string.Join(" ", recordDeclaration.BaseList.Types.Select(t => GeneralizeType(t.Type)))}"
            });
        }

        var methodAnalyzer = new MethodAnalyzer();
        var propertyAnalyzer = new PropertyAnalyzer();

        foreach (var member in recordDeclaration.Members)
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