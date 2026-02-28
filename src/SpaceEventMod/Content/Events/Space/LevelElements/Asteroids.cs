using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Animation.SecondOrderDynamics;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Components.Animation;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Events.Space.LevelElements;

// to-do:
// - cracks to indicate low health
// - netcoding
// - make auto item hotkey select a pickaxe when hovering over an asteroid or star
// - make smart cursor select asteroids and stars
// - fix the bug where grapple hooks dont move with the asteroid or star
// - make sure stars and asteroids dont spawn inside tiles
// - make asteroids appear on the map
public class Asteroids : ModSystem
{
    public static List<Asteroid> List = new List<Asteroid>();

    internal static readonly SecondOrderAnimation AsteroidMovement = new SecondOrderAnimation(1f / 64f, 0.5f, 0.2f);

    public override void OnWorldUnload()
    {
        List.Clear();
    }

    public override void PostUpdateNPCs()
    {
        for (var i = 0; i < List.Count; i++)
        {
            var shouldDespawn = false;

            List[i] = UpdateAsteroid(List[i], out shouldDespawn);

            if (shouldDespawn)
            {
                List.RemoveAt(i);
                i--;
            }
        }
    }

    private Asteroid UpdateAsteroid(Asteroid asteroid, out bool shouldDespawn)
    {
        var newAsteroid = asteroid;

        shouldDespawn = (asteroid.GetCenter() - Main.LocalPlayer.Center).LengthSquared() > 100f * 16f * 100f * 16f;

        newAsteroid.SpriteDisplacement = MathF.Sin((Main.GameUpdateCount + asteroid.RandomTimeDisplacement) / 60f) * 4 * Vector2.UnitY;

        var target = asteroid.BeingStoodOn ? asteroid.RestPosition + Vector2.UnitY * 24f : asteroid.RestPosition;

        var physicsObject = new PhysicsObject(newAsteroid.Transform);
        physicsObject.AddComponent(new SecondOrderData(1, AsteroidMovement, target));

        SecondOrderDynamics.Solver.RunPhysicsPasses([physicsObject]);

        newAsteroid.Transform = physicsObject.Center;

        //newAsteroid.Transform = AsteroidMovement.Update(1, asteroid.Transform, );
        newAsteroid.BeingStoodOn = false;

        if (asteroid.ShakeTime > 0)
            newAsteroid.ShakeTime--;

        return newAsteroid;
    }
}
