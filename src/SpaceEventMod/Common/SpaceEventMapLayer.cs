using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Events.Space;
using SpaceEventMod.Content.Events.Space.LevelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Common;

public class SpaceEventMapLayer : ModMapLayer
{
    public override Position GetDefaultPosition() => BeforeFirstVanillaLayer;

    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        // We can check Main.mapStyle or Main.mapFullscreen to limit drawing to specific map modes.
        // This example doesn't draw on the overlay map, but draws on the minimap and fullscreen map.
        if (Main.mapStyle == 2)
            return;

        ModifiedMapDrawContext newContext = new ModifiedMapDrawContext(context);
        Texture2D whitePixel = Assets.Assets.Textures.WhitePixel.Value;

        // draw sea
        // help

        // draw asteroids
        Vector2 GetDimensions(int variant)
        {
            Vector2[] dimensions = [
                new Vector2(3, 1),
                new Vector2(3, 2),
                new Vector2(3, 3),
                new Vector2(4, 1),
                new Vector2(4, 2),
                new Vector2(4, 3),
            ];

            return dimensions[variant];
        }

        foreach (Asteroid asteroid in Asteroids.List)
        {
            Vector2 scale = GetDimensions(asteroid.Variant) * context.MapScale;
            Vector2 position = SpaceEvent.SeaToWorldCoordinates(asteroid.Transform.Position) / 16f;

            Color color = new Color(40, 35, 47);

            newContext.Draw(whitePixel, position, color, new SpriteFrame(1, 1, 0, 0), scale, scale, Alignment.TopLeft);
        }

        // draw stars
        foreach (Content.Events.Space.LevelElements.Star star in Stars.List)
        {
            Texture2D itemTexture = TextureAssets.Item[ItemID.FallenStar].Value;

            Vector2 tilePosition = star.GetCenter() / 16f;

            if (context.Draw(itemTexture, tilePosition, Color.White, new SpriteFrame(1, 8, 0, 0), 1f, 1.2f, Alignment.Center).IsMouseOver)
                text = "Star (" + (star.Durability / 10f) + "%)";
        }
    }
}