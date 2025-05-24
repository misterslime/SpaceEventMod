using System;

namespace SourceGeneration.Assets;

/// <summary> Represents an asset file. All parameters are relative to source. </summary>
internal readonly record struct AssetFile(string Path, string Folder, string Name, string Extension, AssetType AssetType)
{
    public string Path { get; } = Path;
    public string Folder { get; } = Folder;
    public string Name { get; } = Name;
    public string Extension { get; } = Extension;
    public AssetType AssetType { get; } = AssetType;
    
    public bool Equals(AssetFile other) 
        => Path.Equals(other.Path, StringComparison.OrdinalIgnoreCase) && Extension.Equals(other.Extension, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() 
        => HashCode.Combine(Path, Extension);
}