using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.DataStructures;
using SpaceEventMod.Common.Graphics;
using SpaceEventMod.Common.Physics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static ReLogic.Peripherals.RGB.Corsair.CorsairDeviceGroup;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

// thinking:
// 3 states: unconnected, start, end
// if unconnected it shoujld search for nearby connective cells it both has line of sight with and is close enough
// if connected cell found then it'll request connection, as in check if the connective cell is already connected or not
internal class ConnectiveCellSystem : ModSystem
{
    private static HashSet<Point16> s_cells = new HashSet<Point16>();
    private static Dictionary<Point16, Point16> s_connectionMap = new Dictionary<Point16, Point16>();
    private static Dictionary<Point16, (bool IsStart, VerletString Rope)> s_ropes = new Dictionary<Point16, (bool IsStart, VerletString Rope)>();
    private static List<(int TimeLeft, VerletString Rope)> s_despawningRopes = new List<(int TimeLeft, VerletString Rope)>();

    /// <summary>
    /// Attempts to place a connective cell at the specified tile coordinate.
    /// </summary>
    /// <returns>Whether the placement was successful or not.</returns>
    public static bool TryAddConnectiveCell(int i, int j)
    {
        Tile tile = Framing.GetTileSafely(i, j);
        Point16 point = new Point16(i, j);

        if ((tile.TileType != ModContent.TileType<Cosmoss>() && tile.TileType != ModContent.TileType<Cosmostone>()) || !tile.HasTile)
        {
            Main.NewText($"Insertion failed at X={i} Y={j} because thats not Cosmostone or Cosmoss you dummy.");
            return false;
        }

        if (!s_cells.Contains(point))
        {
            s_cells.Add(point);

            var a = s_connectionMap.Count / 2f;
            var b = s_ropes.Count / 2f;
            Main.NewText($"{s_cells.Count} unconnected cells, {a} connections, {b} ropes, and {s_despawningRopes.Count} despawning ropes");
            return true;
        }

        Main.NewText($"Insertion failed at X={i} Y={j}");
        return false;
    }

