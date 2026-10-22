using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Interfaces;
using SpaceEventMod.Common.Physics;
using SpaceEventMod.Content.Miscellaneous.Dusts;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SpaceEventMod.Content.CellularGrowth.Items;

internal class Ball : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 2);

        Item.shootSpeed = 10f;
        Item.shoot = ModContent.ProjectileType<BallProjectile>();
        Item.consumable = true;
        Item.UseSound = SoundID.Item1;
        Item.useAnimation = 8;
        Item.useTime = 8;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.useStyle = ItemUseStyleID.Swing;

    }
}

internal class BallProjectile : ModProjectile, IBall
{
    public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/Items/Ball";

    public float Radius { get => 8f; }

    private Vector2 _oldCenter;

    public override void SetDefaults()
    {
        Projectile.width = 1;
        Projectile.height = 1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;

        Projectile.timeLeft = 3600;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.WriteVector2(_oldCenter);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        _oldCenter = reader.ReadVector2();
    }

    public override void OnSpawn(IEntitySource source)
    {
        _oldCenter = Projectile.Center;
        Projectile.Center += Projectile.velocity;
        Projectile.velocity = Vector2.Zero;
    }

    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;

        Vector2 velocity = Projectile.Center - _oldCenter;
        velocity += Vector2.UnitY * 0.2f;

        velocity = VerletString.TileCollision(Projectile.Center, velocity, true, Radius, -1f);

        _oldCenter = Projectile.Center;
        Projectile.Center = Projectile.Center + velocity;
    }
}