using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Events.FirmamentTide.Stars;

// to-do:
// - add starsap lmao
// - make stars appear on the map upon being found
// - star spawning
public class StarSystem : ModSystem
{
    public static List<Star> Stars = new List<Star>();

    public override void Load()
    {
        On_Main.DrawDust += DrawStars;
        On_Projectile.AI_007_GrapplingHooks += GrappleStars;
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += MineStars;
    }

    public override void Unload()
    {
        On_Main.DrawDust -= DrawStars;
        On_Projectile.AI_007_GrapplingHooks -= GrappleStars;
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool -= MineStars;
    }

    public override void OnWorldUnload()
    {
        Stars.Clear();
    }

    public override void PreUpdateNPCs()
    {
        for (var i = 0; i < Stars.Count; i++)
        {
            var star = Stars[i];

            // delete the prop if durability is now below 0
            if (star.Durability <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item70, star.Position);
                Stars.RemoveAt(i);
                i--;
                continue;
            }

            star.SpriteDisplacement = MathF.Sin((Main.GameUpdateCount + star.RandomTimeDisplacement) / 60f) * 10 * Vector2.UnitY;
            star.Rotation = MathF.Sin((Main.GameUpdateCount + star.RandomTimeDisplacement) / 120f) * (MathF.PI / 180f) * 5;

            if (star.ShakeTime > 0)
                star.ShakeTime--;

            Stars[i] = star;
            Stars[i].UpdateSubscribedNPCs();
        }
    }

    private void DrawStars(On_Main.orig_DrawDust orig, Main self)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        for (var i = 0; i < Stars.Count; i++)
        {
            var star = Stars[i];

            var texture = ModContent.Request<Texture2D>(star.TexturePath).Value;
            var drawPosition = star.GetCenter() - Main.screenPosition;
            var origin = texture.Size() * 0.5f;

            var wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
            var lifeRatio = star.Durability / 1000f;
            var drawColor = Color.Lerp(Color.White, Color.Transparent, wave * EasingFunctions.CircEaseIn(1 - lifeRatio));

            var shakeVector = MathF.Sin(Main.GameUpdateCount) * 2f * (star.ShakeTime / 20f) * star.ShakeDirection;

            Main.EntitySpriteDraw(texture, drawPosition + star.SpriteDisplacement + shakeVector, texture.Frame(), drawColor, star.Rotation, origin, 1f, star.Effects);
        }

        Main.spriteBatch.End();

        orig(self);
    }

    private void MineStars(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y)
    {
        orig(self, sItem, out canHitWalls, x, y);

        if (self.whoAmI != Main.myPlayer)
            return;

        for (var i = 0; i < Stars.Count; i++)
        {
            var star = Stars[i];

            if (star.GetBoundingBox().Contains((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y))
            {
                star.Durability -= sItem.pick;
                self.ApplyItemTime(sItem, self.pickSpeed * 1.5f);

                // shake when mining
                var asteroidPosition = star.GetCenter();

                star.ShakeDirection = asteroidPosition - self.Center;
                star.ShakeDirection.Normalize();
                star.ShakeTime = 20;

                // delete the prop if durability is now below 0
                if (star.Durability <= 0)
                {
                    SoundEngine.PlaySound(SoundID.Item70, asteroidPosition);
                    Stars.RemoveAt(i);
                    i--;
                    continue;
                }

                if (Main.myPlayer == self.whoAmI && star.Durability > 0)
                {
                    SoundEngine.PlaySound(SoundID.Tink, Main.MouseWorld);

                    star.InformSubscribedNPCs((npc) =>
                    {
                        npc.target = self.whoAmI;

                        npc.targetRect = Main.player[self.whoAmI].getRect();
                    });
                }
            }

            Stars[i] = star;
            Stars[i].UpdateSubscribedNPCs();
        }
    }

    private void GrappleStars(On_Projectile.orig_AI_007_GrapplingHooks orig, Projectile self)
    {
        orig(self);

        if (self.ai[0] == 2)
            return;

        for (var i = 0; i < Stars.Count; i++)
        {
            var star = Stars[i];

            var colliderBox = star.GetBoundingBox();

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
