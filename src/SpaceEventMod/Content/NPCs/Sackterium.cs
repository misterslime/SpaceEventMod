using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Core.Geometry;
using SpaceEventMod.Core.Utilities.Extensions;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs;

internal class Sackterium : ModNPC
{
    public RotatedRectangle WindGustTrigger { get; private set; }

    public bool IsPushing { get; set; }

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

    public override void AI()
    {
        NPC.TargetClosest(false);

        if (!NPC.HasValidTarget)
            return;

        NPC.rotation = (Main.player[NPC.target].Center - NPC.Center).ToRotation();

        Point rectangleDimensions = new Point(320, 160);
        Point rectangleDisplacement = new Vector2(200, 0).RotatedBy(NPC.rotation).ToPoint();

        Point rectanglePosition = NPC.Center.ToPoint() - (rectangleDimensions.ToVector2() * 0.5f).ToPoint() + rectangleDisplacement;
        Rectangle rectangle = new Rectangle(rectanglePosition.X, rectanglePosition.Y, rectangleDimensions.X, rectangleDimensions.Y);
        WindGustTrigger = new RotatedRectangle(rectangle, NPC.rotation);

        if (!Main.rand.NextBool(5))
            return;

        Rectangle dustVelocityRectangle = new Rectangle(0, 0, rectangleDimensions.X, rectangleDimensions.Y);
        dustVelocityRectangle.X += (int)(rectangleDimensions.X * 0.5f);
        dustVelocityRectangle.Y -= (int)(rectangleDimensions.Y * 0.125f);
        dustVelocityRectangle.Width = (int)(dustVelocityRectangle.Width * 0.5f);
        dustVelocityRectangle.Height = (int)(dustVelocityRectangle.Height * 0.25f);

        Vector2 dustVelocity = Main.rand.NextVector2FromRectangle(dustVelocityRectangle);
        dustVelocity = dustVelocity.RotatedBy(NPC.rotation) / 12f;

        Vector2 dustPosition = NPC.Center;

        Dust dust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<WindParticle>(), dustVelocity);
        dust.noGravity = true;
        dust.color = Main.rand.NextBool() ? Color.White : Color.Gray;

        float curveAmount = Main.rand.NextFloat(0.05f, 0.2f);
        int direction = Main.rand.NextBool() ? 1 : -1;
        int startDirection = Main.rand.NextBool() ? 1 : -1;
        float width = Main.rand.NextFloat(1f, 7f);
        Color second = Main.rand.NextBool() ? Color.White : Color.Gray;

        dust.customData = new WindParticleData(second, 30, direction, startDirection, curveAmount, width);
        dust.fadeIn = 140;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;
        
        var origin = new Vector2(34, 50) * 0.5f;
        var rotation = NPC.rotation + MathHelper.PiOver2;

        spriteBatch.Draw(texture, NPC.Center - screenPos, null, drawColor, rotation, origin, NPC.scale, 0, 0);

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
