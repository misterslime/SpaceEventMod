using SpaceEventMod.Core.Props.Components;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Core.Props.Systems;

public class RenderingSystem : PropSystem<Rendering>
{
    public override void Load()
    {
        On_Main.DrawNPCs += DrawEverything;
    }

    public override void Unload()
    {
        On_Main.DrawNPCs -= DrawEverything;
    }

    private void DrawEverything(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        orig(self, behindTiles);

        foreach (var component in components.ToList())
        {
            component.OnRender?.Invoke();
        }
    }
}
