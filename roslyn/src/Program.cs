using System;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxTongue.Analyzers;

namespace SyntaxTongue
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceCode = "... source code here";

            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = tree.GetCompilationUnitRoot();

            var classAnalyzer = new ClassAnalyzer();
            var interfaceAnalyzer = new InterfaceAnalyzer();
            var enumAnalyzer = new EnumAnalyzer();
            var structAnalyzer = new StructAnalyzer();
            var linqQueryAnalyzer = new LinqQueryAnalyzer();
            var recordAnalyzer = new RecordAnalyzer();
            var topLevelStatementAnalyzer = new TopLevelStatementAnalyzer();

            var allSentences = new List<SyntaxTongueSentence>();

            foreach (var node in root.DescendantNodes())
            {
                switch (node)
                {
                    case ClassDeclarationSyntax classDeclaration:
                        allSentences.AddRange(classAnalyzer.Analyze(classDeclaration));
                        break;
                    case InterfaceDeclarationSyntax interfaceDeclaration:
                        allSentences.AddRange(interfaceAnalyzer.Analyze(interfaceDeclaration));
                        break;
                    case EnumDeclarationSyntax enumDeclaration:
                        allSentences.AddRange(enumAnalyzer.Analyze(enumDeclaration));
                        break;
                    case StructDeclarationSyntax structDeclaration:
                        allSentences.AddRange(structAnalyzer.Analyze(structDeclaration));
                        break;
                    case QueryExpressionSyntax queryExpression:
                        allSentences.AddRange(linqQueryAnalyzer.Analyze(queryExpression));
                        break;
                    case RecordDeclarationSyntax recordDeclaration:
                        allSentences.AddRange(recordAnalyzer.Analyze(recordDeclaration));
                        break;
                    case InvocationExpressionSyntax invocation when IsLinqMethodChain(invocation):
                        allSentences.AddRange(linqQueryAnalyzer.AnalyzeMethodChain(invocation));
                        break;
                    case GlobalStatementSyntax globalStatement:
                        allSentences.AddRange(topLevelStatementAnalyzer.Analyze(globalStatement));
                        break;
                }
            }

            var json = JsonSerializer.Serialize(new { sentences = allSentences }, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
        }
    }
}
