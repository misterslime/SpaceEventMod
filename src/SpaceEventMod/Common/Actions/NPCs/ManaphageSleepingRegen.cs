using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Content.NPCs.Manaphages;
using SpaceEventMod.Core.Behavior.Automata;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct ManaphageSleepingRegen : IState<ModNPC>
{
    public void Enter(ModNPC context)
    {
        if (context.NPC.HasValidTarget)
            return;

        if (context is not Manaphage manaphage)
            throw new Exception("Tried to run SprayInkCloud state code on a non-valid npc type.");

        manaphage.TargetPosition = context.NPC.Center;
        manaphage.Time = 0;
    }

    public bool Update(ModNPC context)
    {
        if (context.NPC.HasValidTarget || context.NPC.life >= context.NPC.lifeMax)
            return true;

        if (context is not Manaphage manaphage)
            throw new Exception("Tried to run SprayInkCloud state code on a non-valid npc type.");

        context.NPC.rotation = context.NPC.rotation.AngleLerp(0f, 0.95f);

        if (context.NPC.velocity.LengthSquared() <= 1f)
        {
            if (manaphage.Time >= 40)
            {
                var sleep = Dust.NewDustPerfect(context.NPC.Center, ModContent.DustType<Sleep>(), Vector2.Zero);
                sleep.noGravity = true;
                sleep.color = Color.White;
                sleep.fadeIn = 120;
                sleep.scale = 1f;
                sleep.customData = new SleepData(context.NPC.whoAmI);
                sleep.velocity = new Vector2(context.NPC.direction, -0.8f);

                //context.NPC.HealEffect(5, true);
                context.NPC.life = Math.Clamp(context.NPC.life + 5, 0, context.NPC.lifeMax);
                context.NPC.netUpdate = true;

                manaphage.Time = 0;
            }

            manaphage.Time++;
        }

        return false;
    }

    public void Exit(ModNPC context)
    {
    }
}
