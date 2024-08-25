public class MethodAnalyzer : BaseAnalyzer
    {
        public List<SyntaxTongueSentence> Analyze(MethodDeclarationSyntax methodDeclaration)
        {
            var sentences = new List<SyntaxTongueSentence>
            {
                new SyntaxTongueSentence
                {
                    OriginalCode = methodDeclaration.ToString(),
                    SyntaxTongue = $"{(methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)) ? "ASYNC " : "")}METHOD {GeneralizeType(methodDeclaration.ReturnType)} IDENTIFIER {GeneralizeParameterList(methodDeclaration.ParameterList)}"
                }
            };

            sentences.AddRange(AnalyzeSyntax(methodDeclaration.Body));
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