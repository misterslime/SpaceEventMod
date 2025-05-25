using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Props;
using SpaceEventMod.Core.Props.Components;
using System;
using Terraria.GameContent;
using Terraria;
using Microsoft.CodeAnalysis.Differencing;
using Terraria.ModLoader;
using SpaceEventMod.Core.Geometry;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Props.Systems;


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
        shake.MaxTime = 30;
        shake.Time = 0;
        shake.MaxStrength = 2;
        shake.UnitDirection = Vector2.UnitX;
        AddComponent(shake);

        Mineable mineable = new Mineable();
        mineable.Durability = 500;
        AddComponent(mineable);

        Collider collider = new Collider();
        collider.OnTestCollisionVector += Collision;
        AddComponent(collider);

        Rendering renderer = new Rendering();
        renderer.OnRender += Draw;
        AddComponent(renderer);

        this.ID = ID;
    }

    public void Draw()
    {
        Texture2D texture = ModContent.Request<Texture2D>("SpaceEventMod/Assets/Textures/Props/Cosmostone").Value;
        Vector2 drawPosition = GetComponent<Hitbox>().GetCenter() - Main.screenPosition;
        Vector2 origin = texture.Size() * 0.5f;

        float wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
        float lifeRatio = GetComponent<Mineable>().Durability / (float)500;
        Color color = Color.Lerp(Color.White, Color.Red, wave * EasingFunctions.CircEaseIn(1 - lifeRatio));

        DirectionalShake shake = GetComponent<DirectionalShake>();
        Vector2 shakeOffset = MathF.Sin(Main.GameUpdateCount) * shake.GetStrength() * shake.UnitDirection;

        Main.EntitySpriteDraw(texture, drawPosition + shakeOffset, texture.Frame(), color, GetComponent<Transformation>().Rotation, origin, 1f, SpriteEffects.None);
    }

    public Vector2? Collision(Prop closest, Vector2 position, int width, int height)
    {
        var closestHitbox = new Polygon(closest.GetComponent<Hitbox>().GetBoundingBox());

        var entityHitbox = new Polygon([
            position,
                position + new Vector2(width, 0),
                position + new Vector2(0, height),
                position + new Vector2(width, height),
            ]);

        var collision = CollisionHelper.TestCollisions(closestHitbox, Vector2.Zero, entityHitbox, position);

        return collision;
    }
}
