using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Content.Events.Space.LevelElements;
using SpaceEventMod.Core.Behavior.Automata;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Star = SpaceEventMod.Content.Events.Space.LevelElements.Star;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct DrinkStar : IState<ModNPC>
{
    public void Enter(ModNPC modNPC)
    {
        if (modNPC is not IWantStar wantStar || modNPC is not ITimer timer)
            throw new Exception("Tried to run DrinkStar state code on a non-valid npc type.");

        if (modNPC.NPC.HasValidTarget || Stars.List.Count <= 0 || !Stars.List.Contains(wantStar.ObservedStar))
            return;

        wantStar.RelativePosition = modNPC.NPC.Center - wantStar.ObservedStar.GetCenter() - wantStar.ObservedStar.SpriteDisplacement;

        timer.Time = 0;
    }

    public bool Update(ModNPC modNPC)
    {
        if (modNPC is not IWantStar wantStar || modNPC is not ITimer timer)
            throw new Exception("Tried to run DrinkStar state code on a non-valid npc type.");

        if (Stars.List.Count <= 0 || !Stars.List.Contains(wantStar.ObservedStar) || !modNPC.NPC.getRect().Intersects(wantStar.ObservedStar.GetBoundingBox()))
            return true;

        if (wantStar.DrinkAnimation() && timer.Time >= 40)
        {
            int starIndex = Stars.List.IndexOf(wantStar.ObservedStar);
            Star star = wantStar.ObservedStar;

            star.Durability -= 10;

            // shake when mining
            var starPosition = star.GetCenter();

            star.ShakeDirection = starPosition - modNPC.NPC.Center;
            star.ShakeDirection.Normalize();
            star.ShakeTime = 20;

            SoundEngine.PlaySound(SoundID.Tink, modNPC.NPC.Center);

            Stars.List[starIndex] = star;
            Stars.List[starIndex].UpdateSubscribedNPCs();
            timer.Time = 0;
        }

        timer.Time++;

        return false;
    }

    public void Exit(ModNPC modNPC)
    {

    }
}
