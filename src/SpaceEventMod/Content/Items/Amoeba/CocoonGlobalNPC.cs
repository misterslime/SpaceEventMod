using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items.Amoeba;

internal class CocoonGlobalNPC : GlobalNPC
{
    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
    {
        if (target.type == ModContent.NPCType<Cocoon>())
        {
            Player player = Main.player[target.target];

            int direction = -1;
            if (npc.position.X + (float)(npc.width / 2) < player.position.X + (float)(player.width / 2))
                direction = 1;

            int damage = Main.DamageVar(hit.Damage, 0f - player.luck);
            if (damage > 1000)
                damage = 1000;

            if (!npc.dontTakeDamage)
                player.ApplyDamageToNPC(npc, damage, hit.Knockback, -direction);
        }
    }
}
