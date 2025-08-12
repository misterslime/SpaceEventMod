using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Events.FirmamentTide.FirmamentSea;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Events.FirmamentTide.Asteroids;

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

    public static readonly Vector2Dynamics AsteroidMovement = new Vector2Dynamics(1f / 64f, 0.5f, 0.2f);

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

        shouldDespawn = (asteroid.GetCenter() - Main.LocalPlayer.Center).LengthSquared() > 60f * 16f * 60f * 16f;

        newAsteroid.SpriteDisplacement = MathF.Sin((Main.GameUpdateCount + asteroid.RandomTimeDisplacement) / 60f) * 4 * Vector2.UnitY;

        newAsteroid.Transform = AsteroidMovement.Update(1, asteroid.Transform, asteroid.BeingStoodOn ? asteroid.RestPosition + Vector2.UnitY * 24f : asteroid.RestPosition);
        newAsteroid.BeingStoodOn = false;

        if (asteroid.ShakeTime > 0)
            newAsteroid.ShakeTime--;

        return newAsteroid;
    }
}

public struct Asteroid(Vector2 initialPosition, float spawnHeight, int variant, int width, int height)
{
    public Kinematics<Vector2> Transform = new Kinematics<Vector2>(new Vector2(initialPosition.X, spawnHeight));
    public int Variant = variant;
    public int Width = width;
    public int Height = height;

    public int Durability = 200;

    public Vector2 RestPosition = initialPosition;
    public bool BeingStoodOn = false;

    public Vector2 ShakeDirection = Vector2.UnitX;
    public int ShakeTime = 0;

    public Vector2 SpriteDisplacement = Vector2.Zero;
    public SpriteEffects Effects = Main.rand.NextBool(2) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

    public int RandomTimeDisplacement = Main.rand.Next(-99999, 99999);

    public Rectangle GetBoundingBox()
    {
        var worldCoords = FirmamentSeaSystem.SeaToWorldCoordinates(Transform.Position);

        return new Rectangle((int)worldCoords.X + (int)SpriteDisplacement.Y, (int)worldCoords.Y + (int)SpriteDisplacement.Y, Width, Height);
    }

    public Vector2 GetCenter()
    {
        return FirmamentSeaSystem.SeaToWorldCoordinates(GetTrueCenter());
    }

    public Vector2 GetTrueCenter()
    {
        return Transform.Position + new Vector2(Width, Height) * 0.5f;
    }
}
