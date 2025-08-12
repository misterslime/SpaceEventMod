using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Content.Events.FirmamentTide.Stars;
using SpaceEventMod.Core.Behavior.Automata;
using System;
using Terraria;
using Terraria.ModLoader;
using Star = SpaceEventMod.Content.Events.FirmamentTide.Stars.Star;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct MovementToStar(params float[] arguments) : IState<ModNPC>
{
    private float[] arguments = arguments;

    public void Enter(ModNPC modNPC)
    {
        var npc = modNPC.NPC;

        if (modNPC is ITimer timer)
            timer.Time = 0;

        if (modNPC is not IWantStar wantStar)
            throw new Exception("Tried to run MovementToStar state code on a non-valid npc type.");

        if (Stars.List.Count <= 0)
            return;

        var distanceToStar = float.MaxValue;

        foreach (var star in Stars.List)
        {
            if (Vector2.DistanceSquared(star.GetCenter(), npc.Center) < distanceToStar)
            {
                distanceToStar = Vector2.DistanceSquared(star.GetCenter(), npc.Center);
                wantStar.ObservedStar = star;
            }
        }

        wantStar.ObservedStar.SubscribeNPC(npc.whoAmI);
    }

    public bool Update(ModNPC modNPC)
    {
        var npc = modNPC.NPC;

        if (modNPC is not IMovement npcMovement || modNPC is not IWantStar wantStar)
            throw new Exception("Tried to run MovementToStar state code on a non-valid npc type.");

        if (npc.HasValidTarget || Stars.List.Count <= 0 || !Stars.List.Contains(wantStar.ObservedStar))
            return true;

        var vectorToStar = wantStar.ObservedStar.GetCenter() - npc.Center;

        npcMovement.EntityMovement(vectorToStar, arguments);

        return false;
    }

    public void Exit(ModNPC modNPC)
    {

    }
}
