using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Components.Animation;
using SpaceEventMod.Common.Components.Behavior;
using SpaceEventMod.Common.Components.Rendering;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Props;
using System;
using Terraria;
using Terraria.ID;

namespace SpaceEventMod.Common.Components.Cosmostone;

public class AsteroidNoiseSpawner(FastNoiseLite noise, float minimumToSpawnAsteroid, float separationDistance) : Component
{
    public FastNoiseLite noise = noise;
    public float minimumToSpawnAsteroid = minimumToSpawnAsteroid;
    public float separationDistance = separationDistance;
}

