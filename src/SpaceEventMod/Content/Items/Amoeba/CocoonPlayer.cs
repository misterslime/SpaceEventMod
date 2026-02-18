using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.BackupIO;

namespace SpaceEventMod.Content.Items.Amoeba;

internal class CocoonPlayer : ModPlayer
{
    public bool AmoebicSet { get; set; }
    public bool Cocooned { get; set; }
    public int MyCocoon { get; set; }

    private int cocoonedManaCounter;

    public override void ResetEffects()
    {
        if (CanCocoon() && Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[0] < 15)
        {
            IEntitySource source = Player.GetSource_FromThis("SetBonus_AmoebicSet");
            int type = ModContent.NPCType<Cocoon>();

            MyCocoon = NPC.NewNPC(source, (int)Player.Center.X, (int)Player.Center.Y, type, Target: Player.whoAmI);
            Main.npc[MyCocoon].velocity = Player.velocity;
        }

        if (Player.controlJump && Cocooned)
        {
            Main.npc[MyCocoon].StrikeNPC(new NPC.HitInfo() { Damage = int.MaxValue });
        }

        AmoebicSet = false;
        Cocooned = false;
    }

    public override void UpdateDead()
    {
        AmoebicSet = false;
        Cocooned = false;
    }

    public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) => Cocooned;

    public override void UpdateLifeRegen()
    {
        if (Cocooned)
        {
            int num = 6;
            cocoonedManaCounter++;
            if (cocoonedManaCounter >= num)
            {
                cocoonedManaCounter = 0;
                Player.statMana++;
                if (Player.statMana >= Player.statManaMax2)
                {
                    Player.statMana = Player.statManaMax2;
                }
            }

            Player.lifeRegen += 16;
        }
        else
        {
            cocoonedManaCounter = 0;
        }
    }

    private bool CanCocoon()
    {
        return AmoebicSet && !Cocooned && !Player.HasBuff(ModContent.BuffType<CocoonCooldown>());
    }
}
