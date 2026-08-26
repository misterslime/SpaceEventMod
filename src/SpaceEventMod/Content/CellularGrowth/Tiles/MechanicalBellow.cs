using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.CellularGrowth.Items;
using SpaceEventMod.Content.Miscellaneous.Projectiles;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using TileHelper.Common;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

public class BellowTileEntity : ModTileEntity
{
    private const int ANIMATION_LENGTH = 96;

    public float Rotation { get; private set; }

    public int AnimationCounter { get; private set; }

    private int _windProjectile;

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<MechanicalBellow>() && TileObjectData.IsTopLeft(x, y);
    }

    // Tile Entities can store data. This data most likely needs to be synced to connected clients.
    public override void SaveData(TagCompound tag)
    {
        tag[nameof(AnimationCounter)] = AnimationCounter;
        tag[nameof(Rotation)] = Rotation;
    }

    public override void LoadData(TagCompound tag)
    {
        AnimationCounter = tag.GetInt(nameof(AnimationCounter));
        Rotation = tag.GetFloat(nameof(Rotation));
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(AnimationCounter);
        writer.Write(Rotation);
    }

    public override void NetReceive(BinaryReader reader)
    {
        AnimationCounter = reader.ReadInt32();
        Rotation = reader.ReadSingle();
    }

    public override void Update()
    {
        if (AnimationCounter < ANIMATION_LENGTH)
            AnimationCounter++;
        
        if (_windProjectile != -1 && AnimationCounter >= ANIMATION_LENGTH)
        {
            Main.projectile[_windProjectile].Kill();
            _windProjectile = -1;
        }

        //debug indicators
        /*Dust.QuickDust(Position.X, Position.Y, Color.Red);

        Vector2 dustPosition = new Vector2(Position.X, Position.Y).ToWorldCoordinates(17, 15);
        dustPosition += (Vector2.UnitX * 13).RotatedBy(Rotation);

        Dust.QuickDust(dustPosition, Color.Yellow);*/
    }

    public void Interact(int i, int j, int item)
    {
        if (AnimationCounter < ANIMATION_LENGTH)
            return;

        if (item == ModContent.ItemType<Windoplasts>())
        {
            Vector2 position = new Vector2(Position.X, Position.Y).ToWorldCoordinates(17, 15);
            //position += (Vector2.UnitX * 13).RotatedBy(Rotation);

            _windProjectile = Projectile.NewProjectile(new EntitySource_Wiring(i, j), position, Vector2.Zero, ModContent.ProjectileType<WindGustBlow>(), 0, 0, -1, -1, 0, 160);

            Main.projectile[_windProjectile].rotation = Rotation;

            AnimationCounter = 0;
            SyncTileEntity();
            return;
        }

        if (Position.X - i == 0)
            Rotation -= MathHelper.PiOver4 / 2;
        else
            Rotation += MathHelper.PiOver4 / 2;

        Rotation = Rotation % MathHelper.TwoPi;
        SyncTileEntity();
    }

    public void SyncTileEntity()
    {
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
    }
}

internal class MechanicalBellow : ModTile, ILoadItem
{
    public void SetItemDefaults(ModItem modItem) => modItem.Item.value = 3000;

    public void AddItemRecipes(ModItem modItem)
    {
        modItem.CreateRecipe()
            .AddIngredient(ModContent.ItemType<Windoplasts>(), 5)
            .AddIngredient(ItemID.LeadBar, 10)
            .AddTile(TileID.WorkBenches)
            .Register();

        modItem.CreateRecipe()
            .AddIngredient(ModContent.ItemType<Windoplasts>(), 5)
            .AddIngredient(ItemID.IronBar, 10)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;

        DustType = -1;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<BellowTileEntity>().Generic_HookPostPlaceMyPlayer;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(170, 91, 68));
    }

    private BellowTileEntity Entity(int i, int j)
    {
        if (TileEntity.TryGet(i, j, out BellowTileEntity tileEntity))
            return tileEntity;

        return null;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!effectOnly && !fail && Entity(i, j) is BellowTileEntity entity)
            entity.Kill(i, j);
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        if (Entity(i, j) is BellowTileEntity entity)
            entity.Kill(i, j);
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        if (Entity(i, j) is BellowTileEntity entity)
            entity.SyncTileEntity();
    }

    public override void MouseOver(int i, int j)
    {
        Terraria.Player player = Main.LocalPlayer;

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<Windoplasts>();
    }

    public override bool RightClick(int i, int j)
    {
        if (Entity(i, j) is BellowTileEntity entity)
        {
            entity.Interact(i, j, Main.LocalPlayer.HeldItem.type);
            return true;
        }

        return false;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (TileObjectData.IsTopLeft(i, j))
        {
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
            return true;
        }
        return true;
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Texture2D texture = TextureAssets.Tile[Type].Value;

        Tile tile = Main.tile[i, j];
        bool drawingTop = tile.TileFrameX == 0 && tile.TileFrameY == 0; //Bottom right corner

        Rectangle source = new(tile.TileFrameX, tile.TileFrameY, 16, 16);
        Vector2 position = new Vector2(i, j) * 16 - Main.screenPosition;

        spriteBatch.Draw(texture, position, source, Lighting.GetColor(i, j), 0, Vector2.Zero, 1, default, 0);

        if (drawingTop && Entity(i, j) is BellowTileEntity entity)
        {
            int frame = (int)((entity.AnimationCounter / 4) % 8);

            Rectangle billowSource = new(0, 36 + 36 * frame, 52, 36);
            Vector2 billowPosition = new Vector2(i, j).ToWorldCoordinates(17, 15) - Main.screenPosition;
            //Vector2 billowOrigin = billowSource.Size() / 2;
            Vector2 billowOrigin = new Vector2(37, 17);

            float rotation = entity.Rotation;

            spriteBatch.Draw(texture, billowPosition, billowSource, Lighting.GetColor(i, j), rotation, billowOrigin, 1, default, 0);
        }
    }
}
