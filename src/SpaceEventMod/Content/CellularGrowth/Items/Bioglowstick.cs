using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.BaseTypes;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items;

internal class Bioglowstick : CellGelThrowableItem
{
    protected override Vector3 LightColor => Color.OrangeRed.ToVector3() * 2.5f;
    protected override int Frames => 1;
    protected override int Damage => 0;
    protected override float Knockback => 0;
    protected override int ThrownProjectile => ModContent.ProjectileType<BioglowstickProjectile>();
}

internal class BioglowstickProjectile : CellGelThrowable
{
    public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/Items/Bioglowstick";
    protected override Asset<Texture2D> Glow => Assets.Textures.CellularGrowth.Items.Bioglowstick_Glow.Asset;
    protected override Color BloomColor => Color.OrangeRed * 0.45f;
    protected override Color GlowColor => Color.White;
    protected override Vector3 LightColor => Color.OrangeRed.ToVector3() * 2.5f;
    protected override int Frames => 1;
    protected override int Damage => 0;
    protected override float Knockback => 0;
    protected override float GlowScale => 1.1f;
    protected override bool Glows => true;
}
