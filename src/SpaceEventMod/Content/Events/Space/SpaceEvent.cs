using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Content.Events.Space.LevelElements;
using SpaceEventMod.Content.NPCs.Manaphages;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Geometry;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Content.Events.Space;

// https://code.tutsplus.com/make-a-splash-with-dynamic-2d-water-effects--gamedev-236t
// made with this thingy
// to-do:
// - antialiasing on the foam
// - add godrays
// - add small star pixel particles that dont appear in godrays
// - add bubble particles when you move
// - maybe stuff behind the foam could be shaded in the foam? or it could be transparent.
// - make the sea appear on the map
public class SpaceEvent : ModSystem
{
    public static FirmamentSea Sea;

    public static Vector2 SeaToWorldCoordinates(Vector2 position) => new Vector2(position.X, position.Y + Sea.SeaPos.Height.Position);

    public static Vector2 WorldToSeaCoordinates(Vector2 position) => new Vector2(position.X, position.Y - Sea.SeaPos.Height.Position);

    private FastNoiseLite noise;
    private float minimumToSpawnAsteroid = 0.7f;
    private float separationDistance = 10 * 16;
    private float starSeparationDistance = 120 * 16;

    public override void ClearWorld()
    {
        Sea = new FirmamentSea();

        noise = new FastNoiseLite(Main.ActiveWorldFileData.Seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
    }

    public override void PostUpdatePlayers()
    {
        if (Sea.Springs is null)
            return;

        Sea = Sea.UpdateChunks()
            .UpdateSeaHeight()
            .CollideSprings()
            .PropagateWaves(0.04f)
            .UpdateSprings(0.1f, 0.005f);

        SpawnAsteroids();
        SpawnStars();
        SpawnAmbientEnemies();
    }

    private void SpawnAmbientEnemies()
    {
        if (!Sea.CanSpawnThings || Main.gameMenu || Main.gameInactive)
            return;

        var playerCenter = Main.player[Main.myPlayer].Center;
        var randomPosition = playerCenter + Main.rand.NextVector2CircularEdge(75 * 16, 75 * 16);
        // 0.05f

        // only spawn 20 tiles above the sea surface
        if (randomPosition.Y > (float)(Main.worldSurface * 0.35 * 16) - 320 || randomPosition.Y <= 5 * 16)
            return;

        if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(1000))
        {
            var enemyPosition = randomPosition + Main.rand.NextVector2CircularEdge(20 * 16, 20 * 16);

            int n = NPC.NewNPC(new EntitySource_SpawnNPC(), (int)randomPosition.X, (int)randomPosition.Y, ModContent.NPCType<Manaphage>());
            if (Main.npc.IndexInRange(n))
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
        }
    }

    private void SpawnStars()
    {
        if (!Sea.CanSpawnThings || Main.gameMenu || Main.gameInactive)
            return;

        var playerCenter = Main.player[Main.myPlayer].Center;
        var randomPosition = playerCenter + Main.rand.NextVector2CircularEdge(75 * 16, 75 * 16);
        // 0.05f

        // only spawn 20 tiles above the sea surface
        if (randomPosition.Y > (float)(Main.worldSurface * 0.35 * 16) - 320 || randomPosition.Y <= 5 * 16)
            return;

        var stars = Stars.List;

        if (stars.Count > 0)
        {
            foreach (var star in stars)
            {
                if ((star.Position - randomPosition).LengthSquared() <= Math.Pow(starSeparationDistance, 2))
                    return;

                if ((star.Position - playerCenter).LengthSquared() <= Math.Pow(starSeparationDistance * 1.35, 2))
                    return;
            }
        }

        Stars.List.Add(new Events.Space.LevelElements.Star(randomPosition));

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            int enemies = Main.rand.Next(1, 4);

            for (int i = 0; i < enemies; i++)
            {
                var enemyPosition = randomPosition + Main.rand.NextVector2CircularEdge(20 * 16, 20 * 16);

                int n = NPC.NewNPC(new EntitySource_SpawnNPC(), (int)enemyPosition.X, (int)enemyPosition.Y, ModContent.NPCType<Manaphage>());
                if (Main.npc.IndexInRange(n))
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
            }
        }

    }

    private void SpawnAsteroids()
    {
        if (!Sea.CanSpawnThings || Main.gameMenu || Main.gameInactive)
            return;

        var playerCenter = Main.player[Main.myPlayer].Center;
        var playerPositionSeaCoords = new Vector2(playerCenter.X, playerCenter.Y - (float)(Main.worldSurface * 0.35 * 16));
        var randomPosition = playerPositionSeaCoords + Main.rand.NextVector2CircularEdge(75 * 16, 75 * 16);
        // 0.05f

        // only spawn 20 tiles above the sea surface
        if (randomPosition.Y > -320)
            return;

        var noiseSample = (float)(1 + noise.GetNoise(randomPosition.X * 0.3f, randomPosition.Y * 0.3f, 0));

        var density = MathHelper.Lerp(0.7f, 30f, EasingFunctions.CircEaseIn(noiseSample));

        var asteroids = Asteroids.List;

        if (asteroids.Count > 0)
        {
            foreach (var asteroid in asteroids)
            {
                if ((asteroid.RestPosition - randomPosition).LengthSquared() <= Math.Pow(separationDistance * density, 2))
                    return;
            }
        }

        var asteroidType = Main.rand.Next(6);

        Point GetDimensions(int variant)
        {
            Point[] dimensions = [
                new Point(48, 16),
                new Point(48, 32),
                new Point(48, 48),
                new Point(64, 24),
                new Point(64, 32),
                new Point(64, 48),
            ];

            return dimensions[variant];
        }

        var dimensions = GetDimensions(asteroidType);

        Asteroids.List.Add(new Asteroid(randomPosition, asteroidType, dimensions.X, dimensions.Y));
    }

    public void DrawLine(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
    {
        var r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
        var v = Vector2.Normalize(begin - end);
        var angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
        if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
        spriteBatch.Draw(Assets.Assets.Textures.WhitePixel.Value, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }
}
