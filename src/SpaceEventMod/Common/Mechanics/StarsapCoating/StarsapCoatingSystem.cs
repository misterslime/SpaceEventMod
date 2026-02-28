using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SpaceEventMod.Common.Mechanics.StarsapCoating;

internal class StarsapCoatingSystem : ModSystem
{
    public static void CoatTile(int i, int j, bool coated)
    {
        var tile = Framing.GetTileSafely(i, j);
        ref StarsapTileData tileData = ref tile.Get<StarsapTileData>();

        tileData.Coated = coated;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        List<Point> points = new List<Point>();
        List<byte> datas = new List<byte>();

        for (int i = 0; i < Main.maxTilesX; i++)
        {
            for (int j = 0; j < Main.maxTilesX; j++)
            {
                Point point = new Point(i, j);
                ref StarsapTileData tileData = ref Framing.GetTileSafely(i, j).Get<StarsapTileData>();

                if (tileData.Coated)
                {
                    points.Add(point);
                    datas.Add((byte)tileData.Types);
                }
            }
        }

        if (datas.Count > 0)
        {
            tag.Add("starsapPoints", points);
            tag.Add("starsapDatas", datas);
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (!tag.ContainsKey("starsapPoints") || !tag.ContainsKey("starsapDatas"))
            return;

        List<Point> points = tag.Get<List<Point>>("starsapPoints");
        List<byte> datas = tag.Get<List<byte>>("starsapDatas");

        for (int i = 0; i < points.Count; i++)
        {
            Point point = points[i];
            ref StarsapTileData tileData = ref Framing.GetTileSafely(point.X, point.Y).Get<StarsapTileData>();

            tileData.Types = (StarsapTypes)datas[i];
        }
    }
}
