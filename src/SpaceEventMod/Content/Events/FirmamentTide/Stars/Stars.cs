using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Events.FirmamentTide.Stars;

// to-do:
// - add starsap lmao
// - make stars appear on the map upon being found
// - star spawning
public class Stars : ModSystem
{
    public static List<Star> List = new List<Star>();

    public override void OnWorldUnload()
    {
        List.Clear();
    }

    public override void PreUpdateNPCs()
    {
        for (var i = 0; i < List.Count; i++)
        {
            var star = List[i];

            // delete the prop if durability is now below 0
            if (star.Durability <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item70, star.Position);
                List.RemoveAt(i);
                i--;
                continue;
            }

            star.SpriteDisplacement = MathF.Sin((Main.GameUpdateCount + star.RandomTimeDisplacement) / 60f) * 10 * Vector2.UnitY;
            star.Rotation = MathF.Sin((Main.GameUpdateCount + star.RandomTimeDisplacement) / 120f) * (MathF.PI / 180f) * 5;

            if (star.ShakeTime > 0)
                star.ShakeTime--;

            List[i] = star;
            List[i].UpdateSubscribedNPCs();
        }
    }
}
