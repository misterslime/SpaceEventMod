using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Graphics;
using SpaceEventMod.Common.Physics;
using SpaceEventMod.Content.Space;
using SpaceEventMod.Content.Space.LevelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

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

    public override void Load()
    {
        LightingEngine.AfterWalls += RenderConnectiveCells;
    }

    public override void Unload()
    {
        LightingEngine.AfterWalls -= RenderConnectiveCells;
    }

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
            return false;
        }

        if (!s_cells.Contains(point))
        {
            s_cells.Add(point);
            return true;
        }

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
            return true;
        }

        if (s_connectionMap.ContainsKey(point))
        {
            s_cells.Add(s_connectionMap[point]);
            s_connectionMap.Remove(s_connectionMap[point]);
            s_connectionMap.Remove(point);
            TryDisconnectRope(in point);
            return true;
        }

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
        int maxLength = (int)Math.Pow(35, 2);
        int minLength = (int)Math.Pow(5, 2);

        var cells = s_cells.ToArray();

        // they attempt to connect to each other
        foreach (var point in cells)
        {
            if (!s_cells.Contains(point) || s_ropes.ContainsKey(point))
                continue;

            if (!Main.rand.NextBool(50))
                continue;

            var pointB = Main.rand.Next(cells);

            var vectorTo = (pointB - point).ToVector2();

            if (pointB == point || !s_cells.Contains(point) || vectorTo.LengthSquared() >= maxLength || vectorTo.LengthSquared() <= minLength)
                continue;

            vectorTo = vectorTo.SafeNormalize(Vector2.Zero) * 24;

            if (!Collision.CanHitLine(point.ToWorldCoordinates() + vectorTo, 6, 6, pointB.ToWorldCoordinates() - vectorTo, 6, 6))
                continue;

            s_connectionMap.Add(point, pointB);
            s_connectionMap.Add(pointB, point);

            s_cells.Remove(point);
            s_cells.Remove(pointB);

            // add rope string
            ConnectCells(point, pointB);

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
            cell.Value.Rope.Update(5, 0f, true, true);
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
                continue;
            }

            // update 
            var entry = s_despawningRopes[i];

            entry.Rope.Update(5, 0f, true, true);
            entry.TimeLeft -= 1;

            s_despawningRopes[i] = entry;
        }
    }

    private bool ConnectCells(Point16 a, Point16 b)
    {
        var direction = b.ToWorldCoordinates() - a.ToWorldCoordinates();
        int segmentLength = 24;
        int segments = (int)Math.Ceiling(direction.Length() / segmentLength);

        var rope = new VerletString(a.ToWorldCoordinates(), segments, segmentLength, direction.ToRotation(), 4f);
        rope.Lock(0);
        rope.Lock(rope.Count - 1);
        rope.Gravity = Vector2.UnitY * 0.4f;

        if (s_ropes.TryAdd(a, (true, rope)))
        {
            s_ropes.Add(b, (false, rope));
            return true;
        }

        return false;
    }
    #endregion

    #region Rendering
    private readonly MiscPaintSystem _ropePaintCache = new MiscPaintSystem(Assets.Textures.CellularGrowth.Tiles.ConnectiveCellSmallTissue.Asset);
    private readonly MiscPaintSystem _cellPaintCache = new MiscPaintSystem(Assets.Textures.CellularGrowth.Tiles.ConnectiveCellTissue.Asset);

    private void RenderConnectiveCells(object? sender, DrawEventArgs e)
    {
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
            var smallTissue = Assets.Textures.CellularGrowth.Tiles.ConnectiveCellSmallTissue.Asset.Value;
            DrawSingleConnectedRope(in pipeline, smallTissue, Rope, Color.White * (TimeLeft / 120f));
        }

        // draw ropes
        foreach (var rope in s_ropes)
        {
            if (alreadyDrawn.Contains(rope.Key))
                continue;


            if (s_connectionMap.ContainsKey(rope.Key))
            {
                var pointB = s_connectionMap[rope.Key];

                alreadyDrawn.Add(pointB);
                alreadyDrawn.Add(rope.Key);

                Texture2D? paint1Texture = null;
                Texture2D? paint2Texture = null;

                if ((!Main.tile[rope.Key].IsTileInvisible && rope.Value.IsStart) || (!Main.tile[pointB].IsTileInvisible && !rope.Value.IsStart))
                {
                    int paint1Color = rope.Value.IsStart ? Main.tile[rope.Key].TileColor : Main.tile[pointB].TileColor;
                    _ropePaintCache.TryGetPaintTexture(paint1Color, out paint1Texture);
                }
                else
                {
                    paint1Texture = Assets.Textures.EmptyPixel.Asset.Value;
                }

                if ((!Main.tile[pointB].IsTileInvisible && rope.Value.IsStart) || (!Main.tile[rope.Key].IsTileInvisible && !rope.Value.IsStart))
                {

                    int paint2Color = rope.Value.IsStart ? Main.tile[pointB].TileColor : Main.tile[rope.Key].TileColor;
                    _ropePaintCache.TryGetPaintTexture(paint2Color, out paint2Texture);
                }
                else
                {
                    paint2Texture = Assets.Textures.EmptyPixel.Asset.Value;
                }


                paint1Texture ??= Assets.Textures.CellularGrowth.Tiles.ConnectiveCellSmallTissue.Asset.Value;
                paint2Texture ??= Assets.Textures.CellularGrowth.Tiles.ConnectiveCellSmallTissue.Asset.Value;

                DrawDoubleConnectedRope(in pipeline, paint1Texture, paint2Texture, rope.Value.Rope, Color.White);

                continue;
            }

            if (Main.tile[rope.Key].IsTileInvisible)
                continue;

            alreadyDrawn.Add(rope.Key);

            Texture2D? paintTexture = null;
            _ropePaintCache.TryGetPaintTexture(Main.tile[rope.Key].TileColor, out paintTexture);
            paintTexture ??= Assets.Textures.CellularGrowth.Tiles.ConnectiveCellSmallTissue.Asset.Value;
            DrawSingleConnectedRope(in pipeline, paintTexture, rope.Value.Rope, Color.White);
        }

        pipeline.Flush();

        // draw unconnected cells
        foreach (var cell in s_cells) 
        {
            if (Main.tile[cell].IsTileInvisible)
                continue;

            if ((cell.ToWorldCoordinates() - player.Center).Length() >= 1000f)
                continue;

            var frame = texture.Frame(3, 1, (cell.X + cell.Y) % 3, 0);

            Texture2D? paintTexture = null;

            _cellPaintCache.TryGetPaintTexture(Main.tile[cell].TileColor, out paintTexture);

            paintTexture ??= texture;

            e.SpriteBatch.Draw(paintTexture, cell.ToWorldCoordinates() - Main.screenPosition, frame, Color.White, 0f, frame.Size() * 0.5f, 1f, 0, 0);
        }

        // draw connected cells
        foreach (var cell in s_connectionMap)
        {
            var pointA = cell.Key;
            var pointB = cell.Value;

            //Main.spriteBatch.DrawLine(pointA.ToWorldCoordinates() - Main.screenPosition, pointB.ToWorldCoordinates() - Main.screenPosition, Color.Yellow, 4);

            if (!Main.tile[pointA].IsTileInvisible)
            {
                Texture2D? paintTexture = null;
                _cellPaintCache.TryGetPaintTexture(Main.tile[pointA].TileColor, out paintTexture);
                paintTexture ??= texture;

                var frame = texture.Frame(3, 1, (pointA.X + pointA.Y) % 3, 0);

                e.SpriteBatch.Draw(paintTexture, pointA.ToWorldCoordinates() - Main.screenPosition, frame, Color.White, 0f, frame.Size() * 0.5f, 1f, 0, 0);

            }

            if (!Main.tile[pointB].IsTileInvisible)
            {
                Texture2D? paintTexture = null;
                _cellPaintCache.TryGetPaintTexture(Main.tile[pointB].TileColor, out paintTexture);
                paintTexture ??= texture;

                var frame = texture.Frame(3, 1, (pointB.X + pointB.Y) % 3, 0);

                e.SpriteBatch.Draw(paintTexture, pointB.ToWorldCoordinates() - Main.screenPosition, frame, Color.White, 0f, frame.Size() * 0.5f, 1f, 0, 0);
            }
        }
    }

    private void DrawDoubleConnectedRope(in Pipeline pipeline, Texture2D texture1, Texture2D texture2, VerletString tentacle, Color drawColor)
    {
        var tex = Assets.Textures.CellularGrowth.Tiles.ConnectiveCellSmallTissue.Asset.Value;
        var repeats = tentacle.Positions.Length * 24f / tex.Height;

        pipeline
            .DrawTrail(
                tentacle.Positions,
                _ => tex.Width,
                a => Color.Lerp(Color.Black, Color.Red, a),
                Assets.Shaders.Trail.ConnectiveCellPainted.Asset.Value,
                ("transformMatrix", Graphics.WorldTransformMatrix),
                ("paint1Texture", texture1),
                ("paint2Texture", texture2),
                ("repeats", repeats),
                ("spriteRotation", 1));
    }

    private void DrawSingleConnectedRope(in Pipeline pipeline, Texture2D texture, VerletString tentacle, Color drawColor)
    {
        var tex = Assets.Textures.CellularGrowth.Tiles.ConnectiveCellSmallTissue.Asset.Value;
        var repeats = tentacle.Positions.Length * 24f / tex.Height;

        pipeline
            .DrawTrail(
                tentacle.Positions,
                _ => tex.Width,
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

        int connectionCount = tag.GetInt($"ConnectionCount");
        for (int i = 0; i < connectionCount; i++)
        {
            int x1 = tag.GetShort($"ConnectionKeyX{i}");
            int y1 = tag.GetShort($"ConnectionKeyY{i}");

            int x2 = tag.GetShort($"ConnectionValueX{i}");
            int y2 = tag.GetShort($"ConnectionValueY{i}");

            var a = new Point16(x1, y1);
            var b = new Point16(x2, y2);

            if (ConnectCells(a, b))
                s_connectionMap.Add(a, b);
        }
    }
    #endregion

    #region Map Layer
    public class ConnectiveCellMapLayer : ModMapLayer
    {
        public override Position GetDefaultPosition() => BeforeFirstVanillaLayer;

        public override void Draw(ref MapOverlayDrawContext context, ref string text)
        {
            // We can check Main.mapStyle or Main.mapFullscreen to limit drawing to specific map modes.
            // This example doesn't draw on the overlay map, but draws on the minimap and fullscreen map.
            if (Main.mapStyle == 2)
                return;

            var whitePixel = Assets.Textures.WhitePixel.Asset.Value;

            foreach (var asteroid in s_cells)
            {
                var scale = new Vector2(3, 3) * context.MapScale;

                var position = asteroid.ToVector2();
                Draw(context, whitePixel, position, Color.Red, new SpriteFrame(1, 1, 0, 0), scale, Alignment.TopLeft);
            }

            foreach (var asteroid in s_connectionMap)
            {
                var scale = new Vector2(3, 3) * context.MapScale;

                var position = asteroid.Key.ToVector2();
                Draw(context, whitePixel, position, Color.Red, new SpriteFrame(1, 1, 0, 0), scale, Alignment.TopLeft);

                position = asteroid.Value.ToVector2();
                Draw(context, whitePixel, position, Color.Red, new SpriteFrame(1, 1, 0, 0), scale, Alignment.TopLeft);
            }


            HashSet<Point16> alreadyDrawn = new HashSet<Point16>(s_ropes.Count);

            // draw ropes
            foreach (var rope in s_ropes)
            {
                if (alreadyDrawn.Contains(rope.Key))
                    continue;

                if (s_connectionMap.ContainsKey(rope.Key))
                {
                    var pointB = s_connectionMap[rope.Key];

                    alreadyDrawn.Add(pointB);
                }

                alreadyDrawn.Add(rope.Key);

                var scale = new Vector2(2, 2) * context.MapScale;

                foreach (var position in rope.Value.Rope.Positions)
                {
                    var mapPosition = position / 16;
                    Draw(context, whitePixel, mapPosition, Color.Red, new SpriteFrame(1, 1, 0, 0), scale, Alignment.TopLeft);
                }
            }
        }

        public bool Draw(MapOverlayDrawContext context, Texture2D texture, Vector2 position, Color color, SpriteFrame frame, Vector2 scale, Alignment alignment, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            position = (position - context.MapPosition) * context.MapScale + context.MapOffset;
            if (context.ClippingRectangle.HasValue && !context.ClippingRectangle.Value.Contains(position.ToPoint()))
                return false;

            var sourceRectangle = frame.GetSourceRectangle(texture);
            var vector = sourceRectangle.Size() * alignment.OffsetMultiplier;
            var position2 = position;

            scale *= context.DrawScale;
            var vector2 = position - vector * scale;

            Main.spriteBatch.Draw(texture, position2, sourceRectangle, color, 0f, vector, scale, spriteEffects, 0f);
            return false;
        }
    }
    #endregion
}
