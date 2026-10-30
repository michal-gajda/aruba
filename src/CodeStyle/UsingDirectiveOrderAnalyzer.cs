namespace Aruba.CodeStyle;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UsingDirectiveOrderAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ARUBA001";
    public const string ExplicitRecordTypeDiagnosticId = "ARUBA002";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Order using directives by namespace group",
        "Order using directives as System, project, then external namespaces, with blank lines between groups",
        "Style",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ExplicitRecordTypeRule = new(
        ExplicitRecordTypeDiagnosticId,
        "Specify the record type explicitly",
        "Specify 'class' or 'struct' in the record declaration",
        "Style",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, ExplicitRecordTypeRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeUsingDirective, SyntaxKind.UsingDirective);
        context.RegisterSyntaxNodeAction(AnalyzeRecordDeclaration, SyntaxKind.RecordDeclaration);
    }

    private static void AnalyzeRecordDeclaration(SyntaxNodeAnalysisContext context)
    {
        var declaration = (RecordDeclarationSyntax)context.Node;
        if (declaration.ClassOrStructKeyword.IsKind(SyntaxKind.None))
        {
            context.ReportDiagnostic(Diagnostic.Create(ExplicitRecordTypeRule, declaration.Keyword.GetLocation()));
        }
    }

    public static SyntaxList<UsingDirectiveSyntax> GetDirectives(UsingDirectiveSyntax directive) => directive.Parent switch
    {
        CompilationUnitSyntax compilationUnit => compilationUnit.Usings,
        BaseNamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration.Usings,
        _ => default,
    };

    public static int GetGroup(UsingDirectiveSyntax directive, string projectPrefix)
    {
        var name = directive.Name?.ToString() ?? string.Empty;
        if (string.Equals(name.Split('.')[0], "System", StringComparison.Ordinal))
        {
            return 0;
        }

        projectPrefix = projectPrefix.Trim().TrimEnd('.');
        if (projectPrefix.Length > 0 &&
            (string.Equals(name, projectPrefix, StringComparison.Ordinal) ||
             name.StartsWith(projectPrefix + ".", StringComparison.Ordinal)))
        {
            return 1;
        }

        return 2;
    }

    public static string GetProjectPrefix(AnalyzerConfigOptions options) =>
        options.TryGetValue("aruba_project_namespace_prefix", out var configuredPrefix)
            ? configuredPrefix
            : "Aruba";

    public static bool CanReorder(SyntaxList<UsingDirectiveSyntax> directives) =>
        directives.All(directive => directive.DescendantTrivia(descendIntoTrivia: true).All(IsWhitespace));

    public static IOrderedEnumerable<UsingDirectiveSyntax> GetOrdered(
        SyntaxList<UsingDirectiveSyntax> directives,
        string projectPrefix) =>
        directives
            .OrderBy(directive => GetGroup(directive, projectPrefix))
            .ThenBy(directive => directive.Name?.ToString(), StringComparer.Ordinal)
            .ThenBy(directive => directive.ToString(), StringComparer.Ordinal);

    private static void AnalyzeUsingDirective(SyntaxNodeAnalysisContext context)
    {
        var directive = (UsingDirectiveSyntax)context.Node;
        var directives = GetDirectives(directive);
        if (directives.Count < 2 || !ReferenceEquals(directives[0], directive) || !CanReorder(directives))
        {
            return;
        }

        var projectPrefix = GetProjectPrefix(context.Options.AnalyzerConfigOptionsProvider.GetOptions(directive.SyntaxTree));
        var ordered = GetOrdered(directives, projectPrefix).ToArray();
        var hasIncorrectOrder = !directives.SequenceEqual(ordered);
        var hasIncorrectSpacing = HasIncorrectSpacing(directives, context);
        if (hasIncorrectOrder || hasIncorrectSpacing)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, directives[0].GetLocation()));
        }
    }

    private static bool HasIncorrectSpacing(
        SyntaxList<UsingDirectiveSyntax> directives,
        SyntaxNodeAnalysisContext context)
    {
        var sourceText = directives[0].SyntaxTree.GetText(context.CancellationToken);
        for (var index = 1; index < directives.Count; index++)
        {
            var previousLine = sourceText.Lines.GetLineFromPosition(directives[index - 1].Span.End).LineNumber;
            var currentLine = sourceText.Lines.GetLineFromPosition(directives[index].SpanStart).LineNumber;
            var blankLines = currentLine - previousLine - 1;
            var groupChanged = GetGroup(directives[index - 1], GetProjectPrefix(context.Options.AnalyzerConfigOptionsProvider.GetOptions(directives[0].SyntaxTree))) !=
                               GetGroup(directives[index], GetProjectPrefix(context.Options.AnalyzerConfigOptionsProvider.GetOptions(directives[0].SyntaxTree)));

            if ((groupChanged && blankLines < 1) || (!groupChanged && blankLines > 0))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsWhitespace(SyntaxTrivia trivia) =>
        trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia);
}