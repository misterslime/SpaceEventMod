using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Content.NPCs;
using SpaceEventMod.Core.Behavior.Automata;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct RandomSquidMovement(float jumpDistance, float gravity, int cooldown) : IState<ModNPC>
{
    private float jumpDistance = jumpDistance;
    private float gravity = gravity;
    private int cooldown = cooldown;

    public void Enter(IAutomata<ModNPC> stateMachine)
    {
    }

    public bool Update(IAutomata<ModNPC> stateMachine)
    {
        var npc = stateMachine.Context;

        if (stateMachine.Context is not IDynamicMotion dynamicMotion || stateMachine.Context is not ITimer timer || stateMachine.Context is not Manaphage manaphage)
            return true;

        if (timer.Time > 0)
        {
            timer.Time--;

            if (dynamicMotion.TargetPosition.Distance(stateMachine.Context.NPC.Center) <= 16)
                dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + new Vector2(0, gravity);

            if (stateMachine.Context is IDynamicStretch squidAnimationp)
            {
                if (timer.Time < 15)
                    squidAnimationp.TargetStretching = new Vector2(1.1f, 0.75f);
                else if (timer.Time >= cooldown - 5)
                    squidAnimationp.TargetStretching = new Vector2(0.8f, 1.25f);
                else
                    squidAnimationp.TargetStretching = Vector2.One;
            }

            return false;
        }

        dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + Main.rand.NextVector2Unit() * jumpDistance;
        timer.Time = cooldown;
        stateMachine.Context.NPC.netUpdate = true;

        return false;
    }

    public void Exit(IAutomata<ModNPC> stateMachine)
    {

    }
}
