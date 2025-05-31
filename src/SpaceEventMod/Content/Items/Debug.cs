using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Physics;
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
        // create and define all necessary components
        Transformation transform = new Transformation();
        transform.Position = spawnPosition;

        Hitbox hitbox = new Hitbox();
        hitbox.Width = width;
        hitbox.Height = height;

        Health health = new Health();
        health.Current = health.MaxHealth = 200;
        health.DeathSound = SoundID.Item70;

        Collider collider = new Collider();
        collider.StoodOn = false;

        DynamicMovement dynamicMovement = new DynamicMovement();
        dynamicMovement.secondOrderSolver = new Vector2Dynamics(1f / 128f, 0.5f, 0.2f, spawnPosition);
        dynamicMovement.TargetPosition = spawnPosition;

        FallWhenStoodOn fallWhenStoodOn = new FallWhenStoodOn();
        fallWhenStoodOn.RestingPosition = spawnPosition;
        fallWhenStoodOn.FallPosition = spawnPosition + Vector2.UnitY * 48f;

        DirectionalShake shake = new DirectionalShake();
        shake.MaxTime = 20;
        shake.Time = 0;
        shake.MaxStrength = 2;
        shake.UnitDirection = Vector2.UnitX;

        HealthFlashing healthFlashing = new HealthFlashing();
        healthFlashing.FlashColor = Color.Red;

        Sprite sprite = new Sprite();
        sprite.SpritePath = spritePath;
        sprite.SpriteDisplacement = Vector2.Zero;
        sprite.Rotation = 0f;
        sprite.Scale = 1f;
        sprite.Effects = Main.rand.NextBool(2) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        // actually create the prop in the world
        new Prop().AddComponent(transform)
            .AddComponent(hitbox)
            .AddComponent(health)
            .AddComponent(collider)
            .AddComponent(new Mineable())
            .AddComponent(new Grappleable())
            .AddComponent(dynamicMovement)
            .AddComponent(fallWhenStoodOn)
            .AddComponent(shake)
            .AddComponent(healthFlashing)
            .AddComponent(sprite).Register();
    }
}
