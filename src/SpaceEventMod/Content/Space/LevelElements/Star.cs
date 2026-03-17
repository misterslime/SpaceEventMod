using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Content.Space.LevelElements;

public struct Star(Vector2 spawnPosition, Rectangle frame)
{
    private HashSet<int> SubscribedNPCs = [];

    public readonly int Width = 68;
    public readonly int Height = 68;
    public readonly int RandomTimeDisplacement = Main.rand.Next(-99999, 99999);
    public readonly Rectangle Frame = frame;

    public Vector2 Position = spawnPosition;
    public float Rotation = 0;

    public int Durability = 1000;

    public Vector2 ShakeDirection = Vector2.UnitX;
    public int ShakeTime = 0;

    public Vector2 SpriteDisplacement = Vector2.Zero;
    public SpriteEffects Effects = Main.rand.NextBool(2) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

    public readonly Rectangle GetBoundingBox()
    {
        return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
    }

    public readonly Vector2 GetCenter()
    {
        return Position + new Vector2(Width, Height) * 0.5f;
    }

    public void SubscribeNPC(int npcID)
    {
        SubscribedNPCs.Add(npcID);
        UpdateSubscribedNPCs();
    }

    public void UnsubscribeNPC(int npcID)
    {
        SubscribedNPCs.Remove(npcID);
        UpdateSubscribedNPCs();
    }

    public void IsNPCSubscribed(int npcID)
    {
        SubscribedNPCs.Contains(npcID);
    }

    public void UpdateSubscribedNPCs()
    {
        foreach (var npcIndex in SubscribedNPCs.ToList())
        {
            if (!Main.npc[npcIndex].active)
            {
                SubscribedNPCs.Remove(npcIndex);
                continue;
            }

            /*if (Main.npc[npcIndex].ModNPC is not IWantStar wantStar)
                continue;

            wantStar.ObservedStar = this;*/
        }
    }

    public void InformSubscribedNPCs(Action<NPC> action)
    {
        foreach (var npcIndex in SubscribedNPCs.ToList())
        {
            if (Main.npc[npcIndex].active)
                action.Invoke(Main.npc[npcIndex]);
        }
    }
}

