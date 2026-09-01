using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod;

internal static class BinaryReaderExtensions
{
    public static Point ReadPoint(this BinaryReader bb)
    {
        return new Point(bb.ReadInt32(), bb.ReadInt32());
    }
}
