using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.Automata;
using SpaceEventMod.Core.GameObjects.Alerts;
using SpaceEventMod.Core.GameObjects.Stars;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct SquidGoToStar(float jumpDistance, float gravity, int cooldown, float range) : IState<ModNPC>
{
    private float jumpDistance = jumpDistance;
    private float gravity = gravity;
    private int cooldown = cooldown;
    private float range = range;

    public void Enter(IAutomata<ModNPC> stateMachine)
    {
    }

    public bool Update(IAutomata<ModNPC> stateMachine)
    {
        if (StarSystem.Stars.Count <= 0 || stateMachine.Context is not IDynamicMotion dynamicMotion || stateMachine.Context is not ITimer timer)
            return true;

        AggroAnythingMiningStar(stateMachine.Context);

        var distanceToStar = float.MaxValue;

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

        var motionVector = Vector2.Zero;

        foreach (var star in StarSystem.Stars)
        {
            if (Vector2.DistanceSquared(star.GetCenter(), stateMachine.Context.NPC.Center) < distanceToStar)
            {
                distanceToStar = Vector2.DistanceSquared(star.GetCenter(), stateMachine.Context.NPC.Center);
                var vectorToStar = star.GetCenter() - stateMachine.Context.NPC.Center;
                vectorToStar.Normalize();

                motionVector = vectorToStar * jumpDistance;
            }
        }

        dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + motionVector;
        timer.Time = cooldown;
        stateMachine.Context.NPC.netUpdate = true;

        return false;
    }

    public void AggroAnythingMiningStar(ModNPC context)
    {
        var npc = context.NPC;

        if (AlertSystem.alerts.Count <= 0)
            return;

        var distanceToStar = float.MaxValue;
        var target = -1;

        foreach (var alert in AlertSystem.alerts)
        {
            if (alert.alertType != AlertType.MiningStar)
                continue;

            npc.target = alert.sourceEntity;

            var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.TranslatedTargetIndex].Center;

            if (Vector2.DistanceSquared(targetCenter, npc.Center) < distanceToStar)
            {
                distanceToStar = Vector2.DistanceSquared(targetCenter, npc.Center);
                target = alert.sourceEntity;
            }
        }

        npc.target = target;

        if (target == -1)
            return;

        npc.targetRect = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].getRect() : Main.player[npc.TranslatedTargetIndex].getRect();
    }

    public void Exit(IAutomata<ModNPC> stateMachine)
    {

    }
}
