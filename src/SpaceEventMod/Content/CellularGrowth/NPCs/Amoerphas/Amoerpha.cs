using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.BaseTypes.NPCs.Attributes;
using SpaceEventMod.Content.BaseTypes.NPCs;
using SpaceEventMod.Content.CellularGrowth.NPCs.Amoerphas;
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

namespace SpaceEventMod.Content.CellularGrowth.NPCs.Amoerphas;

internal enum AmoerphaState : byte
{
    Debug,
    Idle
}

internal partial class Amoerpha : BaseStateNPC<AmoerphaState>
{
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

        NPC.Center = Vector2.Lerp(NPC.Center, position, 0.01f);

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

        if (_edges == null)
            return false;

        if (_edges.Count() == 0)
            return false;

        foreach (Edge edge in _edges)
        {
            float radius = MathHelper.Lerp(-0.2f, 0.25f, edge.Length / MAX_EDGE_LENGTH);

            AmoerphaMetaballRenderer.New(_nodes[edge.To], _nodes[edge.From], radius);
        }

        return false;

        if (_edges is null || _nodes is null || _adjacencyMap is null)
            return false;

        var texture = TextureAssets.Item[ItemID.FallenStar].Value;

        Rectangle frame = texture.Frame(1, 8, 0, 0);
        Vector2 origin = frame.Center.ToVector2();

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
