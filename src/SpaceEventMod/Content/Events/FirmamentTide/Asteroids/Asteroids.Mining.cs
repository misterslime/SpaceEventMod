using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Events.FirmamentTide.Asteroids;

public class AsteroidMining : ILoadable
{
    public void Load(Mod mod)
    {
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += MineAsteroid;
    }

    public void Unload()
    {
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool -= MineAsteroid;
    }

    private void MineAsteroid(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y)
    {
        if (self.whoAmI == Main.myPlayer)
        {
            for (var i = 0; i < Asteroids.List.Count; i++)
            {
                bool hitAsteroid = false;
                bool destroyAsteroid = false;

                Asteroids.List[i] = MineAsteroid(Asteroids.List[i], self, sItem, x, y, out hitAsteroid, out destroyAsteroid);

                if (hitAsteroid)
                {
                    SoundEngine.PlaySound(SoundID.Tink, Main.MouseWorld);
                    self.ApplyItemTime(sItem, self.pickSpeed * 1.5f);
                }

                if (destroyAsteroid)
                {
                    SoundEngine.PlaySound(SoundID.Item70, Asteroids.List[i].GetCenter());
                    Asteroids.List.RemoveAt(i);
                }

                if (hitAsteroid || destroyAsteroid)
                {
                    canHitWalls = false;
                    return;
                }
            }
        }

        orig(self, sItem, out canHitWalls, x, y);
    }

    private Asteroid MineAsteroid(Asteroid asteroid, Player self, Item sItem, int x, int y, out bool hitAsteroid, out bool destroyAsteroid)
    {
        hitAsteroid = false;
        destroyAsteroid = false;

        Asteroid newAsteroid = asteroid;

        if (asteroid.GetBoundingBox().Contains((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y))
        {
            newAsteroid.Durability -= sItem.pick;

            // shake when mining
            var asteroidPosition = asteroid.GetCenter();

            newAsteroid.ShakeDirection = asteroidPosition - self.Center;
            newAsteroid.ShakeDirection.Normalize();
            newAsteroid.ShakeTime = 20;

            hitAsteroid = true;

            // delete the prop if durability is now below 0
            if (asteroid.Durability <= 0)
                destroyAsteroid = true;
        }

        return newAsteroid;
    }
}
