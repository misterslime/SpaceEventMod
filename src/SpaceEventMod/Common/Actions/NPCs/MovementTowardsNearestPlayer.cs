using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.Automata;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public class MovementTowardsNearestPlayer(params float[] arguments) : IState<ModNPC>
{
    private float[] arguments = arguments;

    public void Enter(ModNPC context)
    {
    }

    public void Exit(ModNPC context)
    {
    }

    public bool Update(ModNPC context)
    {
        var npc = context.NPC;

        if (context is not IMovement npcMovement)
            throw new Exception("Tried to run MovementTowardsNearestPlayer state code on a non-valid npc type.");

        npc.TargetClosest(false);

        var vectorToPlayer = Main.player[npc.target].Center - npc.Center;

        npcMovement.EntityMovement(vectorToPlayer, arguments);

        return false;
    }
}
