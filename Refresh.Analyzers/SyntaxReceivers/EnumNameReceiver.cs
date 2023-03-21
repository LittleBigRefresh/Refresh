using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Refresh.Analyzers.SyntaxReceivers;

public class EnumNameReceiver : ISyntaxReceiver
{
    public Dictionary<string, List<string>> Enums { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        foreach (EnumDeclarationSyntax declaration in syntaxNode.DescendantNodes().OfType<EnumDeclarationSyntax>())
        {
            string className = declaration.Identifier.Value!.ToString();
            List<string> enumMembers = declaration.Members.Select(m => m.Identifier.ValueText).ToList();

            this.Enums.TryAdd(className, enumMembers);
        }
    }
}