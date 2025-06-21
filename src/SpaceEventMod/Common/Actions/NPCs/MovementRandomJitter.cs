using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Content.NPCs;
using SpaceEventMod.Core.Behavior.Automata;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct MovementRandomJitter(params float[] arguments) : IState<ModNPC>
{
    private float[] arguments = arguments;

    public void Enter(IAutomata<ModNPC> stateMachine)
    {
    }

    public bool Update(IAutomata<ModNPC> stateMachine)
    {
        var npc = stateMachine.Context;

        if (npc is not IMovement npcMovement)
            return true;

        npcMovement.EntityMovement(Main.rand.NextVector2Unit(), arguments);

        return false;
    }

    public void Exit(IAutomata<ModNPC> stateMachine)
    {

    }
}
