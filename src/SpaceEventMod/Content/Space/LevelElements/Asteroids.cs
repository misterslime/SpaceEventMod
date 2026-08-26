using Daybreak.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Animation.SecondOrderDynamics;
using SpaceEventMod.Core.Animation.Tweening;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Components.Animation;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace SpaceEventMod.Content.Space.LevelElements;

// to-do:
// - cracks to indicate low health
// - netcoding
// - make auto item hotkey select a pickaxe when hovering over an asteroid or star
// - make smart cursor select asteroids and stars
// - fix the bug where grapple hooks dont move with the asteroid or star
// - make sure stars and asteroids dont spawn inside tiles
// - make asteroids appear on the map
public struct Asteroid(Vector2 initialPosition, int variant, int width, int height)
{
    public PhysicsPoint Transform = new PhysicsPoint(initialPosition);
    public int Variant = variant;
    public int Width = width;
    public int Height = height;

    public int Durability = 200;

    public Vector2 RestPosition = initialPosition;
    public bool BeingStoodOn = false;

    public Vector2 ShakeDirection = Vector2.UnitX;
    public int ShakeTime = 0;

    public Vector2 SpriteDisplacement = Vector2.Zero;
    public SpriteEffects Effects = Main.rand.NextBool(2) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

    public int RandomTimeDisplacement = Main.rand.Next(-99999, 99999);

    public Rectangle GetBoundingBox()
    {
        var worldCoords = SpaceEvent.SeaToWorldCoordinates(Transform.Position);

        return new Rectangle((int)worldCoords.X + (int)SpriteDisplacement.Y, (int)worldCoords.Y + (int)SpriteDisplacement.Y, Width, Height);
    }

    public Vector2 GetCenter()
    {
        return SpaceEvent.SeaToWorldCoordinates(GetTrueCenter());
    }

    public Vector2 GetTrueCenter()
    {
        return Transform.Position + new Vector2(Width, Height) * 0.5f;
    }
}

public static class Asteroids
{
    public static List<Asteroid> List = new List<Asteroid>();

    internal static readonly SecondOrderAnimation AsteroidMovement = new SecondOrderAnimation(1f / 64f, 0.5f, 0.2f);

    [OnLoad]
    public static void Load()
    {
        On_Collision.SlopeCollision += CheckSlopeCollision;
        On_Main.DrawDust += DrawAsteroids;
        On_Projectile.AI_007_GrapplingHooks += GrappleAsteroids;
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += MineAsteroid;
    }

    [ModSystemHooks.OnWorldUnload]
    public static void UnloadAsteroids()
    {
        List.Clear();
    }

    #region Updating and Despawning
    [ModSystemHooks.PreUpdateNPCs]
    public static void UpdateAsteroids()
    {
        for (var i = 0; i < List.Count; i++)
        {
            var shouldDespawn = false;

            List[i] = UpdateAsteroid(List[i], out shouldDespawn);

            if (shouldDespawn)
            {
                List.RemoveAt(i);
                i--;
            }
        }
    }

    private static Asteroid UpdateAsteroid(Asteroid asteroid, out bool shouldDespawn)
    {
        var newAsteroid = asteroid;

        shouldDespawn = (asteroid.GetCenter() - Main.LocalPlayer.Center).LengthSquared() > 100f * 16f * 100f * 16f;

        newAsteroid.SpriteDisplacement = MathF.Sin((Main.GameUpdateCount + asteroid.RandomTimeDisplacement) / 60f) * 4 * Vector2.UnitY;

        Vector2 target = asteroid.BeingStoodOn ? asteroid.RestPosition + Vector2.UnitY * 24f : asteroid.RestPosition;

        PhysicsObject physicsObject = new PhysicsObject(newAsteroid.Transform);
        physicsObject.AddComponent(new SecondOrderData(1, AsteroidMovement, target));

        SecondOrderDynamics.Solver.RunPhysicsPasses([physicsObject]);

        newAsteroid.Transform = physicsObject.Center;

        //newAsteroid.Transform = AsteroidMovement.Update(1, asteroid.Transform, );
        newAsteroid.BeingStoodOn = false;

        if (asteroid.ShakeTime > 0)
            newAsteroid.ShakeTime--;

        return newAsteroid;
    }
    #endregion

