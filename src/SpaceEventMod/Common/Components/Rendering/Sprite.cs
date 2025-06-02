using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Props;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Components.Rendering;

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
