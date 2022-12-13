using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Refresh.Analyzers.Extensions;

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
    
    private static readonly DiagnosticDescriptor RouteInInvalidClassRule = new("RFSH003",
        "Endpoint outside of EndpointGroup",
        "Endpoint outside of EndpointGroup",
        "Routing",
        DiagnosticSeverity.Error,
        true,
        "In order for endpoints to be detected, they must first be in an EndpointGroup. Your class must extend this class.");
    
    private static readonly DiagnosticDescriptor RouteMustUseCorrectContextRule = new("RFSH004",
        "Route must use RequestContext type in first argument",
        "Route must use RequestContext type in first argument",
        "Routing",
        DiagnosticSeverity.Error,
        true);


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
        => ImmutableArray.Create(EmptyRouteRule,
            InvalidRouteRule,
            RouteInInvalidClassRule,
            RouteMustUseCorrectContextRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.Attribute);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
    }

    private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
    {
        AttributeSyntax attribute = (AttributeSyntax)context.Node;
        if(!attribute.Name.ToString().EndsWith("Endpoint")) return;

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

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        IMethodSymbol method = (IMethodSymbol)context.Symbol;
        if (!method.GetAttributes().Any(a => a.AttributeClass?.Name.EndsWith("EndpointAttribute") ?? false)) return;

        INamedTypeSymbol namedType = method.ContainingType;

        if (!namedType.BaseType?.Name.Contains("EndpointGroup") ?? true)
        {
            method.Locations.ReportDiagnosticsForAll(context, RouteInInvalidClassRule);
        }
        
        if(method.Parameters.Length == 0) method.Locations.ReportDiagnosticsForAll(context, RouteMustUseCorrectContextRule);
        else
        {
            IParameterSymbol param = method.Parameters[0];
            if(param.Type.Name != "RequestContext")
                method.Locations.ReportDiagnosticsForAll(context, RouteMustUseCorrectContextRule);

            // bool hasAuthAttribute = method.GetAttributes()
                // .Any(a => a.AttributeClass?.Name.EndsWith("AuthenticationAttribute") ?? false);
        }
    }

}