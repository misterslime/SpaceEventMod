using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.LevelElements;

public class StarMining : ILoadable
{
    public void Load(Mod mod) => On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += MineStars;

    public void Unload() => On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool -= MineStars;

    private void MineStars(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y)
    {
        orig(self, sItem, out canHitWalls, x, y);

        if (self.whoAmI != Main.myPlayer)
            return;

        for (var i = 0; i < Stars.List.Count; i++)
        {
            var hitStar = false;
            var destroyStar = false;

            Stars.List[i] = MineStar(Stars.List[i], self, sItem, x, y, out hitStar, out destroyStar);
            Stars.List[i].UpdateSubscribedNPCs();

            if (hitStar)
            {
                SoundEngine.PlaySound(SoundID.Tink, Main.MouseWorld);
                self.ApplyItemTime(sItem, self.pickSpeed * 1.5f);

                Stars.List[i].InformSubscribedNPCs((npc) =>
                {
                    npc.target = self.whoAmI;

                    npc.targetRect = Main.player[self.whoAmI].getRect();
                });
            }

            if (destroyStar)
            {
                SoundEngine.PlaySound(SoundID.Item70, Stars.List[i].GetCenter());
                Stars.List.RemoveAt(i);
                i--;
            }

            if (hitStar || destroyStar)
            {
                canHitWalls = false;
                return;
            }
        }
    }

    private Star MineStar(Star star, Player self, Item sItem, int x, int y, out bool hitStar, out bool destroyStar)
    {
        hitStar = false;
        destroyStar = false;

        var newStar = star;

        if (star.GetBoundingBox().Contains((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y))
        {
            newStar.Durability -= sItem.pick;

            // shake when mining
            var starPosition = star.GetCenter();

            newStar.ShakeDirection = starPosition - self.Center;
            newStar.ShakeDirection.Normalize();
            newStar.ShakeTime = 20;

            hitStar = true;

            // delete the prop if durability is now below 0
            if (star.Durability <= 0)
                destroyStar = true;
        }

        return newStar;
    }
}
