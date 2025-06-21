using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.Automata;
using SpaceEventMod.Core.GameObjects.Alerts;
using SpaceEventMod.Core.GameObjects.Stars;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct MovementToStar(params float[] arguments) : IState<ModNPC>
{
    private float[] arguments = arguments;

    public void Enter(IAutomata<ModNPC> stateMachine)
    {
    }

    public bool Update(IAutomata<ModNPC> stateMachine)
    {
        var modNPC = stateMachine.Context;
        var npc = modNPC.NPC;

        if (StarSystem.Stars.Count <= 0 || modNPC is not IMovement npcMovement)
            return true;

        AggroAnythingMiningStar(modNPC);

        var distanceToStar = float.MaxValue;
        var motionVector = Vector2.Zero;

        foreach (var star in StarSystem.Stars)
        {
            if (Vector2.DistanceSquared(star.GetCenter(), npc.Center) < distanceToStar)
            {
                distanceToStar = Vector2.DistanceSquared(star.GetCenter(), npc.Center);
                var vectorToStar = star.GetCenter() - npc.Center;
                motionVector = vectorToStar;
            }
        }

        npcMovement.EntityMovement(motionVector, arguments);

        return false;
    }

    public void AggroAnythingMiningStar(ModNPC modNPC)
    {
        var npc = modNPC.NPC;

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
