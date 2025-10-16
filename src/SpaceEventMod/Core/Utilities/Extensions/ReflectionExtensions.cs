using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Utilities.Extensions;

internal static class ReflectionExtensions
{
    public static readonly BindingFlags AllInstanceBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    public static T GetFieldValue<T>(this object obj, string name)
    {
        var field = obj.GetType().GetField(name, AllInstanceBindings);
        return (T)field?.GetValue(obj);
    }

    public static T GetFieldValue<T>(this object obj, FieldInfo field)
    {
        return (T)field?.GetValue(obj);
    }

    public static object GetFieldValue(this object obj, string name)
    {
        var field = obj.GetType().GetField(name, AllInstanceBindings);
        return field?.GetValue(obj);
    }

    public static object GetFieldValue(this object obj, FieldInfo field)
    {
        return field?.GetValue(obj);
    }

    public static void SetFieldValue(this object obj, string name, object value)
    {
        var field = obj.GetType().GetField(name, AllInstanceBindings);
        field?.SetValue(obj, value);
    }

    public static void SetFieldValue(this object obj, FieldInfo field, object value)
    {
        field?.SetValue(obj, value);
    }

    public static ReadOnlySpan<FieldInfo> GetFields(this object obj)
    {
        return obj.GetType().GetFields(AllInstanceBindings);
    }
}
