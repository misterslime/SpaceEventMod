using Microsoft.Xna.Framework;
using Terraria.Utilities;

namespace WorldGenSandbox;

internal static class Globals
{
    public static float Time { get; set; }
    public static World World { get; set; }
    public static UnifiedRandom GenRand { get; set; }

    public static void Update(GameTime gt)
    {
        World.TryGenerate();
        Time = (float)gt.ElapsedGameTime.TotalSeconds;
    }
}
