using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using System;
using Terraria;
using WorldGenSandbox.Physics;
using WorldGenSandbox.Utilities;

namespace WorldGenSandbox.Creatures;


internal class Angelatin
{
    public Vector2 Anchor { get; set; }

    private int _segments;
    private float _segmentLength;
    private float _time;
    private Vector2 _velocity;
    private Vector2 _position;

    private Tentacle _leftTentacle;
    private Tentacle _rightTentacle;

    private Vector2 _spring;


    public Angelatin(int segments, float segmentLength)
    {
        _segments = segments;
        _segmentLength = segmentLength;
        _time = 0;
        _velocity = Vector2.Zero;
        _position = Vector2.Zero;
        _spring = Vector2.Zero;


        _leftTentacle = new Physics.Tentacle(_position, segments, _segmentLength, MathHelper.PiOver2);

        var segment = _leftTentacle[0];
        segment.Locked = true;
        _leftTentacle[0] = segment;

        _leftTentacle.Gravity = Vector2.UnitY * 0.85f;


        _rightTentacle = new Physics.Tentacle(_position, segments, _segmentLength, MathHelper.PiOver2);

        segment = _rightTentacle[0];
        segment.Locked = true;
        _rightTentacle[0] = segment;

        _rightTentacle.Gravity = Vector2.UnitY * 0.85f;
    }

    public void DoFuckingThing(float aaa)
    {
        _spring.Y += aaa;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, Matrix transform)
    {

        var headDimensions = new Vector2(56, 40);
        var dressDimensions = new Vector2(40, 14);
        var segmentDimensions = new Vector2(4, 10);
        var starDimensions = new Vector2(14, 12);

        var toTarget = (Anchor - (_position + _velocity)).SafeNormalize(Vector2.Zero);
        _velocity += toTarget;
        _velocity = _velocity.Clamp(10f);
        _position += _velocity;

        var rotation = -_velocity.X * 0.01f;


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


        int tentacles = 2;
        float baseLineLength = 16f;
        float wavelength = 2f * MathF.PI / 5f;
        float speed = 0.015f * MathF.PI;
        float amplitude = 0.15f;
        var color = Color.Blue;

        Vector2 baseLineStart = new Vector2(baseLineLength * -0.5f, dressDimensions.Y * 0.75f);

        // tentacle position
        //Vector2 position = (baseLineStart + Vector2.UnitX * ((1f + (float)0) / ((float)tentacles + 1f)) * baseLineLength).RotatedBy(-rotation * 0.5f);


        Matrix inverse = translate;

        _leftTentacle.AnchorStart = _position + (baseLineStart + Vector2.UnitX * ((1f + (float)0) / ((float)tentacles + 1f)) * baseLineLength).RotatedBy(-rotation * 0.5f);
        WaveMotion(_leftTentacle, 0, tentacles, wavelength, speed, amplitude, _velocity);
        _leftTentacle.Update(8, 0.25f);

        _rightTentacle.AnchorStart = _position + (baseLineStart + Vector2.UnitX * ((1f + (float)1) / ((float)tentacles + 1f)) * baseLineLength).RotatedBy(-rotation * 0.5f);
        WaveMotion(_rightTentacle, 1, tentacles, wavelength, speed, amplitude, _velocity);
        _rightTentacle.Update(8, 0.25f);

        spriteBatch.End();
        spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            effect: null,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: DepthStencilState.None,
            rasterizerState: RasterizerState.CullCounterClockwise,
            transformMatrix: shear * wobble * translate * transform);




        /*int tentacles = 2;
        float wavelength = 4f * MathF.PI / 5f;
        float speed = 0.015f * MathF.PI;
        float amplitude = 0.4f;
        float baseLineLength = 16f;
        var color = Color.Blue;

        Vector2 baseLineStart = new Vector2(baseLineLength * -0.5f, dressDimensions.Y);
        var rotation = -_velocity.X * 0.01f;*/

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

                spriteBatch.Draw(pixel, end, null, Color.Gray, angle + MathHelper.PiOver2, segmentOrigin, scale, 0, 0);

                position = end;

                if (i != _segments - 1)
                    continue;

                spriteBatch.Draw(pixel, position, null, Color.DarkGray, angle - MathHelper.PiOver2, pixel.Size() * 0.5f, starDimensions, 0, 0);
            }
        }

        Matrix invert = Matrix.Invert(translate);

        DrawTentacle(spriteBatch, _leftTentacle, pixel, invert);
        DrawTentacle(spriteBatch, _rightTentacle, pixel, invert);

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

    public void DrawTentacle( SpriteBatch spriteBatch, Tentacle tentacle, Texture2D pixel, Matrix invert)
    {
        var segmentDimensions = new Vector2(4, _segmentLength);
        var starDimensions = new Vector2(14, 12);

        for (int i = 0; i < tentacle.Count; i++)
        {
            var currentPosition = tentacle[i].Position;

            if (i == tentacle.Count - 1)
            {
                var previousPosition = tentacle[i - 1].Position;

                float starAngle = tentacle[i].Position.DirectionFrom(previousPosition).ToRotation();

                spriteBatch.Draw(pixel, Vector2.Transform(currentPosition, invert), null, Color.Yellow, starAngle - MathHelper.PiOver2, pixel.Size() * 0.5f, starDimensions, 0, 0);

                continue;
            }

            var nextPosition = tentacle[i + 1].Position;

            float angle = nextPosition.DirectionTo(currentPosition).ToRotation();

            // texture scaling
            Vector2 scale = new Vector2(segmentDimensions.X, currentPosition.Distance(nextPosition));
            Vector2 segmentOrigin = pixel.Size() * new Vector2(0.5f, 0f);

            spriteBatch.Draw(pixel, Vector2.Transform(nextPosition, invert), null, Color.Blue, angle - MathHelper.PiOver2, segmentOrigin, scale, 0, 0);
        }
    }

    public void WaveMotion(Tentacle tentacle, int tentacleIndex, int tentacles, float wavelength, float speed, float amplitude, Vector2 tentacleVel)
    {
        // tentacle movement displacement
        float animDisplacement = (2f * ((float)tentacleIndex / (float)(tentacles - 1f)) - 1f);

        // progress and start angle
        float tentProgress = ((float)tentacleIndex / (float)tentacles);

        for (int i = 0; i < tentacle.Count - 1; i++)
        {
            var next = tentacle[i + 1];

            float progress = Ease(i / (float)tentacle.Count);

            // tentacle sine wave
            float transverse = (2f / wavelength) * (MathF.PI * progress - _time * speed) - tentProgress * MathF.PI * 0.5f;
            float newRot = Math.Clamp(tentacleVel.Y * 0.015f, -1f, 1f) * animDisplacement * (1f - progress);

            float angularAcceleration = animDisplacement * amplitude * (wavelength * MathF.Sin(transverse) + 2 * MathF.PI * progress * MathF.Cos(transverse));
            angularAcceleration /= wavelength;
            angularAcceleration += newRot;

            // add up angles
            // angle += wave * animDisplacement * amplitude;

            var angle = (next.Position - tentacle[i].Position).PerpendicularClockwise().SafeNormalize(Vector2.Zero);

            var linearAcceleration = angularAcceleration * angle;

            next.Acceleration -= linearAcceleration;
            tentacle[i + 1] = next;
        }
    }

    float Ease(float x) => 1f - MathF.Pow(1f - x, 3);
}
