using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Content.Space.LevelElements;
using SpaceEventMod.Core.Animation.Tweening;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace SpaceEventMod.Content.Space;

// https://code.tutsplus.com/make-a-splash-with-dynamic-2d-water-effects--gamedev-236t
// made with this thingy
// to-do:
// - antialiasing on the foam
// - add godrays
// - add small star pixel particles that dont appear in godrays
// - add bubble particles when you move
// - maybe stuff behind the foam could be shaded in the foam? or it could be transparent.
// - make the sea appear on the map
internal class SpaceEvent : ModSystem
{
    public static FirmamentSea Sea;

    public static Vector2 SeaToWorldCoordinates(Vector2 position) => new Vector2(position.X, position.Y + Sea.SeaPos.Height.Position);

    public static Vector2 WorldToSeaCoordinates(Vector2 position) => new Vector2(position.X, position.Y - Sea.SeaPos.Height.Position);

    private FastNoiseLite noise;
    private float minimumToSpawnAsteroid = 0.7f;
    private float separationDistance = 10 * 16;
    private float starSeparationDistance = 120 * 16;

    public override void ClearWorld()
    {
        Sea = new FirmamentSea();

        noise = new FastNoiseLite(Main.ActiveWorldFileData.Seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
    }

    public override void PostUpdatePlayers()
    {
        if (Sea.Springs is null)
            return;

        Sea = Sea.UpdateChunks()
            .UpdateSeaHeight()
            .CollideSprings()
            .PropagateWaves(0.04f)
            .UpdateSprings(0.1f, 0.005f);

        SpawnAsteroids();
        SpawnStars();
    }

    private void SpawnStars()
    {
        if (!Sea.CanSpawnThings || Main.gameMenu || FocusHelper.GameplayActive)
            return;

        var playerCenter = Main.player[Main.myPlayer].Center;
        var randomPosition = playerCenter + Main.rand.NextVector2CircularEdge(75 * 16, 75 * 16);
        // 0.05f

        // only spawn 20 tiles above the sea surface
        if (randomPosition.Y > (float)(Main.worldSurface * 0.35 * 16) - 320 || randomPosition.Y <= 5 * 16)
            return;

        var stars = Stars.List;

        if (stars.Count > 0)
        {
            foreach (var star in stars)
            {
                if ((star.Position - randomPosition).LengthSquared() <= Math.Pow(starSeparationDistance, 2))
                    return;

                if ((star.Position - playerCenter).LengthSquared() <= Math.Pow(starSeparationDistance * 1.35, 2))
                    return;
            }
        }

        if (!Main.rand.NextBool(300))
            return;

        int frameX = Main.rand.Next(0, 3);
        int frameY = Main.rand.Next(0, 2);

        Texture2D texture = Assets.Textures.Space.LevelElements.Star.Asset.Value;
        Rectangle frame = texture.Frame(3, 2, frameX, frameY);

        Stars.List.Add(new LevelElements.Star(randomPosition, frame));

    }

    private void SpawnAsteroids()
    {
        if (!Sea.CanSpawnThings || Main.gameMenu || FocusHelper.GameplayActive)
            return;

        var playerCenter = Main.player[Main.myPlayer].Center;
        var playerPositionSeaCoords = new Vector2(playerCenter.X, playerCenter.Y - (float)(Main.worldSurface * 0.35 * 16));
        var randomPosition = playerPositionSeaCoords + Main.rand.NextVector2CircularEdge(75 * 16, 75 * 16);
        // 0.05f

        // only spawn 20 tiles above the sea surface
        if (randomPosition.Y > -320)
            return;

        var noiseSample = (float)(1 + noise.GetNoise(randomPosition.X * 0.3f, randomPosition.Y * 0.3f, 0));

        var density = MathHelper.Lerp(0.7f, 30f, EasingFunctions.InCirc(noiseSample));

        var asteroids = Asteroids.List;

        if (asteroids.Count > 0)
        {
            foreach (var asteroid in asteroids)
            {
                if ((asteroid.RestPosition - randomPosition).LengthSquared() <= Math.Pow(separationDistance * density, 2))
                    return;
            }
        }

        var asteroidType = Main.rand.Next(9);

        Point GetDimensions(int variant)
        {
            Point[] dimensions = [
                new Point(48, 16),
                new Point(48, 32),
                new Point(48, 48),
                new Point(64, 24),
                new Point(64, 32),
                new Point(64, 48),
                new Point(96, 44),
                new Point(144, 74),
                new Point(176, 110),
            ];

            return dimensions[variant];
        }

        var dimensions = GetDimensions(asteroidType);

        Asteroids.List.Add(new Asteroid(randomPosition, asteroidType, dimensions.X, dimensions.Y));
    }

    [ModPlayerHooks.PostUpdateBuffs]
    public static void PostUpdateBuffs(ModPlayer self)
    {
        Player player = self.Player;

        if (SpaceEvent.Sea.Active && player.Center.Y < SpaceEvent.Sea.SeaPos.Height.Position)
            player.gravity = 0.25f;
    }
}

internal class SpaceEventMapLayer : ModMapLayer
{
    public override Position GetDefaultPosition() => BeforeFirstVanillaLayer;

    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        // We can check Main.mapStyle or Main.mapFullscreen to limit drawing to specific map modes.
        // This example doesn't draw on the overlay map, but draws on the minimap and fullscreen map.
        if (Main.mapStyle == 2)
            return;

        var whitePixel = Assets.Textures.WhitePixel.Asset.Value;

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

internal class SpaceEventFogShaderData : ScreenShaderData
{
    private static Filter _myFilter;

    public SpaceEventFogShaderData(Asset<Effect> shader, string passName)
            : base(shader, passName)
    {
    }

    [OnLoad]
    private static void Load()
    {
        var shader = Assets.Shaders.Space.SeaDistortFog.Asset;

        _myFilter = new Filter(new SpaceEventFogShaderData(shader, "Pass0")
            .UseImage(Assets.Textures.Noise.SwirlyDisplaceNoise.Asset, 0, SamplerState.LinearWrap), EffectPriority.VeryHigh);

        Filters.Scene["SeaDistortFog"] = _myFilter;
        Filters.Scene["SeaDistortFog"].Load();
    }

    [ModSystemHooks.PostUpdateEverything]
    private static void UpdateShaderParameters()
    {
        if (_myFilter is null || SeaBuffers.SeaMeshBuffer is null)
            return;

        Filters.Scene["SeaDistortFog"]._shader.UseImage(SeaBuffers.SeaMeshBuffer.Target, 1, SamplerState.LinearWrap);
    }

    public override void Apply()
    {
        // base.Shader.Parameters["fogColor"]?.SetValue(new Vector4(0.0f, 0.25f, 1.0f, 0.25f));
        base.Shader.Parameters["fogColor"]?.SetValue(new Vector4(0.0f, 0.25f, 1.0f, 0.35f));
        base.Shader.Parameters["fogStart"]?.SetValue(0.15f);
        base.Shader.Parameters["fogEnd"]?.SetValue(0.65f);
        base.Shader.Parameters["distortIntensity"]?.SetValue(0.07f);
        base.Shader.Parameters["distortNoiseScale"]?.SetValue(0.001f);
        base.Shader.Parameters["timeScale"]?.SetValue(0.02f);
        base.Shader.Parameters["blurMulti"]?.SetValue(0.0005f);

        base.Apply();
    }
}
