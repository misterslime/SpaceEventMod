using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.Automata;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct TargetedSquidMovement(float jumpDistance, float gravity, int cooldown, bool towards = true) : IState<ModNPC>
{
    private float jumpDistance = jumpDistance;
    private float gravity = gravity;
    private int cooldown = cooldown;
    private bool towards = towards;

    public void Enter(IAutomata<ModNPC> stateMachine)
    {
    }

    public bool Update(IAutomata<ModNPC> stateMachine)
    {
        var npc = stateMachine.Context.NPC;

        if (stateMachine.Context is not IDynamicMotion dynamicMotion || stateMachine.Context is not ITimer timer || !npc.HasValidTarget)
            return true;

        var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.TranslatedTargetIndex].Center;

        if (timer.Time > 0)
        {
            timer.Time--;

            if (dynamicMotion.TargetPosition.Distance(npc.Center) <= 16)
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

        var vectorToTarget = targetCenter - npc.Center;
        vectorToTarget.Normalize();

        dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + (towards ? vectorToTarget * jumpDistance : -vectorToTarget * jumpDistance);
        timer.Time = cooldown;
        npc.netUpdate = true;

        return false;
    }

    public void Exit(IAutomata<ModNPC> stateMachine)
    {

    }
}

