using Microsoft.Xna.Framework;
using Mono.Cecil;
using SpaceEventMod.Common.NPCs.Attributes;
using SpaceEventMod.Content.NPCs.Droplings;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
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
    private readonly static List<FieldInfo> s_extraAiFields = new List<FieldInfo>();
    private readonly static Dictionary<TState, MethodInfo> s_stateBehaviors = new Dictionary<TState, MethodInfo>();

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

    public PhysicsPoint VelocityPhysics
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

    public PhysicsPoint PositionPhysics
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

    public sealed override void Load()
    {
        var methods = from method in this.GetType().GetMethods()
                      where method.ReturnType == typeof(TState)
                      select method;

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttributes(typeof(StateProcessAttribute<TState>), false).FirstOrDefault();

            if (method is null || attribute is null)
                continue;

            s_stateBehaviors.Add((attribute as StateProcessAttribute<TState>).State, method);
        }

        var fields = this.GetFields();

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttributes(typeof(ExtraAIAttribute), false).FirstOrDefault();

            if (field is null || attribute is null)
                continue;

            if (!ExtraAIAttribute.AllowedExtraAITypes.Contains(field.FieldType))
                continue;

            s_extraAiFields.Add(field);
        }
    }

    public sealed override void SendExtraAI(BinaryWriter writer)
    {
        if (s_extraAiFields.Count == 0)
            return;

        foreach (var field in s_extraAiFields)
        {
            if (field.FieldType == typeof(Vector2))
                writer.WriteVector2((Vector2)this.GetFieldValue(field));

            else if (field.FieldType == typeof(int))
                writer.Write((int)this.GetFieldValue(field));

            else if (field.FieldType == typeof(float))
                writer.Write((float)this.GetFieldValue(field));

            else if (field.FieldType == typeof(bool))
                writer.Write((bool)this.GetFieldValue(field));
        }
    }

    public sealed override void ReceiveExtraAI(BinaryReader reader)
    {
        if (s_extraAiFields.Count == 0)
            return;

        foreach (var field in s_extraAiFields)
        {
            if (field.FieldType == typeof(Vector2))
                this.SetFieldValue(field, reader.ReadVector2());

            else if (field.FieldType == typeof(int))
                this.SetFieldValue(field, reader.ReadInt32());

            else if (field.FieldType == typeof(float))
                this.SetFieldValue(field, reader.ReadSingle());

            else if (field.FieldType == typeof(bool))
                this.SetFieldValue(field, reader.ReadBoolean());
        }
    }

    public sealed override void AI()
    {
        Timer++;

        if (!s_stateBehaviors.ContainsKey(State))
            return;

        TState newState = State;

        newState = (TState)s_stateBehaviors[State].Invoke(this, null);

        if (Equals(newState, State))
            return;

        NPC.netUpdate = true;
        State = newState;
        Timer = 0;
    }
}
