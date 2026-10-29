using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.BaseTypes;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items;

internal class Virunade : CellGelThrowableItem
{
    protected override Vector3 LightColor => Color.Blue.ToVector3() * 0.5f;
    protected override int Frames => 1;
    protected override int Damage => 65;
    protected override float Knockback => 8;
    protected override int ThrownProjectile => ModContent.ProjectileType<VirunadeProjectile>();
}

internal class VirunadeProjectile : CellGelThrowable
{
    public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/Items/Virunade";
    protected override Asset<Texture2D> Glow => Assets.Textures.CellularGrowth.Items.Virunade_Glow.Asset;
    protected override Color BloomColor => Color.Transparent * 0.75f;
    protected override Color GlowColor => Color.Aqua;
    protected override Vector3 LightColor => Color.Blue.ToVector3() * 0.5f;
    protected override int Frames => 1;
    protected override int Damage => 65;
    protected override float Knockback => 8;
    protected override float GlowScale => 1;
}
