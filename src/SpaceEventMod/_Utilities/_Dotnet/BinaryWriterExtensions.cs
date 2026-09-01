using Microsoft.Xna.Framework;
using System.IO;

namespace SpaceEventMod;

internal static class BinaryWriterExtensions
{
    public static void WritePoint(this BinaryWriter bb, Point p)
    {
        bb.Write(p.X);
        bb.Write(p.Y);
    }
}
