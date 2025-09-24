using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content.Sources;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod;

internal class SpaceEventMod : Mod
{
    internal static SpaceEventMod Instance { get; private set; }

    internal static PrimitiveBatch PrimitiveBatch;

    public override void Load()
    {
        Instance = this;

        // code to generate white and empty pixels bc i can lmao :fire:
        Main.QueueMainThreadAction(() =>
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            PrimitiveBatch = new PrimitiveBatch(Main.graphics.GraphicsDevice);
        });
    }

    public override void Unload()
    {
        Instance = null;

        Main.QueueMainThreadAction(() =>
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            PrimitiveBatch.Dispose();
            PrimitiveBatch = null;
        });
    }

    public override IContentSource CreateDefaultContentSource()
    {
        var source = new RedirectContentSource(base.CreateDefaultContentSource());

        source.AddRedirect("Content", "Assets/Textures");
        return source;
    }
}