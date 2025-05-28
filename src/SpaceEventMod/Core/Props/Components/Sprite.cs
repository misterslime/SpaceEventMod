using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Props.Components;

public class Sprite : Component
{
    public string SpritePath;
    public Vector2 SpriteDisplacement = Vector2.Zero;
    public Color DrawColor = Color.White;
    public float Scale;
    public float Rotation;

    public Sprite()
    {
        SpriteSystem.Register(this);
    }
}

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

        foreach (var component in components.ToList())
        {
            Texture2D texture = ModContent.Request<Texture2D>(component.SpritePath).Value;
            Vector2 drawPosition = component.prop.GetComponent<Hitbox>().GetCenter() - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            Main.EntitySpriteDraw(texture, drawPosition + component.SpriteDisplacement, texture.Frame(), component.DrawColor, component.prop.GetComponent<Transformation>().Rotation, origin, 1f, SpriteEffects.None);
        }
    }
}
