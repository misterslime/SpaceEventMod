using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Content.NPCs.Manaphages;
using SpaceEventMod.Core.Behavior.Automata;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct ManaphageSprayInkCloud : IState<ModNPC>
{
    public void Enter(ModNPC modNPC)
    {
        if (!modNPC.NPC.HasValidTarget)
            return;

        if (modNPC is not Manaphage manaphage)
            throw new Exception("Tried to run ManaphageSprayInkCloud state code on a non-valid npc type.");

        var targetCenter = modNPC.NPC.HasNPCTarget ? Main.npc[modNPC.NPC.TranslatedTargetIndex].Center : Main.player[modNPC.NPC.TranslatedTargetIndex].Center;

        manaphage.CloudPosition = targetCenter;
        manaphage.IsSpraying = true;
        //manaphage.Mana--;

        modNPC.NPC.target = -1;
        manaphage.TargetPosition = modNPC.NPC.Center;
        manaphage.Time = 120;
    }

    public bool Update(ModNPC modNPC)
    {
        var npc = modNPC.NPC;

        if (npc.ModNPC is not Manaphage manaphage)
            throw new Exception("Tried to run SprayInkCloud state code on a non-valid npc type.");

        manaphage.PositionKinematics = Manaphage.PositionSolver.Update(1, manaphage.PositionKinematics, manaphage.TargetPosition);
        manaphage.TargetStretching = Vector2.One;

        if (manaphage.Time > 0)
        {
            var desiredRotation = (manaphage.CloudPosition - npc.Center).ToRotation() - MathHelper.PiOver2;
            npc.rotation = npc.rotation.AngleLerp(desiredRotation, 0.95f);

            manaphage.Time--;

            if (manaphage.Time > 100)
                return false;

            var rotate = MathHelper.ToRadians(Main.rand.NextFloat(-3, 0));

            for (var i = 0; i < 2; i++)
            {
                var mist = Dust.NewDustPerfect(npc.Center + (npc.rotation + MathHelper.PiOver2).ToRotationVector2() * 29, ModContent.DustType<ManaInk>(), Main.rand.NextVector2Circular(1, 1));
                mist.noGravity = true;
                mist.color = new Color(9, 17, 51);
                mist.fadeIn = 120;
                mist.scale = 0.2f;
                mist.customData = new ManaInkData(Main.rand.Next(3), InkType.Spraying, 120, rotate, manaphage.CloudPosition);
            }

            var sparkle = Dust.NewDustPerfect(npc.Center + (npc.rotation + MathHelper.PiOver2).ToRotationVector2() * 29, ModContent.DustType<InkStar>(), Main.rand.NextVector2Circular(3, 3));
            sparkle.noGravity = true;
            sparkle.color = new Color(89, 97, 255);
            sparkle.fadeIn = 20;
            sparkle.scale = 1f;
            sparkle.customData = new InkStarData(InkType.Spraying, manaphage.CloudPosition, Color.Lerp(Color.Yellow, Color.Purple, Main.rand.NextFloat()));

            if (manaphage.Time == 70 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                var cloud = Projectile.NewProjectile(npc.GetSource_FromAI(), manaphage.CloudPosition.X, manaphage.CloudPosition.Y, 0, 0, ModContent.ProjectileType<ManaCloud>(), 80, 0f, Main.myPlayer, 0, 0, 0);

                if (Main.projectile.IndexInRange(cloud))
                    Main.projectile[cloud].netUpdate = true;
            }

            return false;
        }

        return true;
    }

    public void Exit(ModNPC modNPC)
    {

    }
}
