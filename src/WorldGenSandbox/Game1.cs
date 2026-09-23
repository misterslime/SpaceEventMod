using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Utilities;
using WorldGenSandbox.Creatures;
using WorldGenSandbox.Managers;

namespace WorldGenSandbox;

internal class ClickEventArgs(Vector2 mouseWorld) : EventArgs
{
    public Vector2 MouseWorld { get; } = mouseWorld;
}

// a lot of this is copied from this tutorial
// https://fna-xna.github.io/docs/2b%3A-Building-New-Games-with-FNA/
partial class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;

    private DrawManager _drawManager;
    private CameraManager _cameraManager;

    private List<BaseCreature> _creatureList;

    public List<BaseCreature> Creatures { get => _creatureList; }

    private Vector2 _mouseWorld;
    private MouseState _previousState;
    private Angelatin _tentacle;

    public Vector2 MouseWorld { get => _mouseWorld * 16f; }

    public static Game1 Instance {  get; private set; }

    public EventHandler<ClickEventArgs> OnClick;

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

        Instance = this;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _drawManager = new DrawManager(GraphicsDevice, Content);
        _cameraManager = new CameraManager(_graphics.GraphicsDevice.Viewport);

        Globals.Time = 0f;
        Globals.World = new World(500, 400); // small world size
        Globals.GenRand = new UnifiedRandom();

        _creatureList = new List<BaseCreature>();
        _previousState = Mouse.GetState();

        _tentacle = new Angelatin(5, 10);

        SubscribeDrawEvents(Globals.World);
    }

    protected override void UnloadContent()
    {
        _drawManager.Dispose();
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            this.Exit();

        _cameraManager.UpdateCamera(_graphics.GraphicsDevice.Viewport);

        Globals.Update(gameTime);

        foreach(var creature in _creatureList.ToArray())
        {
            if (!creature.Active)
                _creatureList.Remove(creature);

            creature.AI();
            creature.Center += creature.Velocity;
        }

        var mouseState = Mouse.GetState();

        _mouseWorld = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(_cameraManager.Transform));

        if (mouseState.LeftButton == ButtonState.Pressed
            && _previousState.LeftButton == ButtonState.Released
            && this.IsActive
            && mouseState.X >= 0 && mouseState.X < _graphics.PreferredBackBufferWidth
            && mouseState.Y >= 0 && mouseState.Y < _graphics.PreferredBackBufferHeight)
        {
            OnClick?.Invoke(this, new ClickEventArgs(_mouseWorld));
        }

        _previousState = mouseState;

        _tentacle.Anchor = _mouseWorld;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _drawManager.Draw(GraphicsDevice, _cameraManager.Transform, _creatureList);
        base.Draw(gameTime);
    }
}