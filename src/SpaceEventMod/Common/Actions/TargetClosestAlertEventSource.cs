using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Common.Components;
using SpaceEventMod.Common.Components.Events;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Common.Actions;

/// <summary>
/// Make the npc target the source of the nearest alert event.
/// </summary>
/// <param name="rangeExtension">How much further the npc will detect ranges from than the default. Defaults to 0.</param>
/// <param name="typesToTarget">What alert types the npc will target.</param>
public class TargetClosestAlertEventSource(float rangeExtension = 0f, params AlertEventType[] typesToTarget) : Node
{
    private AlertEventType[] typesToTarget = typesToTarget;
    private float rangeExtension = rangeExtension;

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        if (npc.ModNPC is not IRespondToEvent)
            return NodeState.Failure;

        if (ComponentManager.ComponentExists<AlertEvent>((npc.ModNPC as IRespondToEvent).EventProp))
            return NodeState.Failure;

        List<AlertEvent> alerts = AlertEventSystem.GetAlertEventsInRange(npc.Center, rangeExtension);

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
