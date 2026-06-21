using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Utilities;
using WorldGenSandbox.Managers;

namespace WorldGenSandbox;

// a lot of this is copied from this tutorial
// https://fna-xna.github.io/docs/2b%3A-Building-New-Games-with-FNA/
class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;

    private DrawManager _drawManager;
    private CameraManager _cameraManager;

    public Game1()
    {
        Window.Title = "The Sandbox";

        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            IsFullScreen = false,
            SynchronizeWithVerticalRetrace = true
        };

        IsMouseVisible = true;

        Content.RootDirectory = "Assets";
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        Globals.Time = 0f;
        Globals.World = new World(4200, 1200); // small world size
        Globals.GenRand = new UnifiedRandom();

        _drawManager = new DrawManager(GraphicsDevice, Content);
        _cameraManager = new CameraManager(_graphics.GraphicsDevice.Viewport);
    }

    protected override void UnloadContent()
    {
        _drawManager.Dispose();
    }

    protected override void Update(GameTime gameTime)
    {
        _cameraManager.UpdateCamera(_graphics.GraphicsDevice.Viewport);

        Globals.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _drawManager.Draw(GraphicsDevice, _cameraManager.Transform);
        base.Draw(gameTime);
    }
}