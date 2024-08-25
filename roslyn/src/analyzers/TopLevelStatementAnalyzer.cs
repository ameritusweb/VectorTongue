public class TopLevelStatementAnalyzer : BaseAnalyzer
    {
        public List<SyntaxTongueSentence> Analyze(GlobalStatementSyntax globalStatement)
        {
            var sentences = new List<SyntaxTongueSentence>
            {
                new SyntaxTongueSentence
                {
                    OriginalCode = globalStatement.ToString(),
                    SyntaxTongue = "TOP_LEVEL_STATEMENT"
                }
            };

            sentences.AddRange(AnalyzeSyntax(globalStatement.Statement));

            return sentences;
        }
    }