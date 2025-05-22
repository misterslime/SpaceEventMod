using Microsoft.Extensions.Primitives;
using System;

namespace SourceGeneration.Utilities;

internal static class PathUtils {
    public static StringSegment GetFolder(StringSegment path) {
        var result = path;
        var lastSlash = result.LastIndexOf('/');
        if(lastSlash == -1) return default;
        return result.Substring(0, lastSlash);
    }

    public static StringSegment GetFileNameWithoutExtension(StringSegment file) {
        var result = file;
        var lastDot = result.LastIndexOf('.');
        if(lastDot != -1) result = result.Substring(0, lastDot);
        var lastSlash = result.LastIndexOf('/');
        if(lastSlash != -1) result = result.Substring(lastSlash + 1);
        return result;
    }

    /// <summary>
    ///     Returns the extension including the dot '.' of the given file path.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static StringSegment GetExtension(StringSegment file) {
        var index = file.LastIndexOf('.');
        if(index == -1) return default;
        return file.Substring(index);
    }

    public static StringSegment RemoveExtension(StringSegment file) {
        var index = file.AsSpan().LastIndexOf('.');
        if(index == -1) return file;
        return file.Substring(0, index);
    }
}