    /// <summary>
    /// Attempts to kill any connective cell placed at the specified tile coordinate.
    /// </summary>
    /// <returns>Whether the removal was successful or not.</returns>
    public static bool TryKillConnectiveCell(int i, int j)
    {
        Point16 point = new Point16(i, j);

        if (s_cells.Contains(point))
        {
            s_cells.Remove(point);
            TryDisconnectRope(in point, true);

            var a = s_connectionMap.Count / 2f;
            var b = s_ropes.Count / 2f;
            Main.NewText($"{s_cells.Count} unconnected cells, {a} connections, {b} ropes, and {s_despawningRopes.Count} despawning ropes");
            return true;
        }

        if (s_connectionMap.ContainsKey(point))
        {
            s_cells.Add(s_connectionMap[point]);
            s_connectionMap.Remove(s_connectionMap[point]);
            s_connectionMap.Remove(point);
            TryDisconnectRope(in point);

            var a = s_connectionMap.Count / 2f;
            var b = s_ropes.Count / 2f;
            Main.NewText($"{s_cells.Count} unconnected cells, {a} connections, {b} ropes, and {s_despawningRopes.Count} despawning ropes");
            return true;
        }

        Main.NewText($"Removal failed at X={i} Y={j}");
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryDisconnectRope(in Point16 point, bool despawnRope = false)
    {
        if (!s_ropes.ContainsKey(point))
            return false;

        var ropeConnection = s_ropes[point];
        ropeConnection.Rope.Unlock(ropeConnection.IsStart ? 0 : ropeConnection.Rope.Count - 1);

        if (despawnRope)
            s_despawningRopes.Add((120, ropeConnection.Rope));

        s_ropes.Remove(point);

        return true;
    }

    #region Updating
    public override void PreUpdateNPCs()
    {
        var cells = s_cells.ToArray();

        // they attempt to connect to each other
        foreach (var point in cells)
        {
            if (!s_cells.Contains(point) || s_ropes.ContainsKey(point))
                continue;

            if (!Main.rand.NextBool(30))
                continue;

            var pointB = Main.rand.Next(cells);

            if (pointB == point || !s_cells.Contains(point))
                continue;

            s_connectionMap.Add(point, pointB);
            s_connectionMap.Add(pointB, point);

            s_cells.Remove(point);
            s_cells.Remove(pointB);

            // add rope string
            var direction = pointB.ToWorldCoordinates() - point.ToWorldCoordinates();
            int segmentLength = 16; // 16 px bc each segment is the length of a tile.
            //int padding = 10; // make them a lil loose;
            int segments = (int)Math.Ceiling(direction.Length() / segmentLength);

            var rope = new VerletString(point.ToWorldCoordinates(), segments, segmentLength, direction.ToRotation(), 4f);
            rope.Lock(0);
            rope.Lock(rope.Count - 1);
            rope.Gravity = Vector2.UnitY * 0.4f;

            s_ropes.Add(point, (true, rope));
            s_ropes.Add(pointB, (false, rope));

            Main.NewText($"Spawned new rope with {segments} segments of length {segmentLength}.");
            var a = s_connectionMap.Count / 2f;
            var b = s_ropes.Count / 2f;
            Main.NewText($"{s_cells.Count} unconnected cells, {a} connections, {b} ropes, and {s_despawningRopes.Count} despawning ropes");
        }

        if (Main.dedServ)
            return;

        HashSet<Point16> alreadyUpdated = new HashSet<Point16>(s_ropes.Count);

        foreach (var cell in s_ropes)
        {
            if (alreadyUpdated.Contains(cell.Key))
                continue;

            // update 
            cell.Value.Rope.Update(8, 0f, true, true);
            if (cell.Value.IsStart)
                cell.Value.Rope.AnchorStart = cell.Key.ToWorldCoordinates();
            else
                cell.Value.Rope.AnchorEnd = cell.Key.ToWorldCoordinates();

            if (s_connectionMap.ContainsKey(cell.Key))
            {
                var pointB = s_connectionMap[cell.Key];

                if (s_ropes[pointB].IsStart)
                    cell.Value.Rope.AnchorStart = pointB.ToWorldCoordinates();
                else
                    cell.Value.Rope.AnchorEnd = pointB.ToWorldCoordinates();

                alreadyUpdated.Add(pointB);
            }

            alreadyUpdated.Add(cell.Key);
        }

        // despawn ropes that are in s_despawningRopes
        for (int i = 0; i < s_despawningRopes.Count; i++)
        {
            if (s_despawningRopes[i].TimeLeft <= 0)
            {
                s_despawningRopes.RemoveAt(i);
                i--;

                var a = s_connectionMap.Count / 2f;
                var b = s_ropes.Count / 2f;
                Main.NewText("Rope despawned successfully!");
                Main.NewText($"{s_cells.Count} unconnected cells, {a} connections, {b} ropes, and {s_despawningRopes.Count} despawning ropes");
                continue;
            }

            // update 
            var entry = s_despawningRopes[i];

            entry.Rope.Update(8, 0f, true, true);
            entry.TimeLeft -= 1;

            s_despawningRopes[i] = entry;
        }
    }
    #endregion

    #region Rendering
    public override void Load()
    {
        On_Main.DoDraw_WallsAndBlacks += DrawConnectiveCells;
    }

    private void DrawConnectiveCells(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self)
    {
        orig(self);

        if (s_cells.Count == 0 && s_connectionMap.Count == 0 && s_despawningRopes.Count == 0)
            return;

        var player = Main.LocalPlayer;
        var texture = Assets.Textures.CellularGrowth.Tiles.ConnectiveCellTissue.Asset.Value;

        HashSet<Point16> alreadyDrawn = new HashSet<Point16>(s_ropes.Count);

        Rectangle[] cellTissueFrames = [
            new(2, 2, 12, 12),
            new(20, 0, 12, 16),
            new(36, 0, 16, 16),
            new(54, 0, 16, 16),
            new(72, 0, 32, 16)
        ];

        Pipeline pipeline = Graphics.BeginPipeline();


        foreach (var (TimeLeft, Rope) in s_despawningRopes)
        {
            DrawRope(in pipeline, Rope, Color.White * (TimeLeft / 120f), in cellTissueFrames);
        }

        // draw ropes
        foreach (var rope in s_ropes)
        {
            if (alreadyDrawn.Contains(rope.Key))
                continue;

            DrawRope(in pipeline, rope.Value.Rope, Color.White, in cellTissueFrames);

            if (s_connectionMap.ContainsKey(rope.Key))
            {
                var pointB = s_connectionMap[rope.Key];
                alreadyDrawn.Add(pointB);
            }

            alreadyDrawn.Add(rope.Key);
        }

        pipeline.Flush();


        using var _ = Main.spriteBatch.Scope();

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        // draw unconnected cells
        foreach (var cell in s_cells) 
        {
            if ((cell.ToWorldCoordinates() - player.Center).Length() >= 1000f)
                continue;

            var frame = texture.Frame(3, 1, (cell.X + cell.Y) % 3, 0);

            Main.spriteBatch.Draw(texture, cell.ToWorldCoordinates() - Main.screenPosition, frame, Color.White, 0f, frame.Size() * 0.5f, 1f, 0, 0);
        }

        // draw connected cells
        foreach (var cell in s_connectionMap)
        {
            var pointA = cell.Key;
            var pointB = cell.Value;

            //Main.spriteBatch.DrawLine(pointA.ToWorldCoordinates() - Main.screenPosition, pointB.ToWorldCoordinates() - Main.screenPosition, Color.Yellow, 4);

            var frame = texture.Frame(3, 1, (pointA.X + pointA.Y) % 3, 0);

            Main.spriteBatch.Draw(texture, pointA.ToWorldCoordinates() - Main.screenPosition, frame, Color.White, 0f, frame.Size() * 0.5f, 1f, 0, 0);

            frame = texture.Frame(3, 1, (pointB.X + pointB.Y) % 3, 0);

            Main.spriteBatch.Draw(texture, pointB.ToWorldCoordinates() - Main.screenPosition, frame, Color.White, 0f, frame.Size() * 0.5f, 1f, 0, 0);
        }

        Main.spriteBatch.End();
    }

    private void DrawRope(in Pipeline pipeline, VerletString tentacle, Color drawColor, in Rectangle[] cellTissueFrames)
    {
        var texture = Assets.Textures.CellularGrowth.Tiles.ConnectiveCellSmallTissue.Asset.Value;

        var repeats = tentacle.Positions.Length * 16f / texture.Height;

        pipeline
            .DrawTrail(
                tentacle.Positions,
                _ => texture.Width,
                _ => drawColor,
                Assets.Shaders.Trail.RepeatingTexture.Asset.Value,
                ("transformMatrix", Graphics.WorldTransformMatrix),
                ("sampleTexture", texture),
                ("repeats", repeats),
                ("spriteRotation", 1));
    }

    #endregion

    #region Saving/Loading
    public override void ClearWorld()
    {
        s_cells = new HashSet<Point16>();
        s_connectionMap = new Dictionary<Point16, Point16>();
        s_ropes = new Dictionary<Point16, (bool IsStart, VerletString Rope)>();
        s_despawningRopes = new List<(int TimeLeft, VerletString Rope)>();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        var cells = s_cells.ToArray();

        tag[$"CellCount"] = s_cells.Count;
        for (int i = 0; i < s_cells.Count; i++)
        {
            tag[$"CellPositionX{i}"] = cells[i].X;
            tag[$"CellPositionY{i}"] = cells[i].Y;
        }

        var startCells = s_connectionMap.Keys.ToArray();
        var endCells = s_connectionMap.Values.ToArray();

        tag[$"ConnectionCount"] = s_connectionMap.Count;
        for (int i = 0; i < s_connectionMap.Count; i++)
        {
            tag[$"ConnectionKeyX{i}"] = startCells[i].X;
            tag[$"ConnectionKeyY{i}"] = startCells[i].Y;
            tag[$"ConnectionValueX{i}"] = endCells[i].X;
            tag[$"ConnectionValueY{i}"] = endCells[i].Y;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        int cellCount = tag.GetInt("CellCount");
        for (int i = 0; i < cellCount; i++)
        {
            int x = tag.GetShort($"CellPositionX{i}");
            int y = tag.GetShort($"CellPositionY{i}");

            s_cells.Add(new Point16(x, y));
        }

        int connectionCount = tag.GetInt(nameof(connectionCount));
        for (int i = 0; i < connectionCount; i++)
        {
            int x1 = tag.GetShort($"ConnectionKeyX{i}");
            int y1 = tag.GetShort($"ConnectionKeyY{i}");

            int x2 = tag.GetShort($"ConnectionValueX{i}");
            int y2 = tag.GetShort($"ConnectionValueY{i}");

            s_connectionMap.Add(new Point16(x1, y1), new Point16(x2, y2));
        }
    }
    #endregion
}
