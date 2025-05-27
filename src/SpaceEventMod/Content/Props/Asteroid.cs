using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Geometry;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Props;
using SpaceEventMod.Core.Props.Components;
using System;
using Terraria;
using Terraria.ModLoader;


namespace SpaceEventMod.Content.Props;

public class Asteroid : Prop
{
    public Asteroid(Vector2 spawnPosition, int ID)
    {
        Transformation transform = new Transformation();
        transform.Position = spawnPosition;
        AddComponent(transform);

        Hitbox hitbox = new Hitbox();
        hitbox.Width = 64;
        hitbox.Height = 48;
        AddComponent(hitbox);

        DirectionalShake shake = new DirectionalShake();
        shake.MaxTime = 20;
        shake.Time = 0;
        shake.MaxStrength = 2;
        shake.UnitDirection = Vector2.UnitX;
        AddComponent(shake);

        Mineable mineable = new Mineable();
        mineable.Durability = 200;
        AddComponent(mineable);

        Collider collider = new Collider();
        AddComponent(collider);

        DynamicMovement dynamicMovement = new DynamicMovement();
        dynamicMovement.secondOrderSolver = new Vector2Dynamics(1f / 128f, 0.5f, 0.2f, spawnPosition);
        dynamicMovement.TargetPosition = spawnPosition;
        AddComponent(dynamicMovement);

        FallWhenStoodOn fallWhenStoodOn = new FallWhenStoodOn();
        fallWhenStoodOn.RestingPosition = spawnPosition;
        fallWhenStoodOn.FallPosition = spawnPosition + Vector2.UnitY * 48f;
        AddComponent(fallWhenStoodOn);

        Rendering renderer = new Rendering();
        renderer.OnRender += Draw;
        AddComponent(renderer);

        this.ID = ID;
    }

    public void Draw()
    {
        Texture2D texture = ModContent.Request<Texture2D>("SpaceEventMod/Assets/Textures/Props/Asteroid").Value;
        Vector2 drawPosition = GetComponent<Transformation>().Position - Main.screenPosition;
        Vector2 origin = Vector2.Zero;

        float wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
        float lifeRatio = GetComponent<Mineable>().Durability / (float)200;
        Color color = Color.Lerp(Color.White, Color.Red, wave * EasingFunctions.CircEaseIn(1 - lifeRatio));

        DirectionalShake shake = GetComponent<DirectionalShake>();
        Vector2 shakeOffset = MathF.Sin(Main.GameUpdateCount) * shake.MaxStrength * ((float)shake.Time / (float)shake.MaxTime) * shake.UnitDirection;

        Main.EntitySpriteDraw(texture, drawPosition + shakeOffset, texture.Frame(), color, GetComponent<Transformation>().Rotation, origin, 1f, SpriteEffects.None);
    }
}
