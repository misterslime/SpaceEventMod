using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Animation;
using SpaceEventMod.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static tModPorter.ProgressUpdate;

namespace SpaceEventMod.Content.Dusts;

internal struct WindParticleData(Color secondColor, int maxOldPositions, int direction, int startDirection, float curveAmount, float width)
{
    public Color SecondColor { get; } = secondColor;
    public Vector2[] OldPositions { get; init; } = new Vector2[maxOldPositions];
    public int Direction { get; } = direction;
    public int StartDirection { get; } = startDirection;
    public float CurveAmount { get; } = curveAmount;
    public float Width { get; } = width;
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

        float curveAmount = (dust.fadeIn <= 120) ? data.CurveAmount * data.Direction : 0.02f * data.StartDirection;

        dust.fadeIn--;
        if (dust.fadeIn >= 60)
        {
            dust.velocity *= 0.965f;
            dust.velocity = dust.velocity.RotatedBy(curveAmount).RotatedByRandom(curveAmount);

            dust.position += dust.velocity;
        }
        else if (dust.fadeIn <= 0)
            dust.active = false;

        dust.customData = UpdatePositions(dust, in data);

        return false;
    }

    private WindParticleData UpdatePositions(Dust dust, in WindParticleData data)
    {
        WindParticleData newData = data;

        for (int i = data.OldPositions.Length - 2; i >= 0; i--)
            newData.OldPositions[i + 1] = data.OldPositions[i];

        newData.OldPositions[0] = dust.position;

        return newData;
    }

    public override bool PreDraw(Dust dust)
    {
        if (dust.customData == null || dust.customData is not WindParticleData data)
        {
            return false;
        }

        var positions = from position in data.OldPositions
                        where !Equals(position, default(Vector2))
                        select position;

        if (positions.Count() < 2)
            return false;

        var trailPoints = new List<Vector2>();

        ReadOnlySpan<Vector2> controlPoints = positions.ToArray();
        using (var curve = new BezierCurve(controlPoints))
            trailPoints = curve.GetPoints(20);

        Graphics.BeginPipeline(0.5f)
            .DrawTrail(
                trailPoints.ToArray(),
                progress => MathF.Sin(progress * MathHelper.Pi) * data.Width,
                progress => Color.Lerp(dust.color, data.SecondColor, progress),
                Assets.Assets.Shaders.Trail.BendyTexture.Value,
                ("transformMatrix", Graphics.WorldTransformMatrix),
                ("sampleTexture", Assets.Assets.Textures.WhitePixel.Value),
                ("frame", new Vector4(0, 0, 1, 1)))
            .Schedule(RenderLayer.AfterPlayers);

        return false;
    }
}