using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.BaseTypes;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ModLoader;
using TileHelper.Common;

namespace SpaceEventMod.Content.Miscellaneous.Walls;

internal class HydratedJellyCell : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    protected override int Variants => 3;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Green);
    }
}

internal class HydratedJellyCell2 : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    protected override int Variants => 3;
    protected override float Depth => 0.05f;

    public override string Texture => "SpaceEventMod/Assets/Textures/Miscellaneous/Walls/HydratedJellyCell";

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Green);
    }
}

internal class HerbCellPrototype : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Green);
    }
}

internal class HerbCellPrototype2 : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    protected override float Depth => 0.1f;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Green);
    }
}


internal class CosmostoneWalls3 : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    protected override int Variants => 3;
    protected override float Depth => 0.4f;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Green);
    }
}

internal class CosmostoneWalls2 : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    protected override int Variants => 3;
    protected override float Depth => 0.2f;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Blue);
    }

}

internal class CosmostoneWalls1 : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    protected override int Variants => 3;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Red);
    }

}

internal class RedWall : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Red);
    }

}

internal class DualGridWallTest : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    protected override int Variants => 7;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Black);
    }
}

internal class CellWoodWall : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    protected override int Variants => 3;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Green);
    }
}

internal class AsterdustWall1 : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    protected override int Variants => 3;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Red);
    }

}
internal class AsterdustWall2 : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    protected override int Variants => 3;

    protected override float Depth => 0.2f;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Red);
    }

}

internal class AsterdustWall3 : WangWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    protected override int Variants => 3;

    protected override float Depth => 0.7f;

    public override void SetWallDefaults()
    {
        AddMapEntry(Color.Red);
    }

    protected override bool CustomSpriteBatchParameters([NotNullWhen(true)] out SpriteBatchParameters? parameters)
    {
        Effect? effect = null;

        if (Program.IsMainThread)
        {
            var overlay = Assets.Textures.Miscellaneous.Walls.AsterdustWall3Overlay.Asset.Value;

            var shaderPass = Assets.Shaders.CellularGrowth.AsterdustBackWallOverlay.CreatePass1();
            shaderPass.Parameters.OverlayTexture = overlay;
            shaderPass.Parameters.OutlineColor = new Color(25, 20, 66).ToVector4();
            shaderPass.Parameters.Parallax = 0.94f;
            shaderPass.Parameters.PixelSize = Vector2.One / overlay.Size();
            shaderPass.Parameters.ScreenPosition = Main.screenPosition;
            shaderPass.Apply();

            effect = shaderPass.Shader;
        }

        parameters = new(sortMode: SpriteSortMode.FrontToBack,samplerState: SamplerState.PointClamp, blendState: BlendState.Opaque, customEffect: effect);

        return true;
    }

}