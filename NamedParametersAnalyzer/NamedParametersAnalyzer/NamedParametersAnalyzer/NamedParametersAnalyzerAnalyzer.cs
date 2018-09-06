using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NamedParametersAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NamedParametersAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        private const int ParamsThreshold = 4;
        public const string DiagnosticId = "NamedParametersAnalyzer";

        private static readonly string Title =
            $"Method calls with {ParamsThreshold} or more parameters should be named";

        public static readonly string MessageFormat =
            $"Method calls with {ParamsThreshold} or more parameters have param names";

        private static readonly string Description = "Check that calls with many parameters has their names";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: Title,
            messageFormat: MessageFormat,
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression, SyntaxKind.ObjectCreationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext nodeContext)
        {
            var argumentsList = GetArgumentsList(nodeContext.Node);
            if (argumentsList == null || argumentsList.Arguments.Count < ParamsThreshold)
                return;

            var argumentsWithoutNamecolon = argumentsList.Arguments.Where(x => x.NameColon == null);

            if (!argumentsWithoutNamecolon.Any())
                return;

            var diagnostic = Diagnostic.Create(
                descriptor: Rule,
                location: nodeContext.Node.GetLocation());

            nodeContext.ReportDiagnostic(diagnostic);
        }

        private ArgumentListSyntax GetArgumentsList(SyntaxNode node)
        {
            switch (node)
            {
                case InvocationExpressionSyntax invocation: return invocation.ArgumentList;
                case ObjectCreationExpressionSyntax creation: return creation.ArgumentList;
                default: return null;
            }
        }
    }
}

