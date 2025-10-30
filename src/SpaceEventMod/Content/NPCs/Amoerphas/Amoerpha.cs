using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs.Amoerphas;

internal class Amoerpha : ModNPC
{

    private List<Vector2> _arms = new List<Vector2>();
    private List<Vector2> _points = new List<Vector2>();

    private const int MAX_POINTS = 20;
    private const float MAX_SKELETON_LENGTH = 320f;

    private ref float Timer => ref NPC.ai[1];

    public override void SetDefaults()
    {
        NPC.width = 46;
        NPC.height = 42;
        NPC.damage = 50;
        NPC.defense = 16;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _points = new List<Vector2>();

        _points.Add(NPC.Center);
    }

    public override bool PreAI()
    {
        Timer++;

        NPC.Center = _points.Last();

        NPC.TargetClosest();

        float distanceMove = 24f;

        if (Timer % 12 == 0)
        {
            Vector2 newPoint = _points.Last();
            newPoint += (Main.player[NPC.target].Center - newPoint).RotatedByRandom(MathHelper.PiOver4 * 0.5f).SafeNormalize(Vector2.Zero) * distanceMove;

            _points.Add(newPoint);
        }

        if (_points.Count > MAX_POINTS && _points.Count != 0)
            _points.RemoveAt(0);

        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;

        int num = _points.Count;

        if (num <= 2)
            return false;

        for (int i = 0, j = 0; i < num; j = i, i++)
        {
            spriteBatch.DrawLine(_points[i] - Main.screenPosition, _points[j] - Main.screenPosition, Color.White, 2);
            spriteBatch.Draw(texture, _points[i] - Main.screenPosition, null, Color.White, 0f, texture.Size() * 0.5f, NPC.scale, 0, 0);
        }

        return false;
    }
}
