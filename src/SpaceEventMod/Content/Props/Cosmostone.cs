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

        Mineable mineable = new Mineable();
        mineable.Durability = 500;
        AddComponent(mineable);

        Collider collider = new Collider();
        collider.Pinned = true;
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
        Vector2 shakeOffset = MathF.Sin(Main.GameUpdateCount) * shake.MaxStrength * ((float)shake.Time / (float)shake.MaxTime) * shake.UnitDirection;

        Main.EntitySpriteDraw(texture, drawPosition + shakeOffset, texture.Frame(), color, GetComponent<Transformation>().Rotation, origin, 1f, SpriteEffects.None);
    }
}
