using Microsoft.Xna.Framework;
using SpaceEventMod.Content.NPCs.Manaphages;
using SpaceEventMod.Core.Behavior.Automata;
using SpaceEventMod.Core.Physics.Animation;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct ManaphageInkSpit : IState<ModNPC>
{
    public static readonly SecondOrderDynamics SuddenJerk = new SecondOrderDynamics(1f / 120f, 0.7f, 0.8f);

    public void Enter(ModNPC context)
    {
        if (context is not Manaphage manaphage)
            throw new Exception("Tried to run ManaphageInkSpit state code on a non-valid npc type.");

        manaphage.TargetPosition = context.NPC.Center;
        manaphage.Time = 150;
    }

    public void Exit(ModNPC context)
    {
        if (context is not Manaphage manaphage)
            throw new Exception("Tried to run ManaphageInkSpit state code on a non-valid npc type.");

        manaphage.TargetStretching = new Vector2(1f, 1f);
    }

    public bool Update(ModNPC context)
    {
        var npc = context.NPC;

        if (context is not Manaphage manaphage)
            throw new Exception("Tried to run ManaphageInkSpit state code on a non-valid npc type.");

        if (manaphage.Time <= 0 || !npc.HasValidTarget)
            return true;

        var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.TranslatedTargetIndex].Center;

        var toTarget = targetCenter - npc.Center;

        var speed = 10f;

        var gravity = 0.1f;

        var theta = new Vector2(toTarget.X, -toTarget.Y).GetArtilleryAngle(speed, -gravity);

        if (theta is null)
            return true;

        if (manaphage.Time >= 110)
        {
            npc.rotation = npc.rotation.AngleLerp(theta.Value - MathHelper.PiOver2, 0.1f);

            manaphage.TargetStretching = new Vector2(1.2f, 0.8f);
            manaphage.PositionKinematics = Manaphage.PositionSolver.Update(1, manaphage.PositionKinematics, manaphage.TargetPosition);
        }
        else
        {
            npc.rotation = npc.rotation.AngleLerp(-npc.velocity.X / (3 * MathF.PI), 0.35f);

            manaphage.PositionKinematics = SuddenJerk.Update(1, manaphage.PositionKinematics, manaphage.TargetPosition);

            if (manaphage.Time == 109 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                var projectileVelocity = new Vector2((float)Math.Cos(theta.Value), -(float)Math.Sin(theta.Value));
                projectileVelocity.Normalize();
                manaphage.TargetPosition -= projectileVelocity * 16 * 7;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    projectileVelocity *= speed;
                    var finalInk = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center.X, npc.Center.Y, projectileVelocity.X, projectileVelocity.Y, ModContent.ProjectileType<InkSpit>(), 20, 1f, Main.myPlayer, 0, gravity, 0);

                    if (Main.projectile.IndexInRange(finalInk))
                        Main.projectile[finalInk].netUpdate = true;
                }
            }

            if (manaphage.Time > 100)
                manaphage.TargetStretching = new Vector2(0.90f, 1.1f);
            else
                manaphage.TargetStretching = Vector2.One;
        }

        manaphage.Time--;

        return false;
    }
}
