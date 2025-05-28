using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Props;
using SpaceEventMod.Core.Props.Components;
using System;
using Terraria;
using Terraria.ModLoader;


namespace SpaceEventMod.Content.Props;

public class Asteroid : Prop
{
    public Asteroid(Vector2 spawnPosition, int width, int height, string spritePath, int ID)
    {
        Transformation transform = new Transformation();
        transform.Position = spawnPosition;
        AddComponent(transform);

        Hitbox hitbox = new Hitbox();
        hitbox.Width = width;
        hitbox.Height = height;
        AddComponent(hitbox);

        Health health = new Health();
        health.Current = health.MaxHealth = 200;
        AddComponent(health);

        Mineable mineable = new Mineable();
        AddComponent(mineable);

        Collider collider = new Collider();
        AddComponent(collider);

        Grappleable grappleable = new Grappleable();
        AddComponent(grappleable);

        DynamicMovement dynamicMovement = new DynamicMovement();
        dynamicMovement.secondOrderSolver = new Vector2Dynamics(1f / 128f, 0.5f, 0.2f, spawnPosition);
        dynamicMovement.TargetPosition = spawnPosition;
        AddComponent(dynamicMovement);

        FallWhenStoodOn fallWhenStoodOn = new FallWhenStoodOn();
        fallWhenStoodOn.RestingPosition = spawnPosition;
        fallWhenStoodOn.FallPosition = spawnPosition + Vector2.UnitY * 48f;
        AddComponent(fallWhenStoodOn);

        DirectionalShake shake = new DirectionalShake();
        shake.MaxTime = 20;
        shake.Time = 0;
        shake.MaxStrength = 2;
        shake.UnitDirection = Vector2.UnitX;
        AddComponent(shake);

        HealthFlashing healthFlashing = new HealthFlashing();
        healthFlashing.FlashColor = Color.Red;
        AddComponent(healthFlashing);

        Sprite sprite = new Sprite();
        sprite.SpritePath = spritePath;
        sprite.SpriteDisplacement = Vector2.Zero;
        sprite.Rotation = 0f;
        sprite.Scale = 1f;
        AddComponent(sprite);

        this.ID = ID;
    }
}