    #region Collision
    private static Vector4 CheckSlopeCollision(On_Collision.orig_SlopeCollision orig, Vector2 position, Vector2 velocity, int width, int height, float gravity, bool fall, bool ignoreAetheriumPlatforms)
    {
        var result = new Vector4(position.X, position.Y, velocity.X, velocity.Y);

        if (!fall && !SpaceEvent.Sea.Despawning)
            result = CheckCollision(position, velocity, width, height, gravity);

        return orig(result.XY(), result.ZW(), width, height, gravity, fall, ignoreAetheriumPlatforms);
    }

    private static Vector4 CheckCollision(Vector2 position, Vector2 velocity, int width, int height, float gravity)
    {
        var originalVector = new Vector4(position.X, position.Y, velocity.X, velocity.Y);

        // make the entity's hitbox only be its bottom half
        var entityHitbox = new Rectangle((int)position.X, (int)position.Y, width, height + 2);

        for (var i = 0; i < Asteroids.List.Count; i++)
        {
            var asteroid = Asteroids.List[i];

            var colliderBox = asteroid.GetBoundingBox();

            var propCenter = asteroid.GetCenter();
            var canHit = Collision.CanHit(position, 1, 1, propCenter, 1, 1);

            if (!entityHitbox.Intersects(colliderBox) || velocity.Y < 0 || !(position.X + width > colliderBox.Left && position.X < colliderBox.Right) || !canHit)
                continue;

            if (position.Y + height * 0.5f <= colliderBox.Y)
            {
                if (velocity.Y > 0)
                    asteroid.BeingStoodOn = true;

                position.Y = MathHelper.Lerp(position.Y, colliderBox.Y - height + 2, 0.66f);
                velocity.Y = 0;
            }

            Collision.up = true;
            Collision.stair = true;

            Asteroids.List[i] = asteroid;
        }

        return new Vector4(position.X, position.Y, velocity.X, velocity.Y);
    }
    #endregion

    #region Drawing
    private static void DrawAsteroids(On_Main.orig_DrawDust orig, Main self)
    {
        orig(self);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        for (var i = 0; i < Asteroids.List.Count; i++)
        {
            var asteroid = Asteroids.List[i];

            var texture = GetVariantTexture(asteroid.Variant);

            var drawPosition = asteroid.GetCenter() - Main.screenPosition;
            var origin = texture.Size() * 0.5f;

            var wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
            var lifeRatio = asteroid.Durability / 200f;
            var drawColor = Color.Lerp(Color.White, Color.Red, wave * EasingFunctions.InCirc(1 - lifeRatio));

            var shakeVector = MathF.Sin(Main.GameUpdateCount) * 2f * (asteroid.ShakeTime / 20f) * asteroid.ShakeDirection;

            Main.EntitySpriteDraw(texture, drawPosition + asteroid.SpriteDisplacement + shakeVector, texture.Frame(), drawColor, 0f, origin, 1f, asteroid.Effects);
        }

        Main.spriteBatch.End();
    }

    private static Texture2D GetVariantTexture(int variant)
    {
        Texture2D[] textures = [
            Assets.Textures.Space.LevelElements.Asteroid3Small.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid3Medium.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid3Large.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid4Small.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid4Medium.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid4Large.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid6.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid9.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid11.Asset.Value
        ];

        return textures[variant];
    }
    #endregion

    #region Grappling
    private static void GrappleAsteroids(On_Projectile.orig_AI_007_GrapplingHooks orig, Projectile self)
    {
        orig(self);

        if (self.ai[0] == 2)
            return;

        for (var i = 0; i < Asteroids.List.Count; i++)
        {
            var asteroid = Asteroids.List[i];

            var colliderBox = asteroid.GetBoundingBox();

            if (!(self.position.X + self.width > colliderBox.Left && self.position.X < colliderBox.Right) || !self.Hitbox.Intersects(colliderBox))
                continue;

            SetGrapple(self.position, self);
            return;
        }
    }

