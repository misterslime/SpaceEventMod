using GoldMeridian.CodeAnalysis;
using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Miscellaneous.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Miscellaneous.Gores;

[ExtensionDataFor<Gore>]
internal sealed class SplatterGoreData
{
    public required bool HasHitGround { get; set; }
    public required bool ShouldSpawnDust { get; set; }
    public required Color SplatColor { get; set; }
}

public static partial class SplatterGoreExt
{
    internal static SplatterGoreData TryGetData(this Gore gore, Color defaultColor)
    {
        gore.SplatterGoreData ??= new SplatterGoreData
        {
            HasHitGround = false,
            ShouldSpawnDust = false,
            SplatColor = defaultColor,
        };

        return gore.SplatterGoreData!;
    }
}

[Autoload(false)]
internal class LiquidSplattererGore : ModGore
{
    public override bool Update(Gore gore)
    {
        if (gore.SplatterGoreData is null)
            return base.Update(gore);

        bool oldGroundValue = gore.SplatterGoreData!.HasHitGround;
        bool oldSpawnSplatValue = gore.SplatterGoreData!.ShouldSpawnDust;

        if (gore.velocity != Collision.TileCollision(gore.position, gore.velocity, 16, 14))
        {
            gore.SplatterGoreData!.HasHitGround = true;
            gore.SplatterGoreData!.ShouldSpawnDust = true;
        }
        else
        {
            gore.SplatterGoreData!.ShouldSpawnDust = false;
        }

        if (oldGroundValue != gore.SplatterGoreData!.HasHitGround)
        {
            SoundEngine.PlaySound(SoundID.Item167 with { Pitch = 1f, Volume = 0.5f }, gore.position);

            Dust dust = Dust.NewDustDirect(gore.position, 0, 0, ModContent.DustType<TileSplatBig>(), 0f, 0f, 100, default, 1f);
            dust.velocity = Vector2.Zero;
            dust.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
            dust.fadeIn = 0;
            dust.color = gore.SplatterGoreData!.SplatColor;
            dust.customData = 600;
        }
        else if (oldSpawnSplatValue != gore.SplatterGoreData!.ShouldSpawnDust && Main.rand.NextBool(5))
        {
            Dust dust = Dust.NewDustDirect(gore.position, 0, 0, ModContent.DustType<TileSplat>(), 0f, 0f, 100, default, 1f);
            dust.velocity = Vector2.Zero;
            dust.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
            dust.fadeIn = 0;
            dust.color = gore.SplatterGoreData!.SplatColor;
            dust.customData = 300;
        }

        return base.Update(gore);
    }
}
