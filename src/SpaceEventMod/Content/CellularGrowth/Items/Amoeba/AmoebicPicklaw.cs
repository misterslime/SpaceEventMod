using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Animation;
using SpaceEventMod.Common.Splines;
using SpaceEventMod.Common.WorldGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items.Amoeba;

internal class AmoebicPicklaw : ModItem
{
    public override void Load()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.HandsOn}", EquipType.Shield, this);
    }


    public override void SetDefaults()
    {
        Item.width = 52;
        Item.height = 50;
        Item.SetShopValues((Terraria.Enums.ItemRarityColor)ItemRarityID.Blue, Item.buyPrice(silver: 20));

        Item.channel = true;
        Item.shootSpeed = 12f;
        Item.shoot = ModContent.ProjectileType<AmoebicPicklawProjectile>();
        Item.UseSound = SoundID.Item7;
        Item.useTime = Item.useAnimation = 40;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.accessory = true;
    }
}

internal class ExamplePlayerDrawLayer : PlayerDrawLayer
{
    public override bool IsHeadLayer => false;

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.HeldItem?.type == ModContent.ItemType<AmoebicPicklaw>();

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var texture = Assets.Textures.CellularGrowth.Items.Amoeba.AmoebicPicklaw_HandsOn.Asset.Value;

        var position = new Vector2(
            (int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2),
            (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)
        ) + drawInfo.drawPlayer.bodyPosition + drawInfo.bodyVect;


        drawInfo.DrawDataCache.Add(new DrawData(
            texture,
            position,
            drawInfo.drawPlayer.bodyFrame,
            drawInfo.colorArmorBody,
            drawInfo.drawPlayer.fullRotation,
            drawInfo.bodyVect,
            1f,
            drawInfo.playerEffect,
            0));
    }
}

internal class AmoebicPicklawProjectile : ModProjectile
{
    private const int DEFAULT_WIDTH_HEIGHT = 26;
    private const int EXPLOSION_WIDTH_HEIGHT = 250;

