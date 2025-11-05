using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Mechanics.SmoothParticleHydrodynamics;
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
    private FluidSimulation _simulation;

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
        _simulation = new FluidSimulation();

        _simulation.Activate(NPC.Center);
    }

    public override bool PreAI()
    {
        Timer++;

        _simulation.Update();

        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Item[ItemID.FallenStar].Value;

        Vector2 toMouse = Main.MouseWorld - NPC.Center;
        toMouse = toMouse.SafeNormalize(Vector2.Zero);

        float startLength = texture.Width * 1.2f;

        Vector2 start = NPC.Center + startLength * toMouse - Main.screenPosition;
        Vector2 end = Main.MouseWorld - startLength * toMouse - Main.screenPosition;

        Rectangle frame = texture.Frame(1, 8, 0, 0);
        Vector2 origin = new Vector2(texture.Width, texture.Height / 8) * 0.5f;

        spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, frame, Color.White, 0f, origin, 1f, 0, 0);
        spriteBatch.Draw(texture, Main.MouseWorld - Main.screenPosition, frame, Color.White, 0f, origin, 0.75f, 0, 0);
        spriteBatch.DrawLine(start, end, Color.Yellow, 2);

        if (_simulation is null)
            return false;

        _simulation.Draw(spriteBatch);

        return false;
    }
}
