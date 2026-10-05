using Microsoft.Xna.Framework;
using SpaceEventMod.Common.BaseTypes;
using TileHelper.Common;

namespace SpaceEventMod.Content.Miscellaneous.Walls;

internal class CosmostoneWalls3 : WangWall, ILoadItem
{
    protected override int Variants => 3;
    protected override float Depth => 0.4f;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Green);
    }
}

internal class CosmostoneWalls2 : WangWall, ILoadItem
{
    protected override int Variants => 3;
    protected override float Depth => 0.2f;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Blue);
    }

}

internal class CosmostoneWalls1 : WangWall, ILoadItem
{
    protected override int Variants => 3;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Red);
    }

}

internal class RedWall : WangWall, ILoadItem
{
    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Red);
    }

}

internal class DualGridWallTest : WangWall, ILoadItem
{
    protected override int Variants => 7;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Black);
    }
}
