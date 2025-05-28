using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Props;
using SpaceEventMod.Core.Props.Components;
using System;
using Terraria;
using Terraria.ModLoader;


namespace SpaceEventMod.Content.Props;

public class Cosmostone : Prop
{
    public Cosmostone(Vector2 spawnPosition, int ID)
    {
        Transformation transform = new Transformation();
        transform.Position = spawnPosition;
        AddComponent(transform);

        Hitbox hitbox = new Hitbox();
        hitbox.Width = 120;
        hitbox.Height = 80;
        AddComponent(hitbox);

        DirectionalShake shake = new DirectionalShake();
        shake.MaxTime = 20;
        shake.Time = 0;
        shake.MaxStrength = 2;
        shake.UnitDirection = Vector2.UnitX;
        AddComponent(shake);

        Health health = new Health();
        health.Current = health.MaxHealth = 500;
        AddComponent(health);

        Mineable mineable = new Mineable();
        AddComponent(mineable);

        Collider collider = new Collider();
        AddComponent(collider);

        Grappleable grappleable = new Grappleable();
        AddComponent(grappleable);

        HealthFlashing healthFlashing = new HealthFlashing();
        healthFlashing.FlashColor = Color.Red;
        AddComponent(healthFlashing);

        Sprite sprite = new Sprite();
        sprite.SpritePath = "SpaceEventMod/Assets/Textures/Props/Cosmostone";
        sprite.SpriteDisplacement = Vector2.Zero;
        sprite.Rotation = 0f;
        sprite.Scale = 1f;
        AddComponent(sprite);

        this.ID = ID;
    }
}
