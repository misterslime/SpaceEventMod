using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Props;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items;

public class Debug : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 120;
        Item.height = 80;
        Item.useTime = 18;
        Item.useAnimation = 18;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5f;
        Item.value = 1000;
        Item.rare = ItemRarityID.Green;
    }

    public override bool? UseItem(Player player)
    {
        FastNoiseLite cellular = new FastNoiseLite();
        cellular.SetNoiseType(FastNoiseLite.NoiseType.Cellular);

        new Prop().AddComponent(new AsteroidNoiseSpawner(cellular, 0.7f, 10 * 16)).Register();

        new Prop().AddComponent(new Transformation(Main.MouseWorld, Vector2.Zero))
            .AddComponent(new Hitbox(160, 160))
            .AddComponent(new Health(1000, SoundID.Item70))
            .AddComponent(new Mineable())
            .AddComponent(new Grappleable())
            .AddComponent(new DirectionalShake(2, Vector2.UnitX, 0, 20))
            .AddComponent(new LowHealthFlashing(Color.Transparent))
            .AddComponent(new Sprite("SpaceEventMod/Assets/Textures/Props/Star", 1f, 0f, Vector2.Zero, Color.White, Main.rand.NextBool(2) ? SpriteEffects.None : SpriteEffects.FlipHorizontally))
            .AddComponent(new DespawnWithDistance(60f * 16f))
            .AddComponent(new Bobbing(10))
            .AddComponent(new BobbingRotation(5f))
            .AddComponent(new SeparatedSpawning())
            .Register();

        return true;
    }
}
