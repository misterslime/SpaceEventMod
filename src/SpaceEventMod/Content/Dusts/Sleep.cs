using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core;
using SpaceEventMod.Core.GameObjects.Stars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Dusts;

public struct SleepData(int parent)
{
    public int Parent = parent;
    public readonly int RandomTimeDisplacement = Main.rand.Next(-99999, 99999);
}

public class Sleep : ModDust
{
    public override bool Update(Dust dust)
    {
        if (dust.customData == null || dust.customData is not SleepData sleepData)
        {
            dust.active = false;
            return false;
        }

        dust.color = new Color(1, 1, 1, Math.Clamp(dust.fadeIn / 60, 0, 1));

        dust.scale = 0.7f + MathF.Pow(MathF.Sin((Main.GameUpdateCount + sleepData.RandomTimeDisplacement) / 15f), 2) * 0.3f;
        dust.rotation = MathF.Sin((Main.GameUpdateCount + sleepData.RandomTimeDisplacement) / 10f) * (MathF.PI / 180f) * 10;

        dust.velocity.X *= 0.975f;

        dust.position += dust.velocity + Vector2.UnitX * MathF.Sin(dust.fadeIn / 15f) * 0.6f;

        dust.fadeIn--;
        if (dust.fadeIn <= 0)
            dust.active = false;

        return false;
    }

    public override bool PreDraw(Dust dust)
    {
        var sleepTexture = Assets.Assets.Textures.Dusts.Sleep.Value;

        Main.spriteBatch.Draw(sleepTexture, dust.position - Main.screenPosition, sleepTexture.Frame(), dust.color, dust.rotation, sleepTexture.Size() / 2f, dust.scale, SpriteEffects.None, 0f);

        return false;
    }
}
