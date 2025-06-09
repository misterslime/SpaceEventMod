using Microsoft.Xna.Framework;
using Mono.Cecil;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Content.NPCs;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.Behavior.StateMachines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace SpaceEventMod.Common.Actions.Leaf.Attacking;

public struct SprayInkCloud(float distance) : INode
{
    public float distance;

    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (npc.ModNPC is not ITimer timer || npc.ModNPC is not ISquidInk squidInk || npc.ModNPC is not IDynamicMotion dynamicMotion || !npc.HasValidTarget)
            return NodeState.Failure;

        if (npc.HasValidTarget)
        {
            var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.TranslatedTargetIndex].Center;

            squidInk.CloudPosition = targetCenter;
            squidInk.IsSpraying = true;
            //squidInk.Mana--;

            npc.target = -1;
            timer.Time = 120;
            dynamicMotion.TargetPosition = npc.Center;

            return NodeState.Success;
        }


        return NodeState.Failure;
    }
}