    private static AnimationParameters s_picklawMovement = new AnimationParameters(4, 0.35f, 0);

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = DEFAULT_WIDTH_HEIGHT;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.aiStyle = -1;
    }

    public override bool PreAI()
    {
        if (!Projectile.TryGetOwner(out Player player))
        {
            Projectile.Kill();
            return false;
        }

        player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);
        player.heldProj = Projectile.whoAmI;
        player.itemTime = player.itemAnimation = 2;

        if (Main.myPlayer != player.whoAmI)
            return false;

        if (player.channel)
        {
            var targetPosition = Main.MouseWorld;
            var topLeft = Projectile.Hitbox.TopLeft().ToTileCoordinates();
            var bottomRight = Projectile.Hitbox.BottomRight().ToTileCoordinates();

            var targetCenter = new Vector2(targetPosition.X - (Projectile.width / 2), targetPosition.Y - (Projectile.height / 2));

            Projectile.Integrate(new AnimationParameters(2, 0.62f, 0), targetCenter);

            Projectile.netUpdate = true;
        }
        else
        {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.2f);
            Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center, 0.2f);

            if (player.Hitbox.Intersects(Projectile.Hitbox))
                Projectile.Kill();
        }

        return true;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        return true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Projectile.velocity.Length() <= 3f)
            return false;

        bool onGround = oldVelocity.X * Projectile.velocity.X != 0 && oldVelocity.Y * Projectile.velocity.Y != 0;

        if (onGround)
            SoundEngine.PlaySound(SoundID.Tink, Projectile.position);

        if (!Projectile.TryGetOwner(out Player player))
        {
            Projectile.Kill();
            return false;
        }

        if (!player.channel)
            return true;

        if (!onGround)
            return false;

        if (Projectile.owner == Main.myPlayer)
        {
            int searchRadius = 7;

            int minTileX = (int)(Projectile.Center.X / 16f - searchRadius);
            int maxTileX = (int)(Projectile.Center.X / 16f + searchRadius);
            int minTileY = (int)(Projectile.Center.Y / 16f - searchRadius);
            int maxTileY = (int)(Projectile.Center.Y / 16f + searchRadius);

            var nearest = GetNearestSolid(Projectile.Center, searchRadius, minTileX, maxTileX, minTileY, maxTileY);

            HashSet<Point> tiles = new();

            tiles.Add(nearest);

            GetConnectedTiles(tiles, nearest, searchRadius);

            foreach (var lemon in tiles)
            {
                var pickPower = 65;

                PickTile(player, lemon.X, lemon.Y, 55);
            }
        }

        return false;
    }

    #region evil copied and modified vanilla code
    public void PickTile(Player player, int x, int y, int pickPower)
    {
        Tile tile = Main.tile[x, y];
        if (tile.type == 504)
            return;

        PickTile_DetermineDamage(player, x, y, pickPower, tile, out var bufferIndex, out var damage);

        var addDamage = player.hitTile.AddDamage(bufferIndex, damage == 0 ? 0 : 65);

        if (addDamage >= 100)
        {
            AchievementsHelper.CurrentlyMining = true;
            player.ClearMiningCacheAt(x, y, 1);
            if (Main.netMode == 1 && Main.tileContainer[Main.tile[x, y].type])
            {
                if (Main.tile[x, y].type == 470 || Main.tile[x, y].type == 475)
                {
                    NetMessage.SendData(17, -1, -1, null, 20, x, y);
                }
                else
                {
                    WorldGen.KillTile(x, y, fail: true);
                    NetMessage.SendData(17, -1, -1, null, 0, x, y, 1f);
                }

                if (Main.tile[x, y].type == 21 || Main.tile[x, y].type < TileID.Count && TileID.Sets.BasicChest[Main.tile[x, y].type])
                    NetMessage.SendData(34, -1, -1, null, 1, x, y);

                if (Main.tile[x, y].type == 467)
                    NetMessage.SendData(34, -1, -1, null, 5, x, y);

                if (Main.tile[x, y].type == 88)
                    NetMessage.SendData(34, -1, -1, null, 3, x, y);

                if (Main.tile[x, y].type >= TileID.Count)
                {
                    if (TileID.Sets.BasicChest[Main.tile[x, y].type])
                        NetMessage.SendData(34, -1, -1, null, 101, x, y, 0f, 0, Main.tile[x, y].type, 0);

                    if (TileID.Sets.BasicDresser[Main.tile[x, y].type])
                        NetMessage.SendData(34, -1, -1, null, 103, x, y, 0f, 0, Main.tile[x, y].type, 0);
                }
            }
            else
            {
                bool flag = Main.tile[x, y].active();
                WorldGen.KillTile(x, y);
                if (!Main.dedServ && flag && !Main.tile[x, y].active())
                    AchievementsHelper.HandleMining();

                if (Main.netMode == 1)
                    NetMessage.SendData(17, -1, -1, null, 0, x, y);
            }

            AchievementsHelper.CurrentlyMining = false;
        }
        else
        {
            WorldGen.KillTile(x, y, fail: true);
            if (Main.netMode == 1)
            {
                NetMessage.SendData(17, -1, -1, null, 0, x, y, 1f);
                NetMessage.SendData(125, -1, -1, null, Main.myPlayer, x, y, damage);
            }
        }

        if (damage != 0)
            player.hitTile.Prune();
    }

    public void PickTile_DetermineDamage(Player player, int x, int y, int pickPower, Tile tileTarget, out int bufferIndex, out int damage)
    {
        bufferIndex = player.hitTile.HitObject(x, y, 1);
        damage = player.GetPickaxeDamage(x, y, pickPower, bufferIndex, tileTarget);
        if (!WorldGen.CanKillTile(x, y))
            damage = 0;

        if (Main.getGoodWorld)
            damage *= 2;

        if (player.DoesPickTargetTransformOnKill(player.hitTile, damage, x, y, pickPower, bufferIndex, tileTarget))
            damage = 0;
    }

    private Point GetNearestSolid(Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ)
    {
        var points = new List<Vector2>();

        for (int i = minI; i <= maxI; i++)
        {
            for (int j = minJ; j <= maxJ; j++)
            {
                float num = Math.Abs((float)i - compareSpot.X / 16f);
                float num2 = Math.Abs((float)j - compareSpot.Y / 16f);
                if (!(Math.Sqrt(num * num + num2 * num2) < (double)radius))
                    continue;

                Tile tile = Main.tile[i, j];
                if (tile != null && tile.active() && WorldGen.SolidOrSlopedTile(tile))
                    points.Add(new Point(i, j).ToWorldCoordinates());
            }
        }

        var nearest = points.OrderBy(x => Math.Abs((x - compareSpot).LengthSquared())).First().ToTileCoordinates();

        return nearest;
    }
    #endregion

    private void GetConnectedTiles(HashSet<Point> tiles, Point tilePosition, int depth)
    {
        if (depth < 0)
            return;

        int tileType = Main.tile[tilePosition].TileType;

        foreach (var point in TileDirections.WithCorners)
        {
            var position = point + tilePosition;

            if (!WorldGen.SolidOrSlopedTile(Main.tile[position]) || tiles.Contains(position) || Main.tile[position].TileType != tileType)
                continue;

            tiles.Add(position);

            GetConnectedTiles(tiles, position, depth - 1);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!Projectile.TryGetOwner(out Player player))
            return false;

        Texture2D texture = TextureAssets.Projectile[Type].Value;

        var handPosition = player.HandPosition.Value;

        var midControlPoint = Vector2.Lerp(handPosition, Projectile.Center, 0.7f);

        if (player.channel)
            midControlPoint += (Main.MouseWorld - Projectile.Center) * 0.45f;

        midControlPoint -= Main.screenPosition;

        var trailPoints = new List<Vector2>();

        ReadOnlySpan<Vector2> controlPoints = [handPosition - Main.screenPosition, midControlPoint, Projectile.Center - Main.screenPosition];
        using (var curve = new BezierCurve(controlPoints))
            trailPoints = curve.GetPoints(20);

        for (int i = 0; i < trailPoints.Count - 1; i++)
        {
            var sourceRect = (i % 3) switch
            {
                0 => new Rectangle(26, 28, 10, 10),
                1 => new Rectangle(14, 28, 10, 10),
                2 => new Rectangle(2, 28, 10, 10),
                _ => throw new ArgumentOutOfRangeException(),
            };

            Vector2 originDisplace = Vector2.Zero;

            if (i == trailPoints.Count - 2)
            {
                sourceRect = new Rectangle(38, 28, 10, 12);
                originDisplace = -Vector2.UnitX * 2f;
            }

            //Main.spriteBatch.DrawLine(trailPoints[i], trailPoints[i + 1], color, 4);

            Vector2 begin = trailPoints[i];
            Vector2 end = trailPoints[i + 1];

            var r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + sourceRect.Height, sourceRect.Height);
            var v = Vector2.Normalize(begin - end);
            var angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            var col = Lighting.GetColor((begin + Main.screenPosition).ToTileCoordinates());
            Main.spriteBatch.Draw(texture, r, sourceRect, col, angle, Vector2.Zero + originDisplace, SpriteEffects.None, 0);
        }

        var rotationDir = (trailPoints[trailPoints.Count - 1] - trailPoints[trailPoints.Count - 2]).SafeNormalize(Vector2.Zero);
        var velocityDir = Projectile.velocity.LengthSquared() > 1f ? Projectile.velocity.SafeNormalize(Vector2.Zero) : Projectile.velocity;
        var realDir = Vector2.Lerp(velocityDir, rotationDir, 0.7f).SafeNormalize(Vector2.Zero);


        float thing = (int)(trailPoints[trailPoints.Count - 1] - trailPoints[trailPoints.Count - 2]).Length() + 12;

        Rectangle frame = new(0, 0, 48, 26);

        Projectile.rotation = rotationDir.ToRotation();
        Projectile.rotation += MathHelper.PiOver2;

        Vector2 drawOrigin = new Vector2(18, 10);

        Main.EntitySpriteDraw(texture, trailPoints.Last() + thing * rotationDir, frame, lightColor, Projectile.rotation, drawOrigin, 1f, SpriteEffects.None);

        return false;
    }
}