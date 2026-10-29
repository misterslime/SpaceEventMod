using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.BaseTypes;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items;

internal class BacteriaBomb : CellGelThrowableItem
{
    protected override Vector3 LightColor => Color.Purple.ToVector3() * 0.5f;
    protected override int Frames => 4;
    protected override int Damage => 65;
    protected override float Knockback => 8;
    protected override int ThrownProjectile => ModContent.ProjectileType<BacteriaBombProjectile>();
}

internal class BacteriaBombProjectile : CellGelThrowable
{
    public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/Items/BacteriaBomb";
    protected override Asset<Texture2D> Glow => Assets.Textures.CellularGrowth.Items.BacteriaBomb_Glow.Asset;
    protected override Color BloomColor => Color.Purple * 0.75f;
    protected override Color GlowColor => Color.Coral;
    protected override Vector3 LightColor => Color.Purple.ToVector3() * 0.5f;
    protected override int Frames => 4;
    protected override int Damage => 65;
    protected override float Knockback => 8;
    protected override float GlowScale => 1;
}