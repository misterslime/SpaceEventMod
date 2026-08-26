using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Graphics;
using SpaceEventMod.Common.Splines;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Miscellaneous.Dusts;

internal struct WindParticleData(
    int projectile,
    Color secondColor, 
    int maxOldPositions,
    Point direction,
    float curveAmount, 
    float width)
{
    public int Projectile { get; } = projectile;
    public Color SecondColor { get; } = secondColor;
    public Vector2[] OldPositions { get; set; } = new Vector2[maxOldPositions];
    public Point Direction { get; } = direction;
    public float CurveAmount { get; } = curveAmount;
    public float Width { get; set; } = width;
}

internal class WindParticle : ModDust
{
    public override string Texture => "SpaceEventMod/Assets/Textures/EmptyPixel";

    public override bool Update(Dust dust)
    {
        if (dust.customData == null || dust.customData is not WindParticleData data)
        {
            dust.active = false;
            return false;
        }

        int steps = 2;

        for (int i = 0; i < steps; i++)
        {
            var curveAmount = dust.fadeIn <= 60 ? data.CurveAmount * data.Direction.X : 0.02f * data.Direction.Y;

            dust.velocity *= 0.985f;
            dust.velocity = dust.velocity.RotatedBy(curveAmount).RotatedByRandom(curveAmount);

            dust.position += dust.velocity;

            dust.fadeIn--;
            if (dust.fadeIn <= 30)
            {
                dust.velocity *= 0.965f;
                data = Shorten(dust, in data);
            }

            if (dust.fadeIn <= 0)
                dust.active = false;

            dust.customData = UpdatePositions(dust, in data);
        }

        return false;
    }

    private WindParticleData Shorten(Dust dust, in WindParticleData data)
    {
        if (data.OldPositions.Length <= 2)
            return data;

        var newData = data;

        var positions = newData.OldPositions;

        Array.Resize(ref positions, positions.Length - 1);

        newData.OldPositions = positions;

        return newData;
    }

    private WindParticleData UpdatePositions(Dust dust, in WindParticleData data)
    {
        if (data.OldPositions.Length < 2)
            return data;

        var newData = data;

        for (var i = data.OldPositions.Length - 2; i >= 0; i--)
            newData.OldPositions[i + 1] = data.OldPositions[i];

        newData.OldPositions[0] = dust.position;

        if (dust.fadeIn <= 30)
            newData.Width *= 0.95f;

        return newData;
    }

    public override bool PreDraw(Dust dust)
    {
        if (dust.customData == null || dust.customData is not WindParticleData data)
        {
            return false;
        }

        var projectile = Main.projectile[data.Projectile];

        var lerpAmount = 0.03f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8) * 0.03f;

        var positions = from position in data.OldPositions
                        where !Equals(position, default(Vector2))
                        select MapPosition(position, dust, projectile, lerpAmount);

        if (positions.Count() < 2)
            return false;

        var trailPoints = new List<Vector2>();

        ReadOnlySpan<Vector2> controlPoints = positions.ToArray();
        using (var curve = new BezierCurve(controlPoints))
            trailPoints = curve.GetPoints(20);

        Graphics.BeginPipeline(0.5f)
            .DrawBasicTrail(
                trailPoints.ToArray(),
                progress => MathF.Sin(progress * MathHelper.Pi) * data.Width,
                Assets.Textures.Trails.WindTrail.Asset.Value,
                progress => Color.Lerp(dust.color, data.SecondColor, progress))
            .Schedule(RenderLayer.AfterPlayers);

        return false;
    }

    private Vector2 MapPosition(Vector2 position, Dust dust, Projectile projectile, float lerpAmount)
    {
        var rotation = projectile.rotation;

        if (dust.position.X < projectile.Center.X)
            rotation += MathF.PI;

        position -= projectile.Center;

        position = Vector2.Lerp(position, Vector2.Zero, lerpAmount);

        position = position.RotatedBy(rotation);

        position += projectile.Center;

        return position;
    }
}