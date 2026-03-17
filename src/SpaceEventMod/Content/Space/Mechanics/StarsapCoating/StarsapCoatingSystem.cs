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

namespace SpaceEventMod.Content.Space.Mechanics.StarsapCoating;

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
        var points = new List<Point>();
        var datas = new List<byte>();

        for (var i = 0; i < Main.maxTilesX; i++)
        {
            for (var j = 0; j < Main.maxTilesX; j++)
            {
                var point = new Point(i, j);
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

        var points = tag.Get<List<Point>>("starsapPoints");
        var datas = tag.Get<List<byte>>("starsapDatas");

        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            ref StarsapTileData tileData = ref Framing.GetTileSafely(point.X, point.Y).Get<StarsapTileData>();

            tileData.Types = (StarsapTypes)datas[i];
        }
    }
}
