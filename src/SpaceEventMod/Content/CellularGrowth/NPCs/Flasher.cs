using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Miscellaneous.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.BackupIO;

namespace SpaceEventMod.Content.CellularGrowth.NPCs;

public class Flasher : ModNPC
{
    public enum BehaviorState
    {
        Idle,
        Flashing,
        Escape
    }

    public override void SetStaticDefaults()
    {
        Main.npcCatchable[Type] = true;

        NPCID.Sets.CountsAsCritter[Type] = true;
        NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
        NPCID.Sets.ShimmerTransformToNPC[Type] = NPCID.Shimmerfly;
    }

    public override void SetDefaults()
    {
        NPC.width = 38;
        NPC.height = 34;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 5;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        int State = (int)BehaviorState.Idle;
        NPC.catchItem = ModContent.ItemType<FlasherItem>();
    }

    public override void AI()
    {

    }

}
    public class FlasherItem : ModItem
    {
        public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/NPCs/Flasher";
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 34;
            Item.maxStack = 9999;
            Item.value = Item.buyPrice(silver: 5);
            Item.rare = ItemRarityID.White;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = 2;
            Item.consumable = true;
            Item.shoot = ModContent.ProjectileType<FlasherProjectile>();
            Item.shootSpeed = 18f;
        }

    }

    //most of this is stolen from windoplast code
    public class FlasherProjectile : ModProjectile
    {

        private const int BURST_RADIUS = 100;
        public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/NPCs/Flasher";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.Explosive[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.aiStyle = 2;

            Projectile.timeLeft = 300;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.owner == Main.myPlayer)
                Projectile.PrepareBombToBlow();

            return true;
        }

        public override void AI()
        {
            if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
                Projectile.PrepareBombToBlow();

            // Projectile.rotation += Projectile.velocity.X * 0.01f;

            if (Projectile.owner != Main.myPlayer)
                return;

            // collide with npcs
            foreach (var npc in Main.ActiveNPCs)
            {
                if (!npc.Hitbox.Intersects(Projectile.Hitbox))
                    continue;

                Projectile.PrepareBombToBlow();
                Projectile.ai[0] = 1;
            }
        }
        public override void PrepareBombToBlow()
        {
            Projectile.timeLeft = 0;
            Projectile.alpha = 255;
            Projectile.netUpdate = true;
            Projectile.tileCollide = false;

        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item45, Projectile.position);
            SoundEngine.PlaySound(SoundID.NPCDeath11, Projectile.position);

            var spawnPos = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);

            spawnPos = Projectile.Center;

        //replace with a custom explosion
           
            Dust dust = Dust.NewDustDirect(spawnPos, 0, 0, ModContent.DustType<FlashbangDust>(), 0f, 0f, 100, default, 2f);
            dust.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 1.2f;
            dust.rotation = 0f;
            dust.fadeIn = 4 * Main.rand.Next(0, 3);
            

        foreach (var npc in Main.ActiveNPCs)
            {
                Vector2 flashVector = npc.Center - Projectile.Hitbox.Bottom();
                float distance = InvLerp(BURST_RADIUS * 0.5f, 0, flashVector.Length());

                if (Projectile.ai[0] == 1)
                    flashVector = Projectile.velocity;

                flashVector = flashVector.SafeNormalize(Vector2.Zero);
                flashVector -= Vector2.UnitY;
                flashVector = flashVector.SafeNormalize(Vector2.Zero);

                if (distance <= BURST_RADIUS)
                {
                    npc.AddBuff(ModContent.BuffType<Flashbang>(), 100); 

                }
                

            }
        }

        private float InvLerp(float a, float b, float v) => (v - a) / (b - a);


    }
 

