using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.BaseTypes;

internal class ExtraAIAttribute : Attribute
{
    public static readonly Type[] AllowedExtraAITypes = new Type[]
    {
        typeof(Vector2),
        typeof(int),
        typeof(bool),
        typeof(float)
    };
}
