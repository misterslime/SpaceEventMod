using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Miscellaneous.Projectiles;
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

namespace SpaceEventMod.Content.CellularGrowth.NPCs.Sackteriums;

internal partial class Sackterium : ModNPC
{
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
        NPC.TargetClosest(false);

        if (!NPC.HasValidTarget)
            return;

        NPC.rotation = (Main.player[NPC.target].Center - NPC.Center).ToRotation();

        if ((int)NPC.ai[0] < 0 || (int)NPC.ai[0] >= Main.maxProjectiles)
            return;

        if (Main.projectile[(int)NPC.ai[0]].type != ModContent.ProjectileType<WindGustBlow>() || !Main.projectile[(int)NPC.ai[0]].active)
            NPC.ai[0] = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.velocity, ModContent.ProjectileType<WindGustBlow>(), 0, 0, -1, NPC.whoAmI, 0, 200);

    }

    public override void OnKill()
    {
        if ((int)NPC.ai[0] < 0 || (int)NPC.ai[0] >= Main.maxProjectiles)
            return;

        if (Main.projectile[(int)NPC.ai[0]].type != ModContent.ProjectileType<WindGustBlow>() || !Main.projectile[(int)NPC.ai[0]].active)
            return;

        Main.projectile[(int)NPC.ai[0]].Kill();
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;

        var origin = new Vector2(34, 50) * 0.5f;
        var rotation = NPC.rotation + MathHelper.PiOver2;

        spriteBatch.Draw(texture, NPC.Center - screenPos, null, drawColor, rotation, origin, NPC.scale, 0, 0);

        return false;
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

        var dust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<WindGust>(), dustVelocity);
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

    [ModSystemHooks.PreUpdateNPCs]
    private static void NPCWind()
    {
        foreach (var npc in Main.ActiveNPCs)
            npc.velocity = GetVelocityFromWind(npc.velocity, npc.getRect());
    }

    [ModSystemHooks.PreUpdatePlayers]
    private static void PlayerWind()
    {
        foreach (var player in Main.ActivePlayers)
            player.velocity = GetVelocityFromWind(player.velocity, player.getRect());
    }

    private static Vector2 GetVelocityFromWind(Vector2 entityVelocity, Rectangle rectangle)
    {
        var sackteriums = from sackterium in Main.npc
                          where sackterium.active
                          where sackterium.type == ModContent.NPCType<Sackterium>()
                          select sackterium.ModNPC as Sackterium;

        foreach (var sackterium in sackteriums)
        {
            if (!sackterium.WindGustTrigger.Intersects(rectangle))
                continue;

            var windAcceleration = Vector2.UnitX.RotatedBy(sackterium.NPC.rotation);

            entityVelocity.X += windAcceleration.X;
            entityVelocity.Y += windAcceleration.Y;
        }

        return entityVelocity;
    }
}
