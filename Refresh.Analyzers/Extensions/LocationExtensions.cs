using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Refresh.Analyzers.Extensions;

public static class LocationExtensions
{
    public static void ReportDiagnosticsForAll(this ImmutableArray<Location> locations, SymbolAnalysisContext ctx, DiagnosticDescriptor descriptor)
    {
        foreach (Location location in locations)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(descriptor, location));
        }
    }
}