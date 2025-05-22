using Microsoft.Extensions.Primitives;
using System;
using System.Runtime.CompilerServices;

namespace SourceGeneration.Assets;

/// <summary> Represents an asset file. All parameters are relative to source. </summary>
internal readonly record struct AssetFile(
    StringSegment Path,
    StringSegment Folder,
    StringSegment Name,
    StringSegment Extension,
    AssetType AssetType)
{
    public bool Equals(AssetFile other)
    {
        return Path.Equals(other.Path, StringComparison.OrdinalIgnoreCase)
               && Extension.Equals(other.Extension, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Path, Extension);
    }

    public StringSegment Path { get; } = Path;
    public StringSegment Folder { get; } = Folder;
    public StringSegment Name { get; } = Name;
    public StringSegment Extension { get; } = Extension;
    public AssetType AssetType { get; } = AssetType;
}