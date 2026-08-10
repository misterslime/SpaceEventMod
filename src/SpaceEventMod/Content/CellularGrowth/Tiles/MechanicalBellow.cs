using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using SpaceEventMod.Content.CellularGrowth.Items;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Terraria.ModLoader.BackupIO;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class MechanicalBellow : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<MechanicalBellowTile>());
        Item.width = 30;
        Item.height = 22;
        Item.value = 3000;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<Windoplasts>(), 5)
            .AddIngredient(ItemID.LeadBar, 10)
            .AddTile(TileID.WorkBenches)
            .Register();

        CreateRecipe()
            .AddIngredient(ModContent.ItemType<Windoplasts>(), 5)
            .AddIngredient(ItemID.IronBar, 10)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public class BellowTileEntity : ModTileEntity
{
    public float Rotation { get; private set; }

    public int AnimationCounter { get; private set; }

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<MechanicalBellowTile>() && TileObjectData.IsTopLeft(x, y);
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        var position = TileObjectData.TopLeft(i, j);
        (i, j) = (position.X, position.Y);

        var tile = Framing.GetTileSafely(i, j);
        var data = TileObjectData.GetTileData(tile);

        var size = (data is null) ? new Point(1, 1) : new Point(data.Width, data.Height);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, size.X, size.Y);
            NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);

            return -1;
        }

        Rotation = 0;
        return Place(i, j);
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
        if (AnimationCounter < 100)
            AnimationCounter++;

        Dust.QuickDust(Position.X, Position.Y, Color.Red);
    }

    public void Interact(int i, int j, int item)
    {
        if (item == ModContent.ItemType<Windoplasts>())
        {
            Main.NewText("woosh!");
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

internal class MechanicalBellowTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;

        DustType = -1;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<BellowTileEntity>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(170, 91, 68));
    }

    private BellowTileEntity Entity(int i, int j)
    {
        var position = TileObjectData.TopLeft(i, j);
        (i, j) = (position.X, position.Y);
        
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
        const int frame_duration = 4;

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
