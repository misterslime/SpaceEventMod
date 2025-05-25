using Terraria.ModLoader;
using Terraria;
using SpaceEventMod.Core.Props.Components;
using System.Linq;

namespace SpaceEventMod.Core.Props.Systems;

public class MiningSystem : PropSystem<Mineable>
{
    public override void Load()
    {
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += MineMeteoroid;
    }

    public override void Unload()
    {
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool -= MineMeteoroid;
    }

    private void MineMeteoroid(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y)
    {
        orig(self, sItem, out canHitWalls, x, y);

        if (self.whoAmI != Main.myPlayer)
            return;

        foreach (Mineable mineable in components.ToList())
        {
            if (mineable.IsHitting(Main.MouseWorld.X, Main.MouseWorld.Y))
                mineable.OnHit(self, sItem);
        }
    }
}
