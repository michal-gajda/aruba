namespace Aruba.CodeStyle;

using System.Collections.Immutable;
using System.Composition;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsingDirectiveOrderCodeFixProvider)), Shared]
public sealed class UsingDirectiveOrderCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        [UsingDirectiveOrderAnalyzer.DiagnosticId, UsingDirectiveOrderAnalyzer.ExplicitRecordTypeDiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (context.Diagnostics.Any(diagnostic => diagnostic.Id == UsingDirectiveOrderAnalyzer.ExplicitRecordTypeDiagnosticId))
        {
            var recordDeclaration = root?.FindNode(context.Span).FirstAncestorOrSelf<RecordDeclarationSyntax>();
            if (recordDeclaration is null || !recordDeclaration.ClassOrStructKeyword.IsKind(SyntaxKind.None))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Add explicit 'class' keyword",
                    cancellationToken => AddExplicitClassAsync(context.Document, recordDeclaration, cancellationToken),
                    equivalenceKey: "AddExplicitRecordClass"),
                context.Diagnostics);
            return;
        }

        var directive = root?.FindNode(context.Span).FirstAncestorOrSelf<UsingDirectiveSyntax>();
        if (directive is null)
        {
            return;
        }

        var directives = UsingDirectiveOrderAnalyzer.GetDirectives(directive);
        if (directives.Count < 2 || !UsingDirectiveOrderAnalyzer.CanReorder(directives))
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Order using directives by namespace group",
                cancellationToken => ReorderAsync(context.Document, directives, cancellationToken),
                equivalenceKey: "OrderUsingDirectives"),
            context.Diagnostics);
    }

    private static async Task<Document> AddExplicitClassAsync(
        Document document,
        RecordDeclarationSyntax recordDeclaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var classKeyword = SyntaxFactory.Token(SyntaxKind.ClassKeyword)
            .WithTrailingTrivia(SyntaxFactory.Whitespace(" "));
        var updatedRecord = recordDeclaration.WithClassOrStructKeyword(classKeyword);
        return document.WithSyntaxRoot(root.ReplaceNode(recordDeclaration, updatedRecord));
    }

    private static async Task<Document> ReorderAsync(
        Document document,
        SyntaxList<UsingDirectiveSyntax> directives,
        CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var options = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(directives[0].SyntaxTree);
        var projectPrefix = UsingDirectiveOrderAnalyzer.GetProjectPrefix(options);
        var ordered = UsingDirectiveOrderAnalyzer.GetOrdered(directives, projectPrefix).ToArray();

        var firstLine = sourceText.Lines.GetLineFromPosition(directives[0].SpanStart);
        var lastLine = sourceText.Lines.GetLineFromPosition(directives[directives.Count - 1].Span.End);
        var indentation = sourceText.ToString(TextSpan.FromBounds(firstLine.Start, directives[0].SpanStart));
        var newline = sourceText.ToString(TextSpan.FromBounds(firstLine.End, firstLine.EndIncludingLineBreak));
        if (newline.Length == 0)
        {
            newline = "\n";
        }

        var replacement = new StringBuilder();
        for (var index = 0; index < ordered.Length; index++)
        {
            if (index > 0)
            {
                replacement.Append(newline);
                var previousGroup = UsingDirectiveOrderAnalyzer.GetGroup(ordered[index - 1], projectPrefix);
                var currentGroup = UsingDirectiveOrderAnalyzer.GetGroup(ordered[index], projectPrefix);
                if (previousGroup != currentGroup)
                {
                    replacement.Append(newline);
                }
            }

            replacement.Append(indentation);
            replacement.Append(ordered[index].WithoutTrivia().ToFullString());
        }

        if (lastLine.EndIncludingLineBreak > lastLine.End)
        {
            replacement.Append(newline);
        }

        var change = new TextChange(
            TextSpan.FromBounds(firstLine.Start, lastLine.EndIncludingLineBreak),
            replacement.ToString());
        return document.WithText(sourceText.WithChanges(change));
    }
}