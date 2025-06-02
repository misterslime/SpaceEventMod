using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Props;
using Terraria.ModLoader;
using Terraria;

namespace SpaceEventMod.Common.Components.Rendering;

public class SpriteSystem : ComponentSystem<Sprite>
{
    public override void Load()
    {
        On_Main.DrawNPCs += DrawEverything;
    }

    public override void Unload()
    {
        On_Main.DrawNPCs -= DrawEverything;
    }

    private void DrawEverything(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        orig(self, behindTiles);

        foreach (var component in components)
        {
            var texture = ModContent.Request<Texture2D>(component.SpritePath).Value;
            var drawPosition = component.GetComponent<Hitbox>().GetCenter() - Main.screenPosition;
            var origin = texture.Size() * 0.5f;

            Main.EntitySpriteDraw(texture, drawPosition + component.SpriteDisplacement, texture.Frame(), component.DrawColor, component.Rotation, origin, 1f, component.Effects);
        }
    }
}
