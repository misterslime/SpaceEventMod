using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content.Sources;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TileHelper.Common;

namespace SpaceEventMod;

internal class SpaceEventMod : Mod
{
    internal static SpaceEventMod Instance { get; private set; }

    internal static BasicEffect basicEffect;

    public override void Load()
    {
        Instance = this;

        TileHelper.Autoloader.Load(this);

        Main.QueueMainThreadAction(() =>
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            basicEffect = new BasicEffect(Main.graphics.graphicsDevice);
            basicEffect.VertexColorEnabled = true;

            basicEffect.Projection = Matrix.CreateOrthographicOffCenter
                (0, Main.graphics.graphicsDevice.Viewport.Width,
                Main.graphics.graphicsDevice.Viewport.Height, 0,
                0, 1);
            basicEffect.World = Matrix.Identity;
            basicEffect.View = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward,
                Vector3.Up);
        });
    }

    public override void Unload()
    {
        Instance = null;

        Main.QueueMainThreadAction(() =>
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            if (basicEffect != null)
                basicEffect.Dispose();
        });
    }

    public override IContentSource CreateDefaultContentSource()
    {
        var source = new RedirectContentSource(base.CreateDefaultContentSource());

        source.AddRedirect("Content", "Assets/Textures");
        return source;
    }
}

public static class AutoItemExtensions
{
    //credit to SpiritR for these
    public const string SUFFIX = "Item";

    public static ModItem AutoModItem<T>(this T t, string prepend = "") where T : ModType => SpaceEventMod.Instance.Find<ModItem>(t.Name + prepend + SUFFIX);

    public static int AutoItemType<T>(this T t, string prepend = "") where T : ModType => AutoModItem(t, prepend).Type;
}