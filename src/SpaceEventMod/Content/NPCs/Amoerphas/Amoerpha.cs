using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.NPCs;
using SpaceEventMod.Common.NPCs.Attributes;
using SpaceEventMod.Content.Events.Space.LevelElements;
using SpaceEventMod.Content.NPCs.Droplings;
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

        position = Vector2.Lerp(position, _nodes[centrality.Last().Node], 0.5f);

        NPC.Center = Vector2.Lerp(NPC.Center, position, 0.01f);

        return true;
    }

    [StateProcess<AmoerphaState>(AmoerphaState.Debug)]
    public AmoerphaState Debug()
    {
        NPC.TargetClosest();

        Vector2 target = Main.player[NPC.target].Center;

        NearestData selectedNode = GetNearestNodeToTarget(target);

        GrowNode(selectedNode, target, 1f);

        foreach (int key in _nodes.Keys.ToArray())
        {
            if (_adjacencyMap[key].Count > 0)
                continue;

            Main.NewText($"killed {key}");

            _nodes.Remove(key);
            _adjacencyMap.Remove(key);
        }

        if (BodyLength > 500)
        {
            ShrinkEdges(MathHelper.Lerp(BodyLength, 500, 0.98f) - 500, selectedNode.Index);
        }

        return AmoerphaState.Debug;
    }


    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Item[ItemID.FallenStar].Value;

        Rectangle frame = texture.Frame(1, 8, 0, 0);
        Vector2 origin = new Vector2(texture.Width, texture.Height / 8) * 0.5f;

        spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, frame, Color.LightBlue, 0f, origin, 2f, 0, 0);


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


        if (_simulation is null)
            return false;

        //FluidParticleTarget.AddParticles(_simulation.Positions, _simulation.Scale);

        //AmoerphaMetaballRenderer.AddMetaballData(_simulation.Positions, 16f, _simulation.Scale);

        return false;
    }
}
