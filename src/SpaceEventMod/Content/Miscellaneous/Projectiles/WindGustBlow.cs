using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Miscellaneous.Dusts;
using SpaceEventMod.Core.Geometry;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Miscellaneous.Projectiles;

internal class WindGustBlow : ModProjectile
{
    private Queue _windDirections = new Queue();

    private int Owner => (int)Projectile.ai[0];
    private ref float Timer => ref Projectile.ai[1];
    private int Displacement => (int)Projectile.ai[2];

    public override string Texture => "SpaceEventMod/Assets/Textures/EmptyPixel";

    public RotatedRectangle WindGustTrigger { get; private set; }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 1;

        Projectile.friendly = true;
        Projectile.damage = 0;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;

        Projectile.alpha = 255;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _windDirections.Enqueue(new Point(-1, -1));
        _windDirections.Enqueue(new Point(1, -1));
        _windDirections.Enqueue(new Point(1, 1));
        _windDirections.Enqueue(new Point(-1, 1));
    }

    public override void AI()
    {
        if (Owner != -1)
        {
            Projectile.Center = Main.npc[Owner].Center;
            Projectile.velocity = Main.npc[Owner].velocity;
            Projectile.rotation = Main.npc[Owner].rotation;
        }

        Timer++;

        Point rectangleDimensions = new Point(320, 160);

        WindGustTrigger = GetWindTriggerBox(rectangleDimensions, Displacement, Projectile.rotation);

        if (Timer % 6 != 0)
            return;

        SpawnWindGust(rectangleDimensions);
    }

    private RotatedRectangle GetWindTriggerBox(Point rectangleDimensions, int displacement, float rotation)
    {
        var rectangleDisplacement = new Vector2(displacement, 0).RotatedBy(rotation).ToPoint();

        var rectanglePosition = Projectile.Center.ToPoint() - (rectangleDimensions.ToVector2() * 0.5f).ToPoint() + rectangleDisplacement;
        var rectangle = new Rectangle(rectanglePosition.X, rectanglePosition.Y, rectangleDimensions.X, rectangleDimensions.Y);

        return new RotatedRectangle(rectangle, rotation);
    }

    private void SpawnWindGust(Point rectangleDimensions)
    {
        var dustVelocityRectangle = new Rectangle(0, 0, rectangleDimensions.X, rectangleDimensions.Y);
        dustVelocityRectangle.X += (int)(rectangleDimensions.X * 0.5f);
        dustVelocityRectangle.Y -= (int)(rectangleDimensions.Y * 0.125f);
        dustVelocityRectangle.Width = (int)(dustVelocityRectangle.Width * 0.5f);
        dustVelocityRectangle.Height = (int)(dustVelocityRectangle.Height * 0.25f);

        var dustVelocity = Main.rand.NextVector2FromRectangle(dustVelocityRectangle);
        dustVelocity = dustVelocity / 12f;

        if (Owner != -1) 
        {
            if (Projectile.Center.X > Main.player[Main.npc[Owner].target].Center.X)
                dustVelocity *= -1;
        }

        var dustPosition = Projectile.Center;

        var color = Main.rand.NextFromList(
            (Color.White, Color.White),
            (Color.Gray, Color.White),
            (Color.White, Color.Gray));

        color.Item1.A = 0;
        color.Item2.A = 0;

        color.Item1 *= 0.8f;
        color.Item2 *= 0.8f;

        var direction = (Point)_windDirections.Dequeue();

        _windDirections.Enqueue(direction);

        var dust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<WindParticle>(), dustVelocity);
        dust.noGravity = true;
        dust.color = color.Item1;

        var curveAmount = Main.rand.NextFloat(0.15f, 0.25f);

        if (direction.X == direction.Y)
            curveAmount = Main.rand.NextFloat(0.2f, 0.3f);

        var width = Main.rand.NextFloat(2f, 8f) * 2.1f;
        var second = color.Item2;

        dust.customData = new WindParticleData(
            Projectile.whoAmI,
            second,
            30,
            direction,
            curveAmount,
            width);
        dust.fadeIn = 80;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        //Main.spriteBatch.DrawLine(WindGustTrigger.TopLeft() - Main.screenPosition, WindGustTrigger.BottomLeft() - Main.screenPosition, Color.White, 4);
        //Main.spriteBatch.DrawLine(WindGustTrigger.TopLeft() - Main.screenPosition, WindGustTrigger.TopRight() - Main.screenPosition, Color.White, 4);
        //Main.spriteBatch.DrawLine(WindGustTrigger.BottomLeft() - Main.screenPosition, WindGustTrigger.BottomRight() - Main.screenPosition, Color.White, 4);
        //Main.spriteBatch.DrawLine(WindGustTrigger.TopRight() - Main.screenPosition, WindGustTrigger.BottomRight() - Main.screenPosition, Color.White, 4);
        return base.PreDraw(ref lightColor);
    }

    private static Vector2 GetVelocityFromWind(Vector2 entityVelocity, Rectangle rectangle, float knockbackResist = 1f)
    {
        var windGusts = from gust in Main.projectile
                          where gust.active
                          where gust.type == ModContent.ProjectileType<WindGustBlow>()
                          select gust.ModProjectile as WindGustBlow;

        foreach (var gust in windGusts)
        {
            if (gust is null)
                continue;

            if (!gust.WindGustTrigger.Intersects(rectangle))
                continue;

            Vector2 windOrigin = Vector2.Lerp(gust.WindGustTrigger.TopLeft(), gust.WindGustTrigger.BottomLeft(), 0.5f);


            // Wind Physics Idea 1
            var windAcceleration = Vector2.UnitX.RotatedBy(gust.Projectile.rotation) * knockbackResist;

            entityVelocity *= 0.92f;
            entityVelocity.X += windAcceleration.X;
            entityVelocity.Y += windAcceleration.Y;
        }

        return entityVelocity;
    }

    [ModSystemHooks.PreUpdateNPCs]
    public static void NPCWindPhysics()
    {
        foreach (var npc in Main.ActiveNPCs)
            npc.velocity = GetVelocityFromWind(npc.velocity, npc.getRect(), npc.knockBackResist);
    }

    [ModSystemHooks.PreUpdatePlayers]
    public static void PlayerWindPhysics()
    {
        foreach (var player in Main.ActivePlayers)
            player.velocity = GetVelocityFromWind(player.velocity, player.getRect(), player.noKnockback ? 0 : 1);
    }

    [ModSystemHooks.PreUpdateProjectiles]
    public static void GolfBallWindPhysics()
    {
        int[] golfBalls = [
            ProjectileID.DirtGolfBall,
            ProjectileID.GolfBallDyedBlack,
            ProjectileID.GolfBallDyedBlue,
            ProjectileID.GolfBallDyedBrown,
            ProjectileID.GolfBallDyedCyan,
            ProjectileID.GolfBallDyedGreen,
            ProjectileID.GolfBallDyedLimeGreen,
            ProjectileID.GolfBallDyedOrange,
            ProjectileID.GolfBallDyedPink,
            ProjectileID.GolfBallDyedPurple,
            ProjectileID.GolfBallDyedRed,
            ProjectileID.GolfBallDyedSkyBlue,
            ProjectileID.GolfBallDyedTeal,
            ProjectileID.GolfBallDyedViolet,
            ProjectileID.GolfBallDyedYellow
            ];

        foreach (var projectile in Main.ActiveProjectiles)
        {
            if (!golfBalls.Contains(projectile.type))
                continue;

            projectile.velocity = GetVelocityFromWind(projectile.velocity, projectile.getRect(), 2f);
        }
    }
}
