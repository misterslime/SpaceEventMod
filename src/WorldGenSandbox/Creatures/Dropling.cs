using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using WorldGenSandbox.Managers;
using WorldGenSandbox.Utilities;
using static System.Net.Mime.MediaTypeNames;

namespace WorldGenSandbox.Creatures;

internal sealed class Dropling : BaseCreature
{
    public override Color Color => Color.LightBlue;

    public override void AI()
    {
    }
}
