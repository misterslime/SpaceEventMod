using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Primitives;
using SourceGeneration.Utilities;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace SourceGeneration.Assets;

// ReSharper disable VariableHidesOuterVariable
[Generator(LanguageNames.CSharp)]
internal sealed class AssetGenerator : IIncrementalGenerator
{
    private const string tool_version = "1.0";
    private const string image_extension = ".png";
    private const string effect_extension = ".fxc";

    private static readonly string[] supported_extensions = new[] { image_extension, effect_extension };

    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<string> modName =
            context.CompilationProvider.Select((compilation, _) => compilation.AssemblyName);

        IncrementalValueProvider<string> assetRootFolder = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith("AssetRoot.txt"))
            .Collect()
            .Select(static (files, _) =>
            {
                string directory = Path.GetDirectoryName(files.FirstOrDefault()?.Path)?.Replace('\\', '/');
                if (string.IsNullOrWhiteSpace(directory))
                {
                    return null;
                }

                return directory;
            });

        IncrementalValueProvider<GeneratorInput> generatorInput = assetRootFolder
            .Combine(modName)
            .Select(
                static (tuple, _) =>
                    new GeneratorInput(tuple.Left, tuple.Right)
            );

        /*
            asset files are grouped by directory
            one generated file per asset is technically the most efficient, but itd generate way too many files
            one generated file with all assets would generate a very large file, which the compiler might not like
            and changing one fill would necessitate an entire file rebuild

            grouping by directory only triggers a rebuild for the directory a file belongs to
         */
        var contents = context.AdditionalTextsProvider
            .Where(static file => supported_extensions.Any(ext =>
                file.Path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)
            ))
            .Select((file, _) => file.Path.Replace('\\', '/'))
            .Combine(generatorInput)
            .Where(tuple =>
                tuple.Right.AssetRootFolder != null && tuple.Left.StartsWith(
                    tuple.Right.AssetRootFolder.Value.ToString(),
                    StringComparison.Ordinal
                )
            )
            .Select(static (tuple, _) =>
            {
                FileInformation fileInfo = new(
                    tuple.Left,
                    tuple.Right.AssetRootFolder!.Value,
                    tuple.Right.AssemblyName
                );

                StringSegment fullPath = fileInfo.FullPath.AsSegment(fileInfo.RootFolder.Length + 1);

                StringSegment path = PathUtils.RemoveExtension(fullPath);
                StringSegment folder = PathUtils.GetFolder(fullPath);
                StringSegment name = PathUtils.GetFileNameWithoutExtension(fullPath);
                StringSegment extension = PathUtils.GetExtension(fullPath);

                if (folder.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                {
                    folder = folder.Substring("Assets/".Length);
                }

                //determine asset type based on file extension
                //exception should never be thrown in any case, but defensive anyways
                AssetType assetType = extension.Equals(image_extension, StringComparison.OrdinalIgnoreCase)
                    ? AssetType.Texture2D
                    : extension.Equals(effect_extension, StringComparison.OrdinalIgnoreCase)
                        ? AssetType.Effect
                        : throw new InvalidOperationException("how");

                return new
                {
                    AssetFile = new AssetFile(
                        path,
                        folder,
                        name,
                        extension,
                        assetType
                    ),
                    fileInfo.AssemblyName
                };
            })
            .Collect()
            .SelectMany(
                (files, _) =>
                    files
                        .GroupBy(
                            f => f.AssetFile.Folder,
                            f => f,
                            (key, group) =>
                                (key, group.ToImmutableArray())
                        )
                        .ToImmutableArray()
            );

        context.RegisterSourceOutput(
            assetRootFolder.Combine(modName),
            (context, tuple) =>
            {
                var (path, modName) = tuple;
                string warn = "";
                if (path == null)
                {
                    warn = "#warning missing AssetRoot.txt file";
                }

                context.AddSource(
                    "Assets.default.g.cs",
                    $@"// <auto-generated/>
namespace {modName}.Assets;
[System.CodeDom.Compiler.GeneratedCodeAttribute(""{typeof(AssetGenerator).FullName}"", ""{tool_version}"")]
partial class Assets;
{warn}
"
                );
            }
        );

        context.RegisterSourceOutput(
            contents.Combine(modName),
            (sourceContext, tuple) =>
            {
                var (contentTuple, tupleModName) = tuple;
                var (folder, assetFiles) = contentTuple;

                sourceContext.CancellationToken.ThrowIfCancellationRequested();
                using IndentedStringWriter writer = new(1024);

                writer.WriteLine("// <auto-generated/>");

                writer.WriteLine($@"
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using System;

using ImageAsset = ReLogic.Content.Asset<Microsoft.Xna.Framework.Graphics.Texture2D>;
using EffectAsset = ReLogic.Content.Asset<Microsoft.Xna.Framework.Graphics.Effect>;

namespace {tupleModName}.Assets;

partial class Assets {{"
                );
                writer.Indent++;
                foreach (StringSegment part in folder.SplitEx('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    writer.WriteLine($"public partial class {part} {{   ");
                    writer.Indent++;
                }

                foreach (var fileData in assetFiles)
                {
                    AssetFile file = fileData.AssetFile;
                    string assetPath = $"{tupleModName}/{file.Path}";

                    writer.WriteLine($"public const string KEY_{file.Name} = \"{assetPath}\";");

                    string typeLazy = file.AssetType switch
                    {
                        AssetType.Texture2D =>
                            $"public readonly static Lazy<ImageAsset> {file.Name}_lazy = new(() => ModContent.Request<Texture2D>(\"{assetPath}\"));",
                        AssetType.Effect =>
                            $"public readonly static Lazy<EffectAsset> {file.Name}_lazy = new(() => ModContent.Request<Effect>(\"{assetPath}\", AssetRequestMode.ImmediateLoad));",
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    string type = file.AssetType switch
                    {
                        AssetType.Texture2D =>
                            $"public static ImageAsset {file.Name} {{ get; }} = {file.Name}_lazy.Value;",
                        AssetType.Effect =>
                            $"public static EffectAsset {file.Name} {{ get; }} = ModContent.Request<Effect>(\"{assetPath}\", AssetRequestMode.ImmediateLoad);",
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    writer.WriteLine(typeLazy);
                    writer.WriteLine(type);
                }

                foreach (StringSegment _ in folder.SplitEx('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    writer.Indent--;
                    writer.WriteLine("}");
                }

                writer.Indent--;
                writer.WriteLine("}"); // Assets class

                string sourceText = writer.ToStringAndClear();

                writer.Write($"Assets.{folder}.cs");
                writer.Builder.Replace('/', '.');
                string fileName = writer.ToString();
                if (fileName.Equals("Assets..cs", StringComparison.Ordinal)) // file was on root
                {
                    fileName = "Assets.g.cs";
                }

                sourceContext.AddSource(fileName, sourceText);
            }
        );
    }

    /// <summary> Represents the input data required for this generator, including the root assets folder and assembly name. </summary>
    /// <param name="AssemblyName">The name of the mod used for generation, derived from the compilation's assembly name.</param>
    private readonly record struct GeneratorInput(StringSegment? AssetRootFolder, StringSegment AssemblyName)
    {
        public StringSegment? AssetRootFolder { get; } = AssetRootFolder;

        /// <summary>The name of the mod used for generation, derived from the compilation's assembly name.</summary>
        public StringSegment AssemblyName { get; } = AssemblyName;
    }

    /// <summary> Represents information about a file. </summary>
    private readonly record struct FileInformation(
        string FullPath,
        StringSegment RootFolder,
        StringSegment AssemblyName)
    {
        public string FullPath { get; } = FullPath;
        public StringSegment RootFolder { get; } = RootFolder;
        public StringSegment AssemblyName { get; } = AssemblyName;
    }
}