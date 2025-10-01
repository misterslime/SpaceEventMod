using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Physics.Components;

internal struct NPCReference(int npc) : IComponent
{
    public int NPCIndex { get; init; } = npc;
}
