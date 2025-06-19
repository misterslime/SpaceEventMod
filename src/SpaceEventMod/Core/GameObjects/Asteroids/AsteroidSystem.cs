using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.GameObjects.Asteroids;

public class AsteroidSystem : ModSystem
{
    public static List<Asteroid> Asteroids = new List<Asteroid>();

    public override void Load()
    {
        On_Main.DrawNPCs += DrawAsteroids;
        On_Collision.SlopeCollision += CheckSlopeCollision;
        On_Projectile.AI_007_GrapplingHooks += GrappleAsteroids;
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += MineAsteroid;
    }

    public override void Unload()
    {
        On_Main.DrawNPCs -= DrawAsteroids;
        On_Collision.SlopeCollision -= CheckSlopeCollision;
        On_Projectile.AI_007_GrapplingHooks -= GrappleAsteroids;
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool -= MineAsteroid;
    }

    public override void OnWorldUnload()
    {
        Asteroids.Clear();
    }

    public override void PostUpdateNPCs()
    {
        for (var i = 0; i < Asteroids.Count; i++)
        {
            var asteroid = Asteroids[i];

            var shouldDespawn = (asteroid.Position - Main.LocalPlayer.Center).LengthSquared() > 60f * 16f * 60f * 16f;

            if (shouldDespawn)
            {
                Asteroids.RemoveAt(i);
                i--;
                continue;
            }

            if (!asteroid.BeingStoodOn)
                asteroid.SpriteDisplacement = MathF.Sin((Main.GameUpdateCount + asteroid.RandomTimeDisplacement) / 60f) * 4 * Vector2.UnitY;
            else
                asteroid.SpriteDisplacement = Vector2.Zero;

            asteroid.Position = asteroid.SecondOrderSolver.Update(1, asteroid.BeingStoodOn ? asteroid.RestPosition + Vector2.UnitY * 48f : asteroid.RestPosition);
            asteroid.BeingStoodOn = false;

            if (asteroid.ShakeTime > 0)
                asteroid.ShakeTime--;

            Asteroids[i] = asteroid;
        }
    }

    private void DrawAsteroids(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        orig(self, behindTiles);

        for (var i = 0; i < Asteroids.Count; i++)
        {
            var asteroid = Asteroids[i];

            Texture2D GetTexture(int variant)
            {
                Texture2D[] textures = [
                    Assets.Assets.Textures.Props.Asteroid3Small.Value,
                    Assets.Assets.Textures.Props.Asteroid3Medium.Value,
                    Assets.Assets.Textures.Props.Asteroid3Large.Value,
                    Assets.Assets.Textures.Props.Asteroid4Small.Value,
                    Assets.Assets.Textures.Props.Asteroid4Medium.Value,
                    Assets.Assets.Textures.Props.Asteroid4Large.Value,
                ];

                return textures[variant];
            }
            ;

            var texture = GetTexture(asteroid.Variant);

            var drawPosition = asteroid.GetCenter() - Main.screenPosition;
            var origin = texture.Size() * 0.5f;

            var wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
            var lifeRatio = asteroid.Durability / 200f;
            var drawColor = Color.Lerp(Color.White, Color.Red, wave * EasingFunctions.CircEaseIn(1 - lifeRatio));

            var shakeVector = MathF.Sin(Main.GameUpdateCount) * 2f * (asteroid.ShakeTime / 20f) * asteroid.ShakeDirection;

            Main.EntitySpriteDraw(texture, drawPosition + asteroid.SpriteDisplacement + shakeVector, texture.Frame(), drawColor, 0f, origin, 1f, asteroid.Effects);
        }
    }

    private void MineAsteroid(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y)
    {
        orig(self, sItem, out canHitWalls, x, y);

        if (self.whoAmI != Main.myPlayer)
            return;

        for (var i = 0; i < Asteroids.Count; i++)
        {
            var asteroid = Asteroids[i];

            if (asteroid.GetBoundingBox().Contains((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y))
            {
                asteroid.Durability -= sItem.pick;
                self.ApplyItemTime(sItem, self.pickSpeed * 1.5f);

                // shake when mining
                var asteroidPosition = asteroid.GetCenter();

                asteroid.ShakeDirection = asteroidPosition - self.Center;
                asteroid.ShakeDirection.Normalize();
                asteroid.ShakeTime = 20;

                // delete the prop if durability is now below 0
                if (asteroid.Durability <= 0)
                {
                    SoundEngine.PlaySound(SoundID.Item70, asteroidPosition);
                    Asteroids.RemoveAt(i);
                    i--;
                    return;
                }

                if (Main.myPlayer == self.whoAmI && asteroid.Durability > 0)
                    SoundEngine.PlaySound(SoundID.Tink, Main.MouseWorld);
            }

            Asteroids[i] = asteroid;
        }
    }

    private Vector4 CheckSlopeCollision(On_Collision.orig_SlopeCollision orig, Vector2 position, Vector2 velocity, int width, int height, float gravity, bool fall)
    {
        var result = new Vector4(position.X, position.Y, velocity.X, velocity.Y);

        if (!fall)
            result = CheckCollision(position, velocity, width, height, gravity);

        return orig(result.XY(), result.ZW(), width, height, gravity, fall);
    }

    public Vector4 CheckCollision(Vector2 position, Vector2 velocity, int width, int height, float gravity)
    {
        var originalVector = new Vector4(position.X, position.Y, velocity.X, velocity.Y);

        // make the entity's hitbox only be its bottom half
        var entityHitbox = new Rectangle((int)position.X, (int)position.Y, width, height + 2);

        for (var i = 0; i < Asteroids.Count; i++)
        {
            var asteroid = Asteroids[i];

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

            Asteroids[i] = asteroid;
        }

        return new Vector4(position.X, position.Y, velocity.X, velocity.Y);
    }

    private void GrappleAsteroids(On_Projectile.orig_AI_007_GrapplingHooks orig, Projectile self)
    {
        orig(self);

        if (self.ai[0] == 2)
            return;

        for (var i = 0; i < Asteroids.Count; i++)
        {
            var asteroid = Asteroids[i];

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
    private void SetGrapple(Vector2 position, Projectile grapple)
    {
        //grapple.tileCollide = true;
        grapple.ai[0] = 2;
        Main.player[grapple.owner].grappling[Main.player[grapple.owner].grapCount] = grapple.whoAmI;
        Main.player[grapple.owner].grapCount++;
        grapple.velocity = Vector2.Zero;
        grapple.netUpdate = true;
        //Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, grapple.Center);
    }
}
