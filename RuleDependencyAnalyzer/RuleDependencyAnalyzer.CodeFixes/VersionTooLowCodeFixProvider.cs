﻿namespace RuleDependencyAnalyzer
{
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = RuleDependencyDiagnosticAnalyzer.VersionTooLowId)]
    public class VersionTooLowCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(RuleDependencyDiagnosticAnalyzer.VersionTooLowId);
            }
        }

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            ImmutableArray<CodeAction>.Builder result = ImmutableArray.CreateBuilder<CodeAction>();
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                // Return a code action that will invoke the fix.
                context.RegisterCodeFix(CodeAction.Create("Update version number", cancellationToken => UpdateVersionAsync(context.Document, diagnostic.Location, cancellationToken)), diagnostic);
            }

            return Task.FromResult(true);
        }

        private async Task<Document> UpdateVersionAsync(Document document, Location location, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // Find the attribute syntax identified by the diagnostic.
            var attributeSyntax = root.FindToken(location.SourceSpan.Start).Parent.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
            if (attributeSyntax == null)
                return document;

            var versionArgumentExpression = attributeSyntax.ArgumentList.Arguments[2].Expression;
            var newRoot = root.ReplaceNode(versionArgumentExpression, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(100)));
            return document.WithSyntaxRoot(newRoot);
        }
    }
}