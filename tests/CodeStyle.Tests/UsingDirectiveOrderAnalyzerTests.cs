namespace Aruba.CodeStyle.Tests;

using Aruba.CodeStyle;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class UsingDirectiveOrderAnalyzerTests
{
    [TestMethod]
    public async Task BareRecordReportsErrorButExplicitRecordKindsAreAllowed()
    {
        const string source = "record Implicit; record class ExplicitClass; record struct ExplicitStruct;";
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create("Test", [syntaxTree]);
        var analyzer = new UsingDirectiveOrderAnalyzer();
        var diagnostics = await compilation
            .WithAnalyzers([analyzer])
            .GetAnalyzerDiagnosticsAsync();

        var recordDiagnostic = diagnostics.Single(diagnostic =>
            diagnostic.Id == UsingDirectiveOrderAnalyzer.ExplicitRecordTypeDiagnosticId);

        Assert.AreEqual(DiagnosticSeverity.Error, recordDiagnostic.Severity);
        Assert.AreEqual(
            "record",
            source.Substring(recordDiagnostic.Location.SourceSpan.Start, recordDiagnostic.Location.SourceSpan.Length));
        Assert.AreEqual(1, diagnostics.Count(diagnostic =>
            diagnostic.Id == UsingDirectiveOrderAnalyzer.ExplicitRecordTypeDiagnosticId));
    }
}
