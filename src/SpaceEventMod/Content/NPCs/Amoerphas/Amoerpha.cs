using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Events.Space.LevelElements;
using SpaceEventMod.Core.Physics.SmoothParticleHydrodynamics;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace SpaceEventMod.Content.NPCs.Amoerphas;

internal partial class Amoerpha : ModNPC
{
    private FluidSimulation _simulation;

    private ref float Timer => ref NPC.ai[1];

    public override void SetDefaults()
    {
        NPC.width = 46;
        NPC.height = 42;
        NPC.damage = 50;
        NPC.defense = 16;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _simulation = new FluidSimulation(50f, 0.35f, 5f, 20f, 2f, 0.075f, 10);

        _simulation.Fill(NPC.Center, 64, 0.07f, 0.07f);

        Init(64);

        Vector2 the = NPC.Center + Main.rand.NextVector2CircularEdge(128, 128);

        AddEdge(NPC.Center, the);
        AddEdge(0, NPC.Center + Main.rand.NextVector2CircularEdge(128, 128));
        //AddEdge(2, NPC.Center + Main.rand.NextVector2CircularEdge(128, 128));
        //AddEdge(3, NPC.Center + Main.rand.NextVector2CircularEdge(128, 128));
    }

    public override bool PreAI()
    {
        Timer++;

        return false;
    }

    private int DrawNearestNode(Vector2 target, SpriteBatch spriteBatch)
    {
        float smallestHypot = 999999999999f;
        int node = -1;
        bool found = false;

        Vector2 direction = Vector2.Zero;

        foreach (int toCheck in _edgesMap.Keys)
        {
            (Vector2 dir, bool rightAngle, float hypot) test = TestNode(toCheck, target);

            if (test.rightAngle && smallestHypot > test.hypot)
            {
                smallestHypot = test.hypot;
                node = toCheck;
                direction = test.dir;
                found = true;
            }

            spriteBatch.DrawLine(_nodes[toCheck] - Main.screenPosition, _nodes[toCheck] + test.dir * 64f - Main.screenPosition, Color.Cyan, 2);
        }


        if (found)
        {
            Vector2 the = target - _nodes[node];
            float hypot = the.Length();
            the = the.SafeNormalize(Vector2.Zero);

            spriteBatch.DrawLine(_nodes[node] - Main.screenPosition, target - Main.screenPosition, Color.Blue, 2);

            float angleBetween = MathF.Atan2(
                direction.X * the.Y - direction.Y * the.X,
                direction.X * the.X + direction.Y * the.Y);

            float opposite = hypot * MathF.Sin(angleBetween);
            float adjacent = hypot * MathF.Cos(angleBetween);

            Vector2 adjacentPosition = direction * adjacent;

            Vector2 oppositePosition = new Vector2(-direction.Y, direction.X) * opposite;

            spriteBatch.DrawLine(_nodes[node] - Main.screenPosition, _nodes[node] + adjacentPosition - Main.screenPosition, Color.Cyan, 2);
            spriteBatch.DrawLine(_nodes[node] + adjacentPosition - Main.screenPosition, _nodes[node] + adjacentPosition + oppositePosition - Main.screenPosition, Color.Green, 2);
        }

        return node;
    }

    // used for nodes with only 1 connected edge
    private (Vector2, bool, float) TestNode(int node, Vector2 target)
    {
        Vector2 position = _nodes[node];

        Vector2 toTarget = target - position;
        float hypotenuse = toTarget.Length();
        toTarget = toTarget.SafeNormalize(Vector2.Zero);

        Vector2 direction = _edgesMap[node].Count switch
        {
            1 => SingleEdgeDirection(node, in position),
            2 => DoubleEdgeDirection(node, in position),
            _ => Vector2.Zero
        };

        bool rightAngle = Vector2.Dot(toTarget, direction) > 0f;

        return (direction, rightAngle, hypotenuse);
    }

    private Vector2 SingleEdgeDirection(int node, in Vector2 position)
    {
        Vector2 otherPosition = _nodes[_edgesMap[node][0].Other(node)];
        Vector2 direction = (position - otherPosition).SafeNormalize(Vector2.Zero);
        return direction;
    }

    private Vector2 DoubleEdgeDirection(int node, in Vector2 position)
    {
        Vector2[] directions = GetNodeDirections(node);

        if (directions.Length != 2)
            throw new Exception($"WHY NOT 2 WTF!!!!!!!! Was instead {directions.Length}.");

        Vector2 direction = Vector2.Lerp(directions[0], directions[1], 0.5f);
        direction = direction.SafeNormalize(Vector2.Zero);
        direction *= -1;

        return direction;
    }


    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Item[ItemID.FallenStar].Value;

        Rectangle frame = texture.Frame(1, 8, 0, 0);
        Vector2 origin = new Vector2(texture.Width, texture.Height / 8) * 0.5f;

        spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, frame, Color.LightBlue, 0f, origin, 2f, 0, 0);


        if (_edges is null || _nodes is null || _edgesMap is null)
            return false;

        Vector2 targetPosition = Main.MouseWorld;

        DrawNearestNode(targetPosition, spriteBatch);

        foreach (Edge edge in _edges)
        {
            //bool selected = i == selectedArm.arm && j <= selectedArm.armlet;
            //float scale = selected ? 1.5f : 1;
            //Color color = selected ? Color.Red : Color.White;

            spriteBatch.Draw(texture, _nodes[edge.To] - Main.screenPosition, frame, Color.White, 0f, origin, 1f, 0, 0);
            spriteBatch.Draw(texture, _nodes[edge.From] - Main.screenPosition, frame, Color.White, 0f, origin, 1f, 0, 0);
            spriteBatch.DrawLine(_nodes[edge.From] - Main.screenPosition, _nodes[edge.To] - Main.screenPosition, Color.White, 2);
        }


        if (_simulation is null)
            return false;

        //FluidParticleTarget.AddParticles(_simulation.Positions, _simulation.Scale);

        //AmoerphaMetaballRenderer.AddMetaballData(_simulation.Positions, 16f, _simulation.Scale);

        return false;
    }
}
