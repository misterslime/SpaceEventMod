using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Core.Geometry;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Biomes.DunesBiome;

namespace SpaceEventMod.Content.NPCs;

internal class Sackterium : ModNPC
{
    private ref float Timer => ref NPC.ai[1];

    public RotatedRectangle WindGustTrigger { get; private set; }

    public bool IsPushing { get; set; }

    private Queue _windDirections = new Queue();

    public override void SetDefaults()
    {
        NPC.width = 34;
        NPC.height = 50;
        NPC.damage = 0;
        NPC.defense = 16;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
        NPC.aiStyle = -1;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
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
        Timer++;

        NPC.TargetClosest(false);

        if (!NPC.HasValidTarget)
            return;

        NPC.rotation = (Main.player[NPC.target].Center - NPC.Center).ToRotation();

        Point rectangleDimensions = new Point(320, 160);
        Point rectangleDisplacement = new Vector2(200, 0).RotatedBy(NPC.rotation).ToPoint();

        Point rectanglePosition = NPC.Center.ToPoint() - (rectangleDimensions.ToVector2() * 0.5f).ToPoint() + rectangleDisplacement;
        Rectangle rectangle = new Rectangle(rectanglePosition.X, rectanglePosition.Y, rectangleDimensions.X, rectangleDimensions.Y);
        WindGustTrigger = new RotatedRectangle(rectangle, NPC.rotation);

        if (Timer % 12 != 0)
            return;

        Rectangle dustVelocityRectangle = new Rectangle(0, 0, rectangleDimensions.X, rectangleDimensions.Y);
        dustVelocityRectangle.X += (int)(rectangleDimensions.X * 0.5f);
        dustVelocityRectangle.Y -= (int)(rectangleDimensions.Y * 0.125f);
        dustVelocityRectangle.Width = (int)(dustVelocityRectangle.Width * 0.5f);
        dustVelocityRectangle.Height = (int)(dustVelocityRectangle.Height * 0.25f);

        Vector2 dustVelocity = Main.rand.NextVector2FromRectangle(dustVelocityRectangle);
        dustVelocity = dustVelocity / 12f;

        if (NPC.Center.X > Main.player[NPC.target].Center.X)
            dustVelocity *= -1;

        Vector2 dustPosition = NPC.Center;

        var color = Main.rand.NextFromList(
            (Color.White, Color.White),
            (Color.Gray, Color.White),
            (Color.White, Color.Gray));

        Point direction = (Point)_windDirections.Dequeue();

        _windDirections.Enqueue(direction);

        Dust dust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<WindParticle>(), dustVelocity);
        dust.noGravity = true;
        dust.color = color.Item1;

        float curveAmount = Main.rand.NextFloat(0.15f, 0.25f);

        if (direction.X == direction.Y)
            curveAmount = Main.rand.NextFloat(0.2f, 0.3f);

        float width = Main.rand.NextFloat(2f, 8f);
        Color second = color.Item2;

        dust.customData = new WindParticleData(
            NPC.whoAmI,
            second, 
            30, 
            direction,
            curveAmount, 
            width);
        dust.fadeIn = 80;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;
        
        var origin = new Vector2(34, 50) * 0.5f;
        var rotation = NPC.rotation + MathHelper.PiOver2;

        spriteBatch.Draw(texture, NPC.Center - screenPos, null, drawColor, rotation, origin, NPC.scale, 0, 0);

        return false;

        Vector2 topLeft = WindGustTrigger.TopLeft() - screenPos;
        Vector2 topRight = WindGustTrigger.TopRight() - screenPos;
        Vector2 bottomLeft = WindGustTrigger.BottomLeft() - screenPos;
        Vector2 bottomRight = WindGustTrigger.BottomRight() - screenPos;

        Color color = IsPushing ? Color.Green : Color.Red;

        spriteBatch.DrawLine(topLeft, topRight, color, 2);
        spriteBatch.DrawLine(topRight, bottomRight, color, 2);
        spriteBatch.DrawLine(bottomRight, bottomLeft, color, 2);
        spriteBatch.DrawLine(bottomLeft, topLeft, color, 2);

        IsPushing = false;
        return false;
    }
}

public class SackteriumWindPhysics : ModSystem
{
    private Vector2 GetVelocityFromWind(Vector2 entityVelocity, Rectangle rectangle)
    {
        var sackteriums = from sackterium in Main.npc
                          where sackterium.active
                          where sackterium.type == ModContent.NPCType<Sackterium>()
                          select sackterium.ModNPC as Sackterium;

        foreach (var sackterium in sackteriums)
        {
            if (!sackterium.WindGustTrigger.Intersects(rectangle))
                continue;

            sackterium.IsPushing = true;

            Vector2 windAcceleration = Vector2.UnitX.RotatedBy(sackterium.NPC.rotation);

            entityVelocity.X += windAcceleration.X;
            entityVelocity.Y += windAcceleration.Y;
        }

        return entityVelocity;
    }

    public override void PreUpdateNPCs()
    {
        foreach (var npc in Main.ActiveNPCs)
            npc.velocity = GetVelocityFromWind(npc.velocity, npc.getRect());
    }

    public override void PreUpdatePlayers()
    {
        foreach (var player in Main.ActivePlayers)
            player.velocity = GetVelocityFromWind(player.velocity, player.getRect());
    }
}
