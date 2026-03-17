using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Space.LevelElements;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace SpaceEventMod.Content.Space;

public class SpaceEventMapLayer : ModMapLayer
{
    public override Position GetDefaultPosition() => BeforeFirstVanillaLayer;

    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        // We can check Main.mapStyle or Main.mapFullscreen to limit drawing to specific map modes.
        // This example doesn't draw on the overlay map, but draws on the minimap and fullscreen map.
        if (Main.mapStyle == 2)
            return;

        var whitePixel = Assets.Assets.Textures.WhitePixel.Value;

        // draw sea
        // help

        // draw asteroids
        Vector2 GetDimensions(int variant)
        {
            Vector2[] dimensions = [
                new Vector2(3, 1),
                new Vector2(3, 2),
                new Vector2(3, 3),
                new Vector2(4, 1.5f),
                new Vector2(4, 2),
                new Vector2(4, 3),
                new Vector2(6, 3),
                new Vector2(9, 4.5f),
                new Vector2(11, 7),
            ];

            return dimensions[variant];
        }

        foreach (var asteroid in Asteroids.List)
        {
            var scale = GetDimensions(asteroid.Variant) * context.MapScale;
            var position = SpaceEvent.SeaToWorldCoordinates(asteroid.Transform.Position) / 16f;

            var color = new Color(40, 35, 47);

            Draw(context, whitePixel, position, color, new SpriteFrame(1, 1, 0, 0), scale, scale, Alignment.TopLeft);
        }

        // draw stars
        foreach (var star in Stars.List)
        {
            var itemTexture = TextureAssets.Item[ItemID.FallenStar].Value;

            var tilePosition = star.GetCenter() / 16f;

            if (context.Draw(itemTexture, tilePosition, Color.White, new SpriteFrame(1, 8, 0, 0), 1f, 1.2f, Alignment.Center).IsMouseOver)
                text = "Star (" + star.Durability / 10f + "%)";
        }
    }

    public bool Draw(MapOverlayDrawContext context, Texture2D texture, Vector2 position, Color color, SpriteFrame frame, Vector2 scaleIfNotSelected, Vector2 scaleIfSelected, Alignment alignment, SpriteEffects spriteEffects = SpriteEffects.None)
    {
        position = (position - context.MapPosition) * context.MapScale + context.MapOffset;
        if (context.ClippingRectangle.HasValue && !context.ClippingRectangle.Value.Contains(position.ToPoint()))
            return false;

        var sourceRectangle = frame.GetSourceRectangle(texture);
        var vector = sourceRectangle.Size() * alignment.OffsetMultiplier;
        var position2 = position;

        var scale = context.DrawScale * scaleIfNotSelected;
        var vector2 = position - vector * scale;

        var mouseSelected = new Rectangle((int)vector2.X, (int)vector2.Y, (int)(sourceRectangle.Width * scale.X), (int)(sourceRectangle.Height * scale.Y)).Contains(Main.MouseScreen.ToPoint());

        if (mouseSelected)
            scale = context.DrawScale * scaleIfSelected;

        Main.spriteBatch.Draw(texture, position2, sourceRectangle, color, 0f, vector, scale, spriteEffects, 0f);
        return mouseSelected;
    }
}