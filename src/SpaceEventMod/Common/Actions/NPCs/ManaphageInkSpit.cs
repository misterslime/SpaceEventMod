using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Content.NPCs;
using SpaceEventMod.Core.Behavior.Automata;
using SpaceEventMod.Core.Physics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct ManaphageInkSpit : IState<ModNPC>
{
    public static readonly Vector2Dynamics SuddenJerk = new Vector2Dynamics(1f / 120f, 0.7f, 0.8f);

    public void Enter(ModNPC context)
    {
        if (context is not Manaphage manaphage)
            throw new Exception("Tried to run ManaphageInkSpit state code on a non-valid npc type.");

        manaphage.TargetPosition = context.NPC.Center;
        manaphage.Time = 120;
    }

    public void Exit(ModNPC context)
    {
    }

    public bool Update(ModNPC context)
    {
        var npc = context.NPC;

        if (context is not Manaphage manaphage)
            throw new Exception("Tried to run ManaphageInkSpit state code on a non-valid npc type.");

        if (manaphage.Time <= 0)
            return true;

        if (manaphage.Time >= 80)
        {
            var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.TranslatedTargetIndex].Center;
            var desiredRotation = (targetCenter - npc.Center).ToRotation() - MathHelper.PiOver2;
            npc.rotation = npc.rotation.AngleLerp(desiredRotation, 0.1f);

            manaphage.TargetStretching = new Vector2(1.2f, 0.8f);
            manaphage.PositionKinematics = Manaphage.PositionSolver.Update(1, manaphage.PositionKinematics, manaphage.TargetPosition);

            if (manaphage.Time == 80)
            {
                var toTarget = npc.Center - targetCenter;
                toTarget.Normalize();
                manaphage.TargetPosition += toTarget * 16 * 7;
            }
        } else
        {
            npc.rotation = npc.rotation.AngleLerp(npc.velocity.X / (6 * MathF.PI), 0.4f);

            manaphage.PositionKinematics = SuddenJerk.Update(1, manaphage.PositionKinematics, manaphage.TargetPosition);

            if (manaphage.Time == 79 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.TranslatedTargetIndex].Center;
                var toTarget = targetCenter - npc.Center;

                for (int i = 0; i < 3; i++)
                {
                    var inkVelocity = toTarget + Main.rand.NextVector2Circular(16f * 4, 16f * 4);
                    inkVelocity.Normalize();
                    inkVelocity *= 12f + Main.rand.NextFloat(-2, 2);

                    var ink = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center.X, npc.Center.Y, inkVelocity.X, inkVelocity.Y, ModContent.ProjectileType<InkSpit>(), 80, 1f, Main.myPlayer, 0, 0, 0);

                    if (Main.projectile.IndexInRange(ink))
                        Main.projectile[ink].netUpdate = true;
                }

                toTarget.Normalize();
                toTarget *= 12f;

                var finalInk = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center.X, npc.Center.Y, toTarget.X, toTarget.Y, ModContent.ProjectileType<InkSpit>(), 80, 1f, Main.myPlayer, 0, 0, 0);

                if (Main.projectile.IndexInRange(finalInk))
                    Main.projectile[finalInk].netUpdate = true;
            }

            if (manaphage.Time > 70)
                manaphage.TargetStretching = new Vector2(0.90f, 1.1f);
            else
                manaphage.TargetStretching = Vector2.One;
        }

        manaphage.Time--;

        return false;
    }
}
