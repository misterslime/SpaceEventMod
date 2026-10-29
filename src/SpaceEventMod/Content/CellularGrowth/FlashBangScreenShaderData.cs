using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.Tweening;
using SpaceEventMod.Content.Miscellaneous.Dusts;
using SpaceEventMod.Content.Space.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space;

internal class FlashBangScreenShaderData : ScreenShaderData
{
    private static RenderTargetLease s_wallTarget;
    private static RenderTargetLease s_everythingTarget;
    private static Filter s_myFilter;
    private static bool s_drawingFlashbang;

    public static bool DrawingFlashbang { get => s_drawingFlashbang; }

    public static float FlashStrength { get; set; }

    public static float FlashRadius { get; set; }


    public static Vector2 FlashPosition { get; set; }

    private static Vector4[] _flashPositions;
    private static int _activeFlashCount;

    public FlashBangScreenShaderData(Asset<Effect> shader, string passName)
            : base(shader, passName)
    {
    }

    [OnLoad]
    private static void Load()
    {
        var shader = Assets.Shaders.CellularGrowth.Flashbang.Asset;

        s_myFilter = new Filter(new FlashBangScreenShaderData(shader, "Pass0"), EffectPriority.VeryHigh);

        Filters.Scene["SpaceEventMod:Flashbang"] = s_myFilter;
        Filters.Scene["SpaceEventMod:Flashbang"].Load();

        _flashPositions = new Vector4[8];

        if (Main.netMode != NetmodeID.Server)
            Main.RunOnMainThread(() =>
            {
                s_wallTarget = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, (w, h, offW, offH) => (offW, offH));
                s_everythingTarget = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, (w, h, offW, offH) => (offW, offH));
            });
    }

    [OnUnload]
    private static void Unload()
    {
        if (Main.netMode != NetmodeID.Server)
            Main.RunOnMainThread(() =>
            {
                s_wallTarget.Dispose();
                s_everythingTarget.Dispose();
            });
    }

    [ModSystemHooks.PostUpdateEverything]
    private static void UpdateShaderParameters()
    {
        if (s_myFilter is null || s_wallTarget is null || s_everythingTarget is null)
            return;

        Main.QueueMainThreadAction(() =>
        {
            s_drawingFlashbang = true;

            using (s_wallTarget.Target.Scope(clearColor: Color.Black))
            {
                using var sbScope = Main.spriteBatch.Scope();

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(Main.instance.wallTarget, (Main.sceneWallPos - Main.screenPosition) * 1f, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                Main.instance.DrawWoF();
                Main.instance.DrawGoreBehind();
                MoonlordDeathDrama.DrawPieces(Main.spriteBatch);
                MoonlordDeathDrama.DrawExplosions(Main.spriteBatch);
                Main.instance.DrawCachedNPCs(Main.instance.DrawCacheNPCsBehindNonSolidTiles, behindTiles: true);
                Main.spriteBatch.End();

                Main.instance.DrawCachedProjs(Main.instance.DrawCacheProjsBehindNPCsAndTiles);
                Main.spriteBatch.Begin();
                Main.instance.DrawNPCs(behindTiles: true);
                Main.spriteBatch.End();
            }

            using (s_everythingTarget.Target.Scope(clearColor: Color.Transparent))
            {
                using var sbScope = Main.spriteBatch.Scope();

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(Main.instance.blackTarget, (Main.sceneTilePos - Main.screenPosition) * 1f, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Main.instance.tileTarget, (Main.sceneTilePos - Main.screenPosition) * 1f, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Main.instance.tile2Target, (Main.sceneTile2Pos - Main.screenPosition) * 1f, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                //Main.spriteBatch.Draw(Main.instance.backWaterTarget, (Main.sceneWaterPos - Main.screenPosition) * 1f, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.instance.TilesRenderer.PostDrawTiles(true, false, false);
                Main.instance.TilesRenderer.PostDrawTiles(false, false, false);
                Main.instance.DrawPlayers_BehindNPCs();
                Main.instance.DrawCachedProjs(Main.instance.DrawCacheProjsBehindNPCs);
                Main.spriteBatch.Begin();
                Main.instance.DrawNPCs();
                Main.spriteBatch.End();
                Main.spriteBatch.Begin();
                Main.instance.DrawCachedNPCs(Main.instance.DrawCacheNPCProjectiles, behindTiles: false);
                Main.spriteBatch.End();
                Main.instance.DrawSuperSpecialProjectiles(Main.instance.DrawCacheFirstFractals);
                Main.instance.DrawCachedProjs(Main.instance.DrawCacheProjsBehindProjectiles);
                Main.instance.DrawProjectiles();
                Main.instance.DrawPlayers_AfterProjectiles();
                Main.instance.DrawCachedProjs(Main.instance.DrawCacheProjsOverPlayers);
                Main.spriteBatch.Begin();
                Main.instance.DrawCachedNPCs(Main.instance.DrawCacheNPCsOverPlayers, behindTiles: false);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin();
                Main.instance.DrawItems();
                Main.spriteBatch.End();
                Main.spriteBatch.Begin();
                Main.instance.DrawRain();
                Main.spriteBatch.End();
                Main.spriteBatch.Begin();
                Main.instance.DrawGore();
                Main.spriteBatch.End();
                Main.instance.DrawDust();
            }

            s_drawingFlashbang = false;
        });

        Filters.Scene["SpaceEventMod:Flashbang"]._shader.UseImage(s_wallTarget.Target, 0, SamplerState.LinearClamp);
        Filters.Scene["SpaceEventMod:Flashbang"]._shader.UseImage(s_everythingTarget.Target, 1, SamplerState.LinearClamp);

        int flashbangDust = ModContent.DustType<FlashbangDust>();

        foreach (var dust in Main.dust)
        {
            if (!dust.active || dust.type != flashbangDust)
                continue;

            if (_activeFlashCount >= _flashPositions.Length)
                break;

            float ratio = 1f - (dust.fadeIn / (13 * 3));
            ratio = MathHelper.Clamp(MathF.Sin(ratio * MathF.PI), 0f, 1f);

            ref var flashPosition = ref _flashPositions[_activeFlashCount];
            ref var flashStrength = ref _flashPositions[_activeFlashCount].Z;
            ref var flashRadius = ref _flashPositions[_activeFlashCount].W;

            var position = (dust.position - Main.screenPosition) / s_wallTarget.Target.Size();

            flashPosition.X = position.X;
            flashPosition.Y = position.Y;
            flashStrength = 1f - (dust.fadeIn / (13 * 3)) + dust.dustIndex;
            flashRadius = EasingFunctions.InOutSine(ratio) * 0.47f;

            _activeFlashCount++;
        }

        if (_activeFlashCount == 0 && Filters.Scene["SpaceEventMod:Flashbang"].IsActive())
            Filters.Scene.Deactivate("SpaceEventMod:Flashbang");

    }

    public override void Apply()
    {
        base.Shader.Parameters["flashColor"]?.SetValue(new Color(51, 159, 255, 255).ToVector4());
        base.Shader.Parameters["shadowColor"]?.SetValue(new Color(114, 164, 25, 255).ToVector4());
        base.Shader.Parameters["flashPosition"]?.SetValue((FlashPosition - Main.screenPosition) / s_wallTarget.Target.Size());
        base.Shader.Parameters["flashStrength"]?.SetValue(FlashStrength);
        base.Shader.Parameters["flashRadius"]?.SetValue(FlashRadius);
        base.Shader.Parameters["screenTargetRatio"]?.SetValue((s_wallTarget.Target.Size() / Main.ScreenSize.ToVector2()));
        base.Shader.Parameters["flashData"]?.SetValue(_flashPositions);
        base.Shader.Parameters["flashCount"]?.SetValue(_activeFlashCount);

        _activeFlashCount = 0;
        base.Apply();
    }
}
