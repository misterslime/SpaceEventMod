using Microsoft.Extensions.Primitives;
using System;

namespace SourceGeneration.Utilities;

internal static class PathUtils
{
    public static StringSegment GetFolder(StringSegment path)
    {
        StringSegment result = path;
        int lastSlash = result.LastIndexOf('/');
        if (lastSlash == -1)
        {
            return default;
        }

        return result.Substring(0, lastSlash);
    }

    public static StringSegment GetFileNameWithoutExtension(StringSegment file)
    {
        StringSegment result = file;
        int lastDot = result.LastIndexOf('.');
        if (lastDot != -1)
        {
            result = result.Substring(0, lastDot);
        }

        int lastSlash = result.LastIndexOf('/');
        if (lastSlash != -1)
        {
            result = result.Substring(lastSlash + 1);
        }

        return result;
    }

    /// <summary>
    ///     Returns the extension including the dot '.' of the given file path.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static StringSegment GetExtension(StringSegment file)
    {
        int index = file.LastIndexOf('.');
        if (index == -1)
        {
            return default;
        }

        return file.Substring(index);
    }

    public static StringSegment RemoveExtension(StringSegment file)
    {
        int index = file.AsSpan().LastIndexOf('.');
        if (index == -1)
        {
            return file;
        }

        return file.Substring(0, index);
    }
}