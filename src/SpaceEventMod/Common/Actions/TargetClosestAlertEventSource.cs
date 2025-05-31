using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.Props;
using SpaceEventMod.Core.Props.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Common.Actions;

public class TargetClosestAlertEventSource : Node
{
    private AlertType[] typesToTarget;

    public TargetClosestAlertEventSource(params AlertType[] typesToTarget)
    {
        this.typesToTarget = typesToTarget;
    }

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        if (npc.ModNPC is not IRespondToEvent)
            return NodeState.Failure;

        if (ComponentManager.HasProp((npc.ModNPC as IRespondToEvent).EventProp))
            return NodeState.Failure;

        List<AlertEvent> alerts = AlertEventSystem.GetAlertEventsInRange(npc.Center);

        AlertEvent closestEvent = null;
        float distanceToEvent = float.MaxValue;

        // get closest of the alerts
        foreach (AlertEvent alertEvent in alerts)
        {
            if (!typesToTarget.Contains(alertEvent.Type))
                continue;

            if (alertEvent.HasComponent<Target>())
            {
                if (!alertEvent.GetComponent<Target>().NPCsToTarget.Contains(npc.whoAmI))
                    continue;
            }

            float distanceTo = Vector2.DistanceSquared(npc.Center, alertEvent.GetComponent<Transformation>().Position);

            if (distanceTo < distanceToEvent)
            {
                distanceToEvent = distanceTo;
                closestEvent = alertEvent;
            }
        }

        if (closestEvent != null)
        {
            npc.target = closestEvent.GetComponent<SourceEntity>().WhoAmI;
            npc.targetRect = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Hitbox : Main.player[npc.TranslatedTargetIndex].Hitbox;
            (npc.ModNPC as IRespondToEvent).EventProp = closestEvent.prop;
            return NodeState.Success;
        }

        return NodeState.Failure;
    }
}
