using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Refresh.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[UsedImplicitly]
public class RouteSlashAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new("RFSH001",
        "Endpoint contains empty route parameter",
        "Endpoint contains empty route parameter",
        "Routing",
        DiagnosticSeverity.Error,
        true,
        "This endpoint will be inaccessible due to an empty route. If you want the route to be accessible at the root, add a slash.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.Attribute);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        AttributeSyntax attribute = (AttributeSyntax)context.Node;
        if(attribute.Name.ToString() != "Endpoint") return;

        AttributeArgumentSyntax arg = attribute.ArgumentList!.Arguments[0];
        if (arg.GetFirstToken().ValueText == string.Empty)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, arg.GetLocation()));
        }
        
    }

}