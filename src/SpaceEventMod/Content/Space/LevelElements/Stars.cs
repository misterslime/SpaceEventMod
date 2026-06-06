using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Animation.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.LevelElements;

// to-do:
// - add starsap lmao
// - make stars appear on the map upon being found
// - star spawning

public struct Star(Vector2 spawnPosition, Rectangle frame)
{
    private HashSet<int> SubscribedNPCs = [];

    public readonly int Width = 68;
    public readonly int Height = 68;
    public readonly int RandomTimeDisplacement = Main.rand.Next(-99999, 99999);
    public readonly Rectangle Frame = frame;

    public Vector2 Position = spawnPosition;
    public float Rotation = 0;

    public int Durability = 1000;

    public Vector2 ShakeDirection = Vector2.UnitX;
    public int ShakeTime = 0;

    public Vector2 SpriteDisplacement = Vector2.Zero;
    public SpriteEffects Effects = Main.rand.NextBool(2) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

    public readonly Rectangle GetBoundingBox()
    {
        return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
    }

    public readonly Vector2 GetCenter()
    {
        return Position + new Vector2(Width, Height) * 0.5f;
    }

    public void SubscribeNPC(int npcID)
    {
        SubscribedNPCs.Add(npcID);
        UpdateSubscribedNPCs();
    }

    public void UnsubscribeNPC(int npcID)
    {
        SubscribedNPCs.Remove(npcID);
        UpdateSubscribedNPCs();
    }

    public void IsNPCSubscribed(int npcID)
    {
        SubscribedNPCs.Contains(npcID);
    }

    public void UpdateSubscribedNPCs()
    {
        foreach (var npcIndex in SubscribedNPCs.ToList())
        {
            if (!Main.npc[npcIndex].active)
            {
                SubscribedNPCs.Remove(npcIndex);
                continue;
            }

            /*if (Main.npc[npcIndex].ModNPC is not IWantStar wantStar)
                continue;

            wantStar.ObservedStar = this;*/
        }
    }

    public void InformSubscribedNPCs(Action<NPC> action)
    {
        foreach (var npcIndex in SubscribedNPCs.ToList())
        {
            if (Main.npc[npcIndex].active)
                action.Invoke(Main.npc[npcIndex]);
        }
    }
}


public static class Stars
{
    public static List<Star> List = new List<Star>();

    [OnLoad]
    public static void Load()
    {
        On_Main.DrawDust += DrawStars;
        On_Projectile.AI_007_GrapplingHooks += GrappleStars;
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += MineStars;
    }

    [ModSystemHooks.OnWorldUnload]
    public static void UnloadStars()
    {
        List.Clear();
    }

    #region Update Logic
    [ModSystemHooks.PreUpdateNPCs]
    public static void UpdateStars()
    {
        for (var i = 0; i < List.Count; i++)
        {
            var star = List[i];

            // delete the prop if durability is now below 0
            if (star.Durability <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item70, star.Position);
                List.RemoveAt(i);
                i--;
                continue;
            }

            star.SpriteDisplacement = MathF.Sin((Main.GameUpdateCount + star.RandomTimeDisplacement) / 60f) * 10 * Vector2.UnitY;
            star.Rotation = MathF.Sin((Main.GameUpdateCount + star.RandomTimeDisplacement) / 120f) * (MathF.PI / 180f) * 5;

            if (star.ShakeTime > 0)
                star.ShakeTime--;

            List[i] = star;
            List[i].UpdateSubscribedNPCs();
        }
    }
    #endregion

    #region Rendering
    private static void DrawStars(On_Main.orig_DrawDust orig, Main self)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        for (var i = 0; i < Stars.List.Count; i++)
        {
            var star = Stars.List[i];

            var texture = Assets.Textures.Space.LevelElements.Star.Asset.Value;
            var drawPosition = star.GetCenter() - Main.screenPosition;
            var origin = star.Frame.Center.ToVector2();

            var wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
            var lifeRatio = star.Durability / 1000f;
            var drawColor = Color.Lerp(Color.White, Color.Transparent, wave * EasingFunctions.InCirc(1 - lifeRatio));

            var shakeVector = MathF.Sin(Main.GameUpdateCount) * 2f * (star.ShakeTime / 20f) * star.ShakeDirection;

            Main.EntitySpriteDraw(texture, drawPosition + star.SpriteDisplacement + shakeVector, star.Frame, drawColor, star.Rotation, origin, 1f, star.Effects);
        }

        Main.spriteBatch.End();

        orig(self);
    }
    #endregion

    #region Grappling
    private static void GrappleStars(On_Projectile.orig_AI_007_GrapplingHooks orig, Projectile self)
    {
        orig(self);

        if (self.ai[0] == 2)
            return;

        for (var i = 0; i < Stars.List.Count; i++)
        {
            var star = Stars.List[i];

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
    private static void MineStars(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y)
    {
        orig(self, sItem, out canHitWalls, x, y);

        if (self.whoAmI != Main.myPlayer)
            return;

        for (var i = 0; i < Stars.List.Count; i++)
        {
            var hitStar = false;
            var destroyStar = false;

            Stars.List[i] = MineStar(Stars.List[i], self, sItem, x, y, out hitStar, out destroyStar);
            Stars.List[i].UpdateSubscribedNPCs();

            if (hitStar)
            {
                SoundEngine.PlaySound(SoundID.Tink, Main.MouseWorld);
                self.ApplyItemTime(sItem, self.pickSpeed * 1.5f);

                Stars.List[i].InformSubscribedNPCs((npc) =>
                {
                    npc.target = self.whoAmI;

                    npc.targetRect = Main.player[self.whoAmI].getRect();
                });
            }

            if (destroyStar)
            {
                SoundEngine.PlaySound(SoundID.Item70, Stars.List[i].GetCenter());
                Stars.List.RemoveAt(i);
                i--;
            }

            if (hitStar || destroyStar)
            {
                canHitWalls = false;
                return;
            }
        }
    }

    private static Star MineStar(Star star, Player self, Item sItem, int x, int y, out bool hitStar, out bool destroyStar)
    {
        hitStar = false;
        destroyStar = false;

        var newStar = star;

        if (star.GetBoundingBox().Contains((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y))
        {
            newStar.Durability -= sItem.pick;

            // shake when mining
            var starPosition = star.GetCenter();

            newStar.ShakeDirection = starPosition - self.Center;
            newStar.ShakeDirection.Normalize();
            newStar.ShakeTime = 20;

            hitStar = true;

            // delete the prop if durability is now below 0
            if (star.Durability <= 0)
                destroyStar = true;
        }

        return newStar;
    }
    #endregion
}
