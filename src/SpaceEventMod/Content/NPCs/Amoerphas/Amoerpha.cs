using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.NPCs;
using SpaceEventMod.Common.NPCs.Attributes;
using SpaceEventMod.Content.Events.Space.LevelElements;
using SpaceEventMod.Content.NPCs.Droplings;
using SpaceEventMod.Core.Geometry;
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
using static Terraria.GameContent.Skies.StardustSky;

namespace SpaceEventMod.Content.NPCs.Amoerphas;

internal enum AmoerphaState : byte
{
    Debug,
    Idle
}

internal partial class Amoerpha : BaseStateNPC<AmoerphaState>
{
    private FluidSimulation _simulation;

    private ref float Timer => ref NPC.ai[1];

    public override void SetDefaults()
    {
        NPC.width = 46;
        NPC.height = 42;
        NPC.damage = 50;
        NPC.defense = 16;
        NPC.lifeMax = 500;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _simulation = new FluidSimulation(50f, 0.35f, 0.8f, 60f, 8f, 0.075f, 7);
        _simulation.Fill(NPC.Center, 64, 0.07f, 0.07f);

        Init();
        
        Edge edgeA = AddEdge(NPC.Center, NPC.Center + Main.rand.NextVector2CircularEdge(MAX_EDGE_LENGTH, MAX_EDGE_LENGTH));
        Edge edgeB = AddEdge(edgeA.From, NPC.Center + Main.rand.NextVector2CircularEdge(MAX_EDGE_LENGTH, MAX_EDGE_LENGTH));
        Edge edgeC = AddEdge(edgeA.From, NPC.Center + Main.rand.NextVector2CircularEdge(MAX_EDGE_LENGTH, MAX_EDGE_LENGTH));
    }

    public override bool PreAI()
    {
        if (_edges.Count == 0)
        {
            return false;
        }

        var centrality = GetDegreeCentrality();

        centrality.Sort();

        Vector2 position = NPC.Center;
        float total = 0f;

        foreach (var peeb in centrality)
        {
            position += (_nodes[peeb.Node] - NPC.Center) * peeb.Centrality;
            total += peeb.Centrality;
        }

        position = Vector2.Lerp(position, _nodes[centrality.Last().Node], 0.25f);

        Vector2 fluidCenter = NPC.Center;

        foreach (var peeb in _simulation.Positions)
        {
            fluidCenter += peeb * _simulation.Scale;
        }

        fluidCenter /= _simulation.Positions.Length;
        fluidCenter -= _simulation.Position / _simulation.Scale;

        NPC.Center = Vector2.Lerp(NPC.Center, position, 0.01f);

        _simulation.Update();

        List<Line> lines = new List<Line>();

        foreach (var edge in _edges)
        {
            if (edge.Length <= 0)
                continue;

            Vector2 pointA = _nodes[edge.From];
            Vector2 pointB = _nodes[edge.To];

            lines.Add(new Line(pointA, pointB));
            //lines.Add(_nodes[edge.From]);
            //lines.Add(_nodes[edge.To]);
        }

        _simulation.AttractToSkeleton(lines, 1 / 120f, 6f);

        return true;
    }

    [StateProcess<AmoerphaState>(AmoerphaState.Debug)]
    public AmoerphaState Debug()
    {
        NPC.TargetClosest();

        Vector2 target = Main.player[NPC.target].Center;

        NearestData selectedNode = GetNearestNodeToTarget(target);

        GrowNode(selectedNode, target, 0.5f);

        foreach (int key in _nodes.Keys.ToArray())
        {
            if (_adjacencyMap[key].Count > 0)
                continue;

            _nodes.Remove(key);
            _adjacencyMap.Remove(key);
        }

        if (BodyLength > 250)
        {
            ShrinkEdges(BodyLength - 250, selectedNode.Index);
        }

        return AmoerphaState.Debug;
    }


    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        //var texture = TextureAssets.Item[ItemID.FallenStar].Value;
        var texture = Assets.Assets.Textures.NPCs.Amoerphas.AmoebaCenter.Value;


        Rectangle frame = texture.Frame(1, 8, 0, 0);
        Vector2 origin = new Vector2(texture.Width, texture.Height) * 0.5f;

        float rotation = MathF.Sin((Main.GameUpdateCount + NPC.whoAmI) / 160f) * (MathF.PI / 180f) * 10f;

        Vector2 displacement = Vector2.Zero;
        displacement.Y += MathF.Sin((Main.GameUpdateCount + NPC.whoAmI) / 40f) * 8f;

        //spriteBatch.Draw(texture, NPC.Center - Main.screenPosition + displacement, null, Color.White, rotation, origin, 1f, 0f, 0);


        if (_simulation is null)
            return false;

        AmoerphaMetaballRenderer.AddMetaballData(_simulation.Positions, 16f, _simulation.Scale);

        return false;

        if (_edges is null || _nodes is null || _adjacencyMap is null)
            return false;

        Vector2 targetPosition = Main.player[NPC.target].Center;

        var selected = GetNearestNodeToTarget(targetPosition);

        Vector2 the = targetPosition - _nodes[selected.Index];
        float hypot = the.Length();
        the = the.SafeNormalize(Vector2.Zero);

        spriteBatch.DrawLine(_nodes[selected.Index] - Main.screenPosition, targetPosition - Main.screenPosition, Color.Blue, 2);

        float angleBetween = MathF.Atan2(
            selected.Direction.X * the.Y - selected.Direction.Y * the.X,
            selected.Direction.X * the.X + selected.Direction.Y * the.Y);

        float opposite = hypot * MathF.Sin(angleBetween);
        float adjacent = hypot * MathF.Cos(angleBetween);

        Vector2 adjacentPosition = selected.Direction * adjacent;

        Vector2 oppositePosition = new Vector2(-selected.Direction.Y, selected.Direction.X) * opposite;

        spriteBatch.DrawLine(_nodes[selected.Index] - Main.screenPosition, _nodes[selected.Index] + adjacentPosition - Main.screenPosition, Color.Cyan, 2);
        spriteBatch.DrawLine(_nodes[selected.Index] + adjacentPosition - Main.screenPosition, _nodes[selected.Index] + adjacentPosition + oppositePosition - Main.screenPosition, Color.Green, 2);

        foreach (Edge edge in _edges)
        {
            float fromScale = edge.From == selected.Index ? 1.5f : 1;
            Color fromColor = edge.From == selected.Index ? Color.Red : Color.White;

            float toScale = edge.To == selected.Index ? 1.5f : 1;
            Color toColor = edge.To == selected.Index ? Color.Red : Color.White;

            spriteBatch.Draw(texture, _nodes[edge.To] - Main.screenPosition, frame, toColor, 0f, origin, toScale, 0, 0);
            spriteBatch.Draw(texture, _nodes[edge.From] - Main.screenPosition, frame, fromColor, 0f, origin, fromScale, 0, 0);
            spriteBatch.DrawLine(_nodes[edge.From] - Main.screenPosition, _nodes[edge.To] - Main.screenPosition, Color.White, 2);

        }

        return false;
    }
}
