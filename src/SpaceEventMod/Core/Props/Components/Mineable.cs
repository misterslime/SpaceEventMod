using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace SpaceEventMod.Core.Props.Components;

public class Mineable : Component
{
}

public class MiningSystem : ComponentSystem<Mineable>
{
    public override void Load()
    {
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += MineProp;
    }

    public override void Unload()
    {
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool -= MineProp;
    }

    private void MineProp(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y)
    {
        orig(self, sItem, out canHitWalls, x, y);

        if (self.whoAmI != Main.myPlayer)
            return;

        foreach (Mineable mineable in components)
        {
            if (mineable.GetComponent<Hitbox>().GetBoundingBox().Contains((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y))
            {
                Health health = mineable.GetComponent<Health>();

                health.Current -= sItem.pick;
                self.ApplyItemTime(sItem, self.pickSpeed * 1.5f);

                // shake when mining
                Vector2 propPosition = mineable.GetComponent<Hitbox>().GetCenter();
                mineable.GetComponent<DirectionalShake>().UnitDirection = propPosition - self.Center;
                mineable.GetComponent<DirectionalShake>().UnitDirection.Normalize();
                mineable.GetComponent<DirectionalShake>().Time = 20;

                // delete the prop if durability is now below 0
                if (health.Current <= 0)
                {
                    SoundEngine.PlaySound(SoundID.Item70, propPosition);
                    ComponentManager.QueuePropRemoval(mineable.prop);
                    return;
                }

                if (Main.myPlayer == self.whoAmI && health.Current > 0)
                    SoundEngine.PlaySound(SoundID.Tink, Main.MouseWorld);
            }
        }
    }
}
