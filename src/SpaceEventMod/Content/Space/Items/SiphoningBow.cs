using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.Items;

internal class SiphoningBow : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 38;
        Item.height = 76;
        Item.rare = ItemRarityID.Green;
        Item.value = Item.buyPrice(silver: 20);

        //Item.channel = true;
        Item.autoReuse = true;
        Item.useTime = Item.useAnimation = 20;
        Item.UseSound = SoundID.Item5;
        Item.useStyle = ItemUseStyleID.Shoot;
        //Item.useTurn = true;

        Item.shootSpeed = 10f;
        Item.shoot = ProjectileID.WoodenArrowFriendly;
        Item.useAmmo = AmmoID.Arrow;

        Item.damage = 16;
        Item.DamageType = DamageClass.Ranged;
        Item.knockBack = 4f;
        Item.noMelee = true;
    }
}

public class SiphoningBowArrows : GlobalProjectile
{
    private bool _siphonArrow = false;
    private int _type = 0; // 0 = red, 1 = yellow, 2 = blue

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.friendly && entity.DamageType == DamageClass.Ranged;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        _siphonArrow = false;

        if (source is EntitySource_ItemUse_WithAmmo { Item: Item item } && item != null && item.ModItem is SiphoningBow)
        {
            _siphonArrow = true;
            _type = Main.rand.Next(0, 3);
        }
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        if (!_siphonArrow)
            return true;

        Texture2D tex = Assets.Textures.Space.Items.SiphoningBowArrows.Asset.Value;
        Rectangle frame = tex.Frame(3, 1, _type, 0);
        Vector2 origin = frame.Center() - frame.Location.ToVector2();

        Main.EntitySpriteDraw(tex, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, origin, projectile.scale, 0, 0);

        return false;
    }
}
