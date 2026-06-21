using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldGenSandbox.Managers;

internal class DrawManager(GraphicsDevice gd, ContentManager content) : IDisposable
{
    private SpriteBatch _batch = new SpriteBatch(gd);
    private Texture2D _pixel = content.Load<Texture2D>("WhitePixel");

    public void Dispose()
    {
        _pixel.Dispose();
    }

    public void Draw(GraphicsDevice gd, Matrix transform)
    {
        gd.Clear(Color.Black);

        if (!Globals.World.Generated)
            return;

        _batch.Begin(
            sortMode: SpriteSortMode.Deferred,
            effect: null,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: DepthStencilState.None,
            rasterizerState: RasterizerState.CullCounterClockwise,
            transformMatrix: transform);

        _batch.Draw(_pixel, new Rectangle(0, 0, Globals.World.MaxTilesX, Globals.World.MaxTilesY), Color.DarkBlue * 0.5f);

        Dictionary<TileTypes, Color> colors = new Dictionary<TileTypes, Color>();

        Color wallColor = Color.Gray * 0.6f;
        wallColor.A = 255;

        colors.Add(TileTypes.Empty, Color.Black);
        colors.Add(TileTypes.Cosmostone, Color.Gray);
        colors.Add(TileTypes.CosmostoneWall, wallColor);
        colors.Add(TileTypes.Cosmoss, Color.LightCoral);
        colors.Add(TileTypes.HerbCell, Color.Turquoise);
        colors.Add(TileTypes.Stone, Color.White);
        colors.Add(TileTypes.Mud, new Color(92, 68, 73));
        colors.Add(TileTypes.SlimeMold, Color.Yellow);

        for (int i = 0; i < Globals.World.MaxTilesX; ++i)
        {
            for (int j = 0; j < Globals.World.MaxTilesY; ++j)
            {
                if (Globals.World.Tiles[i, j] == TileTypes.Empty)
                    continue;

                _batch.Draw(_pixel, new Vector2(i, j), colors[Globals.World.Tiles[i, j]]);
            }
        }

        _batch.Draw(_pixel, new Rectangle(0, 0, Globals.World.MaxTilesX, 40), Color.White * 0.25f);
        _batch.Draw(_pixel, new Rectangle(0, 0, 40, Globals.World.MaxTilesY), Color.White * 0.25f);
        _batch.Draw(_pixel, new Rectangle(0, Globals.World.MaxTilesY - 40, Globals.World.MaxTilesX, 40), Color.White * 0.25f);
        _batch.Draw(_pixel, new Rectangle(Globals.World.MaxTilesX - 40, 0, 40, Globals.World.MaxTilesY), Color.White * 0.25f);

        _batch.End();
    }
}
