using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SpaceEventMod.Core.SourceGeneration;


[Generator]
public class EnumGenerator : IIncrementalGenerator
{
    public const string Attribute = @"
namespace NetEscapades.EnumGenerators
{
    [System.AttributeUsage(System.AttributeTargets.Enum)]
    public class EnumExtensionsAttribute : System.Attribute
    {
    }
}";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute to the compilation
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "EnumExtensionsAttribute.g.cs",
            SourceText.From(Attribute, Encoding.UTF8)));

        // TODO: implement the remainder of the source generator
    }
}
/*public class HookAttributeGenerator : IIncrementalGenerator
{
    private const string SOURCEGEN_NAMESPACE = "SpaceEventMod.Core.SourceGeneration";

    Assembly GetTerrariaHooks()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName.Contains("TerrariaHooks"))
                return assembly;
        }

        throw new NotImplementedException();

        return null;
    }

    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        throw new NotImplementedException();

        var assembly = GetTerrariaHooks();
        int numAttributes = 0;
        var sb = new StringBuilder();

        if (assembly is null)
            return;

        IEnumerable<Type> types =
            from type in assembly.GetTypes()
            where type.Name.StartsWith("On_")
            select type;

        sb.AppendLine($"namespace SpaceEventMod.Core.Detours;");
        sb.AppendLine();
        sb.AppendLine($"public static partial class Hooks");
        sb.AppendLine("{");
        sb.AppendLine($"    public static class On");
        sb.AppendLine("    {");

        foreach (Type type in types)
        {
            numAttributes += GenerateTypeHooks(sb, type);
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");


        context.RegisterPostInitializationOutput(ctx => {
            ctx.AddSource("Hooks.g.cs", sb.ToString().TrimEnd());
        });
    }

    private static int GenerateTypeHooks(StringBuilder sb, Type type)
    {
        var events = type.GetEvents(BindingFlags.Public | BindingFlags.Static);
        var numAttributes = events.Count();

        sb.AppendLine($"        public static class {type.Name.Substring(3)}");
        sb.AppendLine("        {");

        foreach (var member in events)
        {
            sb.AppendLine($"            [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]");
            sb.AppendLine($"            public class {member.Name}Attribute : Attribute;");
            sb.AppendLine();
        }

        sb.AppendLine("        }");
        sb.AppendLine();

        return numAttributes;
    }
}*/
