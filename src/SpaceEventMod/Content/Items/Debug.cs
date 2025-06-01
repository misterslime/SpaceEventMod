using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Props;
using SpaceEventMod.Core.Props.Components;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items;

public class Debug : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 120;
        Item.height = 80;
        Item.useTime = 18;
        Item.useAnimation = 18;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5f;
        Item.value = 1000;
        Item.rare = ItemRarityID.Green;
    }


    public override bool? UseItem(Player player)
    {
        int asteroidType = Main.rand.Next(6);

        switch (asteroidType)
        {
            case 0:
                NewAsteroid(Main.MouseWorld, 48, 16, "SpaceEventMod/Assets/Textures/Props/Asteroid3Small");
                break;

            case 1:
                NewAsteroid(Main.MouseWorld, 48, 32, "SpaceEventMod/Assets/Textures/Props/Asteroid3Medium");
                break;

            case 2:
                NewAsteroid(Main.MouseWorld, 48, 48, "SpaceEventMod/Assets/Textures/Props/Asteroid3Large");
                break;

            case 3:
                NewAsteroid(Main.MouseWorld, 64, 24, "SpaceEventMod/Assets/Textures/Props/Asteroid4Small");
                break;

            case 4:
                NewAsteroid(Main.MouseWorld, 64, 32, "SpaceEventMod/Assets/Textures/Props/Asteroid4Medium");
                break;

            case 5:
                NewAsteroid(Main.MouseWorld, 64, 48, "SpaceEventMod/Assets/Textures/Props/Asteroid4Large");
                break;
        }

        return true;
    }

    public void NewAsteroid(Vector2 spawnPosition, int width, int height, string spritePath)
    {
        // actually create the prop in the world
        new Prop().AddComponent(new Transformation(spawnPosition, Vector2.Zero))
            .AddComponent(new Hitbox(width, height))
            .AddComponent(new Health(200, SoundID.Item70))
            .AddComponent(new Collider(false))
            .AddComponent(new Mineable())
            .AddComponent(new Grappleable())
            .AddComponent(new DynamicMovement(1f / 128f, 0.5f, 0.2f, spawnPosition))
            .AddComponent(new FallWhenStoodOn(spawnPosition, spawnPosition + Vector2.UnitY * 48f))
            .AddComponent(new DirectionalShake(2, Vector2.UnitX, 0, 20))
            .AddComponent(new LowHealthFlashing(Color.Red))
            .AddComponent(new Sprite(spritePath, 1f, 0f, Vector2.Zero, Color.White, Main.rand.NextBool(2) ? SpriteEffects.None : SpriteEffects.FlipHorizontally))
            .Register();
    }
}
