using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace SyntaxTongue.Analyzers
{
    public class PropertyAnalyzer : BaseAnalyzer
    {
        public List<SyntaxTongueSentence> Analyze(PropertyDeclarationSyntax propertyDeclaration)
        {
            var sentences = new List<SyntaxTongueSentence>();

            sentences.AddRange(AnalyzeAttributes(propertyDeclaration.AttributeLists));

            var accessors = propertyDeclaration.AccessorList?.Accessors
                .Select(a => a.Keyword.Text.ToUpper() + (a.Modifiers.Any(SyntaxKind.InitKeyword) ? "_INIT" : ""))
                .ToList() ?? new List<string>();

            var initOnly = propertyDeclaration.Initializer != null ? "INIT_ONLY" : "";

            sentences.Add(new SyntaxTongueSentence
            {
                OriginalCode = propertyDeclaration.ToString(),
                SyntaxTongue = $"{GeneralizeModifiers(propertyDeclaration.Modifiers)} {GeneralizeType(propertyDeclaration.Type)} IDENTIFIER {string.Join(" ", accessors)} {initOnly}"
            });

            if (propertyDeclaration.ExpressionBody != null)
            {
                sentences.Add(new SyntaxTongueSentence
                {
                    OriginalCode = propertyDeclaration.ExpressionBody.ToString(),
                    SyntaxTongue = $"EXPRESSION_BODY LAMBDA_ARROW {GeneralizeExpression(propertyDeclaration.ExpressionBody.Expression)}"
                });
            }

            return sentences;
        }

        // Additional method to generalize property modifiers
        private string GeneralizeModifiers(SyntaxTokenList modifiers)
        {
            var relevantModifiers = new[]
            {
                SyntaxKind.PublicKeyword,
                SyntaxKind.PrivateKeyword,
                SyntaxKind.ProtectedKeyword,
                SyntaxKind.InternalKeyword,
                SyntaxKind.StaticKeyword,
                SyntaxKind.VirtualKeyword,
                SyntaxKind.OverrideKeyword,
                SyntaxKind.AbstractKeyword,
                SyntaxKind.SealedKeyword,
                SyntaxKind.RequiredKeyword
            };

            return string.Join(" ", modifiers
                .Where(m => relevantModifiers.Contains((SyntaxKind)m.RawKind))
                .Select(m => m.Text.ToUpper()));
        }

        // Additional method to generalize property accessors
        private string GeneralizeAccessors(AccessorListSyntax accessorList)
        {
            if (accessorList == null) return string.Empty;

            var accessors = new List<string>();
            foreach (var accessor in accessorList.Accessors)
            {
                if (accessor.Keyword.IsKind(SyntaxKind.GetKeyword))
                    accessors.Add("GET");
                else if (accessor.Keyword.IsKind(SyntaxKind.SetKeyword))
                    accessors.Add("SET");
                else if (accessor.Keyword.IsKind(SyntaxKind.InitKeyword))
                    accessors.Add("INIT");
            }

            return string.Join(" ", accessors);
        }
    }
}
