using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Dusts;
using System.Collections;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs.Sackteriums;
internal partial class Sackterium
{
    private Queue _windDirections = new Queue();

    public override void OnSpawn(IEntitySource source)
    {
        _windDirections.Enqueue(new Point(-1, -1));
        _windDirections.Enqueue(new Point(1, -1));
        _windDirections.Enqueue(new Point(1, 1));
        _windDirections.Enqueue(new Point(-1, 1));
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

        if (NPC.Center.X > Main.player[NPC.target].Center.X)
            dustVelocity *= -1;

        var dustPosition = NPC.Center;

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
            NPC.whoAmI,
            second,
            30,
            direction,
            curveAmount,
            width);
        dust.fadeIn = 80;
    }
}
