using System.Text;
using Terraria;

namespace DetourHookGenerator;

internal static class Program
{
    public static void Main()
    {
        var assembly = typeof(On_Main).Assembly;
        int numAttributes = 0;
        var path = Path.Combine("src", "SpaceEventMod", "Core", "Detours");
        var fileName = Path.Combine(path, "Hooks.cs");
        var sb = new StringBuilder();

        IEnumerable<Type> types =
            from type in assembly.GetTypes()
            where type.Name.StartsWith("On_")
            select type;

        Directory.CreateDirectory(path);

        sb.AppendLine($"namespace SpaceEventMod.Core.Detours;");
        sb.AppendLine();
        sb.AppendLine($"public static partial class Hooks");
        sb.AppendLine("{");

        foreach (Type type in types)
        {
            numAttributes += GenerateTypeHooks(sb, type);
        }

        sb.AppendLine("}");

        File.WriteAllText(fileName, sb.ToString().TrimEnd());

        Console.WriteLine($"Generated a total of {numAttributes} attributes in {types.Count()} types.");
    }

    private static int GenerateTypeHooks(StringBuilder sb, Type type)
    {
        var events = type.GetEvents();
        var numAttributes = events.Count();

        Console.WriteLine($"Generating {numAttributes} attributes for {type.Name}.");

        sb.AppendLine($"    public static class {type.Name}");
        sb.AppendLine("    {");

        foreach (var member in events)
        {
            sb.AppendLine($"        [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]");
            sb.AppendLine($"        public class {member.Name}Attribute : Attribute;");
            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        return numAttributes;
    }

    /*public static partial class Hooks
    {
        public static class On_TypeName
        {
            
            [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
            public class MethodNameAttribute : Attribute;
        }
    }*/
}