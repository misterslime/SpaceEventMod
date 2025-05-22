using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SourceGeneration.Utilities;
using System.Text;

namespace SourceGeneration.Generators;

[Generator(LanguageNames.CSharp)]
public class GlobalUsingsGenerator : IIncrementalGenerator
{
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.AnalyzerConfigOptionsProvider, (x, options) =>
            {
                if (GeneratorUtils.GetRootNamespaceOrRaiseDiagnostic(x, options.GlobalOptions) is not
                    { } rootNamespace)
                {
                    return;
                }

                x.AddSource(
                    "_Usings.g.cs",
                    SourceText.From(GenerateUsings(rootNamespace), Encoding.UTF8)
                );
            }
        );

        string GenerateUsings(string rootNamespace)
        {
            StringBuilder sb = new();
            sb.AppendLine($"global using static {rootNamespace}.Assets.Assets;");
            return sb.ToString();
        }
    }
}