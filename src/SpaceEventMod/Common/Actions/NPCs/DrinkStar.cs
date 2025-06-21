using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.Automata;
using SpaceEventMod.Core.GameObjects.Stars;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Star = SpaceEventMod.Core.GameObjects.Stars.Star;

namespace SpaceEventMod.Common.Actions.NPCs;

public struct DrinkStar : IState<ModNPC>
{
    public void Enter(ModNPC modNPC)
    {
        if (modNPC is not IWantStar wantStar || modNPC is not ITimer timer)
            throw new Exception("Tried to run DrinkStar state code on a non-valid npc type.");

        if (modNPC.NPC.HasValidTarget || StarSystem.Stars.Count <= 0 || !StarSystem.Stars.Contains(wantStar.ObservedStar))
            return;

        wantStar.RelativePosition = modNPC.NPC.Center - wantStar.ObservedStar.GetCenter() - wantStar.ObservedStar.SpriteDisplacement;

        timer.Time = 0;
    }

    public bool Update(ModNPC modNPC)
    {
        if (modNPC is not IWantStar wantStar || modNPC is not ITimer timer)
            throw new Exception("Tried to run DrinkStar state code on a non-valid npc type.");

        if (StarSystem.Stars.Count <= 0 || !StarSystem.Stars.Contains(wantStar.ObservedStar) || !modNPC.NPC.getRect().Intersects(wantStar.ObservedStar.GetBoundingBox()))
            return true;

        if (wantStar.DrinkAnimation() && timer.Time >= 40)
        {
            int starIndex = StarSystem.Stars.IndexOf(wantStar.ObservedStar);
            Star star = wantStar.ObservedStar;

            star.Durability -= 10;

            // shake when mining
            var starPosition = star.GetCenter();

            star.ShakeDirection = starPosition - modNPC.NPC.Center;
            star.ShakeDirection.Normalize();
            star.ShakeTime = 20;

            SoundEngine.PlaySound(SoundID.Tink, modNPC.NPC.Center);

            StarSystem.Stars[starIndex] = star;
            StarSystem.Stars[starIndex].UpdateSubscribedNPCs();
            timer.Time = 0;
        }

        timer.Time++;

        return false;
    }

    public void Exit(ModNPC modNPC)
    {

    }
}
