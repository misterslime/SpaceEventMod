using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Geometry;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs.Sackteriums;

internal partial class Sackterium : ModNPC
{
    private ref float Timer => ref NPC.ai[1];

    public RotatedRectangle WindGustTrigger { get; private set; }

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
        Timer++;

        NPC.TargetClosest(false);

        if (!NPC.HasValidTarget)
            return;

        NPC.rotation = (Main.player[NPC.target].Center - NPC.Center).ToRotation();

        var rectangleDimensions = new Point(320, 160);
        var rectangleDisplacement = new Vector2(200, 0).RotatedBy(NPC.rotation).ToPoint();

        var rectanglePosition = NPC.Center.ToPoint() - (rectangleDimensions.ToVector2() * 0.5f).ToPoint() + rectangleDisplacement;
        var rectangle = new Rectangle(rectanglePosition.X, rectanglePosition.Y, rectangleDimensions.X, rectangleDimensions.Y);
        WindGustTrigger = new RotatedRectangle(rectangle, NPC.rotation);

        if (Timer % 6 != 0)
            return;

        SpawnWindGust(rectangleDimensions);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;

        var origin = new Vector2(34, 50) * 0.5f;
        var rotation = NPC.rotation + MathHelper.PiOver2;

        spriteBatch.Draw(texture, NPC.Center - screenPos, null, drawColor, rotation, origin, NPC.scale, 0, 0);

        return false;
    }
}
