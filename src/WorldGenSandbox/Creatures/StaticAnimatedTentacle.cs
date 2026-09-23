using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using System;
using Terraria;
using WorldGenSandbox.Utilities;

namespace WorldGenSandbox.Creatures;


internal class StaticAnimatedTentacle(int segments, float segmentLength)
{
    public Vector2 Anchor { get; set; }

    private int _segments = segments;
    private float _segmentLength = segmentLength;
    private float _time = 0;
    private Vector2 _velocity = Vector2.Zero;
    private Vector2 _position = Vector2.Zero;

    private Vector2 _spring = Vector2.Zero;

    public void DoFuckingThing(float aaa)
    {
        _spring.Y += aaa;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, Matrix transform)
    {
        var toTarget = (Anchor - (_position + _velocity)).SafeNormalize(Vector2.Zero);
        _velocity += toTarget;
        _velocity = _velocity.Clamp(10f);
        _position += _velocity;

        // transform matrix

        var translate = Matrix.CreateTranslation(_position.X, _position.Y, 0f);

        float dampening = 0.045f;
        float tension = 0.08f;
        float springScale = 0.5f;

        var acceleration = -tension * _spring.X - dampening * _spring.Y;

        _spring.Y += acceleration;
        _spring.X += _spring.Y;

        var shear = Matrix.Identity with
        {
            M12 = _velocity.X * _spring.X * 0.125f * springScale,
            M21 = _velocity.X * _spring.X * 0.02f * springScale
        };

        var wobble = Matrix.CreateScale(1f - _spring.X * springScale, 1f + _spring.X * springScale, 1f);

        spriteBatch.End();
        spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            effect: null,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: DepthStencilState.None,
            rasterizerState: RasterizerState.CullCounterClockwise,
            transformMatrix: shear * wobble * translate * transform);

        var headDimensions = new Vector2(56, 40);
        var dressDimensions = new Vector2(40, 14);
        var segmentDimensions = new Vector2(4, 10);
        var starDimensions = new Vector2(14, 12);
        int tentacles = 2;
        float wavelength = 4f * MathF.PI / 5f;
        float speed = 0.015f * MathF.PI;
        float amplitude = 0.4f;
        float baseLineLength = 16f;
        var color = Color.Blue;

        Vector2 baseLineStart = new Vector2(baseLineLength * -0.5f, dressDimensions.Y);
        var rotation = -_velocity.X * 0.01f;

        for (int tentacle = 0; tentacle < tentacles; tentacle++)
        {
            // tentacle movement displacement
            float animDisplacement = (2f * ((float)tentacle / (float)(tentacles - 1f)) - 1f);

            // tentacle position
            Vector2 position = baseLineStart + Vector2.UnitX * ((1f + (float)tentacle) / ((float)tentacles + 1f)) * baseLineLength;
            position = position.RotatedBy(-rotation * 0.5f);
            //position += _position;

            // progress and start angle
            float tentProgress = ((float)tentacle / (float)tentacles);
            float angle = MathHelper.PiOver2 + -rotation * 0.5f;

            for (int i = 0; i < _segments; i++)
            {
                float progress = i / (float)_segments;

                // tentacle sine wave
                float segmentWave = (2f / wavelength) * (MathF.PI * progress - _time * speed);
                float wave = MathF.Sin(segmentWave + tentProgress * MathF.PI  * 0.5f) * progress;

                // add up angles
                angle += wave * animDisplacement * amplitude;

                Vector2 end = position + angle.ToRotationVector2() * _segmentLength;

                // texture scaling
                Vector2 scale = new Vector2(segmentDimensions.X, _segmentLength);
                Vector2 segmentOrigin = pixel.Size() * new Vector2(0.5f, 0f);

                spriteBatch.Draw(pixel, end, null, Color.Blue, angle + MathHelper.PiOver2, segmentOrigin, scale, 0, 0);

                position = end;

                if (i != _segments - 1)
                    continue;

                spriteBatch.Draw(pixel, position, null, Color.Yellow, angle - MathHelper.PiOver2, pixel.Size() * 0.5f, starDimensions, 0, 0);
            }
        }

        var origin = pixel.Size() * new Vector2(0.5f, 1f);

        spriteBatch.Draw(pixel, Vector2.Zero, null, Color.BlueViolet, -rotation * 0.66f, origin with { Y = 0 }, dressDimensions, 0, 0);
        spriteBatch.Draw(pixel, Vector2.Zero, null, Color.Blue, rotation, origin, headDimensions, 0, 0);

        spriteBatch.End();
        spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            effect: null,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: DepthStencilState.None,
            rasterizerState: RasterizerState.CullCounterClockwise,
            transformMatrix: transform);

        _time++;
    }
}
