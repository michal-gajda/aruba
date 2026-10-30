namespace Aruba.CodeStyle.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class UsingDirectiveOrderCodeFixTests
{
    [TestMethod]
    public async Task AddsClassKeywordToBareRecord()
    {
        const string source = "public record Item;\n";
        const string expected = "public record class Item;\n";
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("Test", LanguageNames.CSharp)
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.Preview));
        var document = project.AddDocument("Test.cs", SourceText.From(source));
        var root = await document.GetSyntaxRootAsync();
        var record = root!.DescendantNodes().OfType<RecordDeclarationSyntax>().Single();
        var descriptor = new DiagnosticDescriptor(
            UsingDirectiveOrderAnalyzer.ExplicitRecordTypeDiagnosticId,
            "title",
            "message",
            "Style",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        var diagnostic = Diagnostic.Create(descriptor, record.Keyword.GetLocation());
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None);

        await new UsingDirectiveOrderCodeFixProvider().RegisterCodeFixesAsync(context);

        Assert.AreEqual(1, actions.Count);
        var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
        var changedSolution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        var changedDocument = changedSolution.GetDocument(document.Id)!;
        var changedText = await changedDocument.GetTextAsync();
        Assert.AreEqual(expected, changedText.ToString().Replace("\r\n", "\n", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task SortsSystemProjectAndExternalGroups()
    {
        const string source = "namespace Aruba.WebApi;\nusing Zeta;\nusing Aruba.Application;\nusing System;\nclass C {}\n";
        const string expected = "namespace Aruba.WebApi;\nusing System;\n\nusing Aruba.Application;\n\nusing Zeta;\nclass C {}\n";
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("Test", LanguageNames.CSharp)
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.Preview));
        var document = project.AddDocument("Test.cs", SourceText.From(source));
        var root = await document.GetSyntaxRootAsync();
        var firstUsing = root!.DescendantNodes().OfType<UsingDirectiveSyntax>().First();
        var descriptor = new DiagnosticDescriptor("ARUBA001", "title", "message", "Style", DiagnosticSeverity.Warning, isEnabledByDefault: true);
        var diagnostic = Diagnostic.Create(descriptor, firstUsing.GetLocation());
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None);

        await new UsingDirectiveOrderCodeFixProvider().RegisterCodeFixesAsync(context);

        Assert.AreEqual(1, actions.Count);
        var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
        var changedSolution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        var changedDocument = changedSolution.GetDocument(document.Id)!;
        var changedText = await changedDocument.GetTextAsync();
        Assert.AreEqual(expected, changedText.ToString().Replace("\r\n", "\n", StringComparison.Ordinal));
    }
}