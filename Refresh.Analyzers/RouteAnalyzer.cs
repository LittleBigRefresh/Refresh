using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Refresh.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[UsedImplicitly]
public class RouteAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor EmptyRouteRule = new("RFSH001",
        "Endpoint contains empty route parameter",
        "Endpoint contains empty route parameter",
        "Routing",
        DiagnosticSeverity.Error,
        true,
        "This endpoint will be inaccessible due to an empty route. If you want the route to be accessible at the root, add a slash.");
    
    private static readonly DiagnosticDescriptor InvalidRouteRule = new("RFSH002",
        "Endpoint contains invalid characters",
        "Endpoint contains invalid characters",
        "Routing",
        DiagnosticSeverity.Error,
        true,
        "This endpoint will be inaccessible due to invalid characters. All characters in the route must be within ASCII.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EmptyRouteRule, InvalidRouteRule);

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
        string text = arg.GetFirstToken().ValueText;
        
        if (text == string.Empty)
        {
            context.ReportDiagnostic(Diagnostic.Create(EmptyRouteRule, arg.GetLocation()));
        }
        // Check if the string is inside ASCII range.
        // This is really clever. Credit where credit is due: https://stackoverflow.com/a/58305196
        else if(Encoding.UTF8.GetByteCount(text) != text.Length)
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidRouteRule, arg.GetLocation()));
        }

    }

}