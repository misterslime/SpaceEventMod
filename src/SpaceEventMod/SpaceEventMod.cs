using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content.Sources;
using SpaceEventMod.Core.Sources;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod;

public class SpaceEventMod : Mod
{
    internal static SpaceEventMod Instance { get; private set; }

    internal static Texture2D WhitePixel;

    internal static Texture2D TransparentPixel;

    public override void Load()
    {
        Instance = this;

        // code to generate white and empty pixels bc i can lmao :fire:
        Main.QueueMainThreadAction(() =>
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            WhitePixel = TransparentPixel = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
            WhitePixel.SetData<Color>([Color.White]);
            TransparentPixel.SetData<Color>([Color.Transparent]);
        });
    }

    public override void Unload()
    {
        Instance = null;

        Main.QueueMainThreadAction(() =>
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            WhitePixel.Dispose();
            TransparentPixel.Dispose();
            WhitePixel = TransparentPixel = null;
        });
    }

    public override IContentSource CreateDefaultContentSource()
    {
        var source = new RedirectContentSource(base.CreateDefaultContentSource());

        source.AddRedirect("Content", "Assets/Textures");
        return source;
    }
}