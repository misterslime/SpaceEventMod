using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Props;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Components;

/// <summary>
/// Adds a sprite to the prop to be drawn.<br/>
/// Requires the <see cref="Transformation"/> and <see cref="Hitbox"/> components to function.
/// </summary>
/// <param name="spritePath">The path to the sprite file.</param>
/// <param name="scale">Size the sprite is drawn at.</param>
/// <param name="rotation">How the sprite is rotated.</param>
/// <param name="spriteDisplacement">Sprite's displacement from where its normally drawn.</param>
/// <param name="drawColor">Color of the sprite.</param>
/// <param name="spriteEffects">Sprite effects. Defaults to <see cref="SpriteEffects.None"/>.</param>
public class Sprite(string spritePath, float scale, float rotation, Vector2 spriteDisplacement, Color drawColor, SpriteEffects spriteEffects = SpriteEffects.None) : Component
{
    public string SpritePath = spritePath;
    public Vector2 SpriteDisplacement = spriteDisplacement;
    public Color DrawColor = drawColor;
    public float Scale = scale;
    public float Rotation = rotation;
    public SpriteEffects Effects = spriteEffects;
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

        foreach (var component in components)
        {
            var texture = ModContent.Request<Texture2D>(component.SpritePath).Value;
            var drawPosition = component.GetComponent<Hitbox>().GetCenter() - Main.screenPosition;
            var origin = texture.Size() * 0.5f;

            Main.EntitySpriteDraw(texture, drawPosition + component.SpriteDisplacement, texture.Frame(), component.DrawColor, component.Rotation, origin, 1f, component.Effects);
        }
    }
}
