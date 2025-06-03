using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.GameObjects.Stars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Targeting;

public struct TargetRandomFacingPosition(float distance, float range) : INode
{
    private float distance = distance;
    private float range = range;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (npc.ModNPC is not ITargetPosition)
            return NodeState.Failure;

        var facingVector = npc.rotation.ToRotationVector2();
        ((ITargetPosition)npc.ModNPC).TargetPosition = npc.Center + facingVector * distance + Main.rand.NextVector2Circular(distance, distance);

        return NodeState.Success;
    }
}
