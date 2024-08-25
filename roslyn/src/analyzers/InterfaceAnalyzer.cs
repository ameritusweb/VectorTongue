public class InterfaceAnalyzer : BaseAnalyzer
    {
        public List<SyntaxTongueSentence> Analyze(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var sentences = new List<SyntaxTongueSentence>();

            sentences.AddRange(AnalyzeAttributes(interfaceDeclaration.AttributeLists));

            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = interfaceDeclaration.Identifier.ToString(),
                SyntaxTongue = $"INTERFACE IDENTIFIER"
            });

            if (interfaceDeclaration.TypeParameterList != null)
            {
                sentences.Add(new SyntaxTongueSentence
                {
                    OriginalCode = interfaceDeclaration.TypeParameterList.ToString(),
                    SyntaxTongue = $"GENERIC_START {string.Join(" ", interfaceDeclaration.TypeParameterList.Parameters.Select(p => "TYPE_PARAMETER"))} GENERIC_END"
                });
            }

            foreach (var constraintClause in interfaceDeclaration.ConstraintClauses)
            {
                sentences.Add(new SyntaxTongueSentence
                {
                    OriginalCode = constraintClause.ToString(),
                    SyntaxTongue = GeneralizeTypeConstraints(constraintClause)
                });
            }

            var methodAnalyzer = new MethodAnalyzer();
            var propertyAnalyzer = new PropertyAnalyzer();

            foreach (var member in interfaceDeclaration.Members)
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