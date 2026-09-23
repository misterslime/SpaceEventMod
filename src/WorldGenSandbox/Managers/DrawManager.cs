using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using WorldGenSandbox.Creatures;

namespace WorldGenSandbox.Managers;

internal class DrawEventArgs(SpriteBatch spriteBatch, Texture2D pixel, Matrix transform) : EventArgs
{
    public SpriteBatch SpriteBatch { get; } = spriteBatch;
    public Texture2D Pixel { get; } = pixel;
    public Matrix Transform { get; } = transform;
}

internal class DrawManager : IDisposable
{
    public event EventHandler<DrawEventArgs> OnDraw;

    public static DrawManager Instance;

    private SpriteBatch _batch;
    private Texture2D _pixel;

    public DrawManager(GraphicsDevice gd, ContentManager content)
    {
        if (Instance is not null)
            throw new Exception("Cannot create a second draw manager.");

        _batch = new SpriteBatch(gd);
        _pixel = content.Load<Texture2D>("WhitePixel");

        Instance = this;
    }

    public void Dispose()
    {
        _pixel.Dispose();
        _batch.Dispose();
    }

    public void Draw(GraphicsDevice gd, Matrix transform, List<BaseCreature> creatures)
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

        OnDrawCalled(new DrawEventArgs(_batch, _pixel, transform));

        _batch.End();
    }

    protected virtual void OnDrawCalled(DrawEventArgs e)
    {
        OnDraw?.Invoke(this, e);
    }
}