    /// <summary>
    /// Makes a grappling hook think it's grappled onto an object.
    /// This function was written by @Impaxim on discord. Thank you Impaxim!
    /// </summary>
    /// <param name="position">The position you want the grappling hook to grapple to.</param>
    /// <param name="grapple">The grappling hook projectile.</param>
    private static void SetGrapple(Vector2 position, Projectile grapple)
    {
        //grapple.tileCollide = true;
        grapple.ai[0] = 2;
        Main.player[grapple.owner].grappling[Main.player[grapple.owner].grapCount] = grapple.whoAmI;
        Main.player[grapple.owner].grapCount++;
        grapple.velocity = Vector2.Zero;
        grapple.netUpdate = true;
        //Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, grapple.Center);
    }
    #endregion

    #region Mining
    private static void MineAsteroid(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y)
    {
        if (self.whoAmI == Main.myPlayer)
        {
            for (var i = 0; i < Asteroids.List.Count; i++)
            {
                var hitAsteroid = false;
                var destroyAsteroid = false;

                Asteroids.List[i] = MineAsteroid(Asteroids.List[i], self, sItem, x, y, out hitAsteroid, out destroyAsteroid);

                if (hitAsteroid)
                {
                    SoundEngine.PlaySound(SoundID.Tink, Main.MouseWorld);
                    self.ApplyItemTime(sItem, self.pickSpeed * 1.5f);
                }

                if (destroyAsteroid)
                {
                    SoundEngine.PlaySound(SoundID.Item70, Asteroids.List[i].GetCenter());
                    Asteroids.List.RemoveAt(i);
                    i--;
                }

                if (hitAsteroid || destroyAsteroid)
                {
                    canHitWalls = false;
                    return;
                }
            }
        }

        orig(self, sItem, out canHitWalls, x, y);
    }

    private static Asteroid MineAsteroid(Asteroid asteroid, Player self, Item sItem, int x, int y, out bool hitAsteroid, out bool destroyAsteroid)
    {
        hitAsteroid = false;
        destroyAsteroid = false;

        var newAsteroid = asteroid;

        if (asteroid.GetBoundingBox().Contains((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y))
        {
            newAsteroid.Durability -= sItem.pick;

            // shake when mining
            var asteroidPosition = asteroid.GetCenter();

            newAsteroid.ShakeDirection = asteroidPosition - self.Center;
            newAsteroid.ShakeDirection.Normalize();
            newAsteroid.ShakeTime = 20;

            hitAsteroid = true;

            // delete the prop if durability is now below 0
            if (asteroid.Durability <= 0)
                destroyAsteroid = true;
        }

        return newAsteroid;
    }
    #endregion

    #region Map Layer
    internal class AsteroidMapLayer : ModMapLayer
    {
        public override Position GetDefaultPosition() => BeforeFirstVanillaLayer;

        public override void Draw(ref MapOverlayDrawContext context, ref string text)
        {
            // We can check Main.mapStyle or Main.mapFullscreen to limit drawing to specific map modes.
            // This example doesn't draw on the overlay map, but draws on the minimap and fullscreen map.
            if (Main.mapStyle == 2)
                return;

            var whitePixel = Assets.Textures.WhitePixel.Asset.Value;

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

                DrawAsteroid(context, whitePixel, position, color, new SpriteFrame(1, 1, 0, 0), scale, scale, Alignment.TopLeft);
            }
        }

        private bool DrawAsteroid(MapOverlayDrawContext context, Texture2D texture, Vector2 position, Color color, SpriteFrame frame, Vector2 scaleIfNotSelected, Vector2 scaleIfSelected, Alignment alignment, SpriteEffects spriteEffects = SpriteEffects.None)
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
    #endregion
}