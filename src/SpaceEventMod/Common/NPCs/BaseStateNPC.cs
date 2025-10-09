using Microsoft.Xna.Framework;
using Mono.Cecil;
using SpaceEventMod.Common.NPCs.Attributes;
using SpaceEventMod.Content.NPCs.Droplings;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.NPCs;

internal abstract class BaseStateNPC<TState> : ModNPC where TState : Enum
{
    protected ref float Timer => ref NPC.ai[1];

    protected TState State
    {
        get
        {
            var value = (int)NPC.ai[0];
            return Unsafe.As<int, TState>(ref value);
        }
        set
        {
            var state = Unsafe.As<TState, int>(ref value);
            NPC.ai[0] = state;
        }
    }

    protected Vector2 PreviousPosition { get; set; }

    protected Vector2 TargetPosition { get; set; }

    protected Vector2 TargetVelocity { get; set; }

    protected PhysicsPoint VelocityPhysics
    {
        get => new PhysicsPoint(NPC.velocity)
        {
            PreviousPosition = NPC.oldVelocity,
        };
        set
        {
            NPC.velocity = value.Position;
            NPC.oldVelocity = value.PreviousPosition;
        }
    }

    protected PhysicsPoint PositionPhysics
    {
        get => new PhysicsPoint(NPC.Center)
        {
            PreviousPosition = PreviousPosition,
        };
        set
        {
            NPC.Center = value.Position;
            PreviousPosition = value.PreviousPosition;
        }
    }

    public sealed override void AI()
    {
        Timer++;

        TState newState = State;

        var methods = from method in this.GetType().GetMethods()
                      where method.ReturnType == typeof(TState)
                      from attribute in method.GetCustomAttributes(typeof(StateProcessAttribute<TState>), false)
                      select new { Method = method, StateAttribute = attribute as StateProcessAttribute<TState> };

        foreach (var method in methods)
        {
            if (method.Method is null || method.StateAttribute is null)
                continue;

            if (!Equals(method.StateAttribute.State, State))
                continue;

            newState = (TState)method.Method.Invoke(this, null);
        }

        if (Equals(newState, State))
            return;

        NPC.netUpdate = true;
        State = newState;
        Timer = 0;
    }
}
