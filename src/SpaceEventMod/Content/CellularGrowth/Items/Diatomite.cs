using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.BaseTypes;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items;

internal class Diatomite : CellGelThrowableItem
{
    protected override Vector3 LightColor => Color.GreenYellow.ToVector3() * 0.15f;
    protected override int Frames => 4;
    protected override int Damage => 65;
    protected override float Knockback => 8;
    protected override int ThrownProjectile => ModContent.ProjectileType<DiatomiteProjectile>();
}

internal class DiatomiteProjectile : CellGelThrowable
{
    public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/Items/Diatomite";
    protected override Asset<Texture2D> Glow => Assets.Textures.CellularGrowth.Items.Diatomite_Glow.Asset;
    protected override Color BloomColor => Color.GreenYellow * 0.35f;
    protected override Color GlowColor => Color.YellowGreen;
    protected override Vector3 LightColor => Color.GreenYellow.ToVector3() * 0.15f;
    protected override int Frames => 4;
    protected override int Damage => 65;
    protected override float Knockback => 8;
    protected override float GlowScale => 1;
}
