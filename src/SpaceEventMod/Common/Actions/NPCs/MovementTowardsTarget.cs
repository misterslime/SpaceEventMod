using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.Automata;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct MovementTowardsTarget(bool towards, params float[] arguments) : IState<ModNPC>
{
    private bool towards = towards;
    private float[] arguments = arguments;

    public void Enter(ModNPC modNPC)
    {
    }

    public bool Update(ModNPC modNPC)
    {
        var npc = modNPC.NPC;

        if (modNPC is not IMovement npcMovement || !npc.HasValidTarget)
            return true;

        var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.TranslatedTargetIndex].Center;

        var vectorToTarget = targetCenter - npc.Center;

        npcMovement.EntityMovement(towards ? vectorToTarget : -vectorToTarget, arguments);

        return false;
    }

    public void Exit(ModNPC modNPC)
    {

    }
}

