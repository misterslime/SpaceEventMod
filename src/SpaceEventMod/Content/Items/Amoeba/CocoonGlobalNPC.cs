using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items.Amoeba;

internal class CocoonGlobalNPC : GlobalNPC
{
    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
    {
        if (target.type == ModContent.NPCType<Cocoon>())
        {
            var player = Main.player[target.target];

            var direction = -1;
            if (npc.position.X + (float)(npc.width / 2) < player.position.X + (float)(player.width / 2))
                direction = 1;

            var damage = Main.DamageVar(hit.Damage, 0f - player.luck);
            if (damage > 1000)
                damage = 1000;

            if (!npc.dontTakeDamage)
                player.ApplyDamageToNPC(npc, damage, hit.Knockback, -direction);
        }
    }
}
