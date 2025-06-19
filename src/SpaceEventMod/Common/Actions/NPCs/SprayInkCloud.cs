using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Content.NPCs;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Behavior.Automata;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct SprayInkCloud : IState<ModNPC>
{
    public void Enter(IAutomata<ModNPC> stateMachine)
    {
        var npc = stateMachine.Context.NPC;

        if (stateMachine.Context is not ITimer timer || stateMachine.Context is not ISquidInk squidInk || stateMachine.Context is not IDynamicMotion dynamicMotion || !npc.HasValidTarget)
            return;

        var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.TranslatedTargetIndex].Center;

        squidInk.CloudPosition = targetCenter;
        squidInk.IsSpraying = true;
        //squidInk.Mana--;

        npc.target = -1;
        timer.Time = 120;
        dynamicMotion.TargetPosition = npc.Center;
    }

    public bool Update(IAutomata<ModNPC> stateMachine)
    {
        var npc = stateMachine.Context.NPC;

        if (stateMachine.Context is not ITimer timer || stateMachine.Context is not ISquidInk squidInk)
            return true;

        if (timer.Time <= 0)
            squidInk.IsSpraying = false;

        if (timer.Time > 0 && squidInk.IsSpraying)
        {
            var desiredRotation = (squidInk.CloudPosition - npc.Center).ToRotation() - MathHelper.PiOver2;
            npc.rotation = desiredRotation.AngleLerp(0f, EasingFunctions.SineEaseInOut(Math.Clamp((timer.Time - 100f) / 20f, 0f, 1f)));

            timer.Time--;

            if (timer.Time > 100)
                return false;

            var rotate = MathHelper.ToRadians(Main.rand.NextFloat(-3, 0));

            var mist = Dust.NewDustPerfect(npc.Center + (npc.rotation + MathHelper.PiOver2).ToRotationVector2() * 29, ModContent.DustType<ManaInk>(), Main.rand.NextVector2Circular(1, 1));
            mist.noGravity = true;
            mist.color = new Color(9, 17, 51);
            mist.fadeIn = 120;
            mist.scale = 1.1f;
            mist.customData = new ManaInkData(Main.rand.Next(3), InkType.Spraying, 120, rotate, squidInk.CloudPosition);

            var sparkle = Dust.NewDustPerfect(npc.Center + (npc.rotation + MathHelper.PiOver2).ToRotationVector2() * 29, ModContent.DustType<InkStar>(), Main.rand.NextVector2Circular(3, 3));
            sparkle.noGravity = true;
            sparkle.color = new Color(89, 97, 255);
            sparkle.fadeIn = 20;
            sparkle.scale = 1f;
            sparkle.customData = new InkStarData(InkType.Spraying, squidInk.CloudPosition, Color.Lerp(Color.Yellow, Color.Purple, Main.rand.NextFloat()));

            if (timer.Time == 70)
            {
                var cloud = Projectile.NewProjectile(npc.GetSource_FromAI(), squidInk.CloudPosition.X, squidInk.CloudPosition.Y, 0, 0, ModContent.ProjectileType<ManaCloud>(), 80, 0f, Main.myPlayer, 0, 0, 0);

                if (Main.projectile.IndexInRange(cloud))
                    Main.projectile[cloud].netUpdate = true;
            }

            return false;
        }

        return true;
    }

    public void Exit(IAutomata<ModNPC> stateMachine)
    {

    }
}
