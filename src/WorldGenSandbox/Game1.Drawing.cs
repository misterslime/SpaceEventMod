using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using WorldGenSandbox.Creatures;
using WorldGenSandbox.Managers;

namespace WorldGenSandbox;

partial class Game1
{
    private void SubscribeDrawEvents(World world)
    {
        // order matters here!
        DrawManager.Instance.OnDraw += world.DrawWorld;
        DrawManager.Instance.OnDraw += DrawCreatures;


        OnClick += (object? sender, ClickEventArgs e) =>
        {
            _tentacle.DoFuckingThing(-0.25f);
        };

        return;
        OnClick += (object? sender, ClickEventArgs e) =>
        {
            int num = 5;

            for (int i = 0; i < num; i++)
            {
                Vector2 velocity = Globals.GenRand.NextVector2Unit();

                _creatureList.Add(new Dropling()
                {
                    Active = true,
                    Center = e.MouseWorld * 16 + velocity * 16,
                    Velocity = velocity
                });
            }
        };
    }

    private void DrawCreatures(object? sender, DrawEventArgs e)
    {
        foreach (var creature in _creatureList)
        {
            creature.Draw(e.SpriteBatch, e.Pixel);
        }

        _tentacle.Draw(e.SpriteBatch, e.Pixel, e.Transform);
    }
}
