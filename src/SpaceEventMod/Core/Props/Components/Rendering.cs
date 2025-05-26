using System.Linq;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

public class Rendering : Component
{
    public delegate void RenderingDelegate();

    public RenderingDelegate OnRender { get; set; } = null;

    public Rendering()
    {
        RenderingSystem.Register(this);
    }
}

public class RenderingSystem : ComponentSystem<Rendering>
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
