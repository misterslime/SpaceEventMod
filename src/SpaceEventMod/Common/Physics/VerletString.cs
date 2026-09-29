using Daybreak.Common.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Transactions;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.IO;
using static tModPorter.ProgressUpdate;

namespace SpaceEventMod.Common.Physics;

// feature ideas:
// rotation constraints
// spring constraints instead of rigid constraints?
// replace locking with bias?
// ability to apply linear and angular forces?
// ability to make any shape instead of exclusively ropes/strings
// make this inverse kinematics-able if possible
// player/entity collision?
internal class VerletString
{
    private float _segmentLength;
    private int _numSegments;
    private float _segmentMass;
    private Vector2[] _positions;
    private Vector2[] _oldPositions;
    private Vector2[] _accelerations;
    private bool[] _locked;

    // segment mass only affects wind speed btw
    public VerletString(Vector2 startPos, int numSegments, float segmentLength, float startRotation, float segmentMass = 1f)
    {
        Debug.Assert(segmentMass != 0);

        _segmentLength = segmentLength;
        _positions = new Vector2[numSegments];
        _oldPositions = new Vector2[numSegments];
        _accelerations = new Vector2[numSegments];
        _locked = new bool[numSegments];
        _numSegments = numSegments;
        _segmentMass = segmentMass;

        var direction = startRotation.ToRotationVector2();
        var position = startPos;

        for (int i = 0; i < numSegments; i++)
        {
            _positions[i] = position;
            _oldPositions[i] = position;

            position += direction * segmentLength;
        }
    }

    public Vector2 AnchorStart { get => _positions[0]; set => _positions[0] = value; }
    public Vector2 AnchorEnd { get => _positions[_positions.Length - 1]; set => _positions[_positions.Length - 1] = value; }
    public Vector2 Gravity { get; set; }

    public int Count { get => _numSegments; }
    public ReadOnlySpan<Vector2> Positions { get => _positions; }


    public (Vector2 Position, Vector2 OldPosition, Vector2 Acceleration, bool Locked) this[int i]
    {
        get => (_positions[i], _oldPositions[i], _accelerations[i], _locked[i]);
        set
        {
            _positions[i] = value.Position;
            _oldPositions[i] = value.OldPosition;
            _accelerations[i] = value.Acceleration;
            _locked[i] = value.Locked;
        }
    }

    public void Lock(int index)
    {
        _locked[index] = true;
    }

    public void Unlock(int index)
    {
        _locked[index] = false;
    }

    public void Update(int steps, float damping = 0f, bool collideWithTiles = false, bool windAffected = false)
    {
        float velocityMult = 1f - damping;

        // integrate
        for (int i = 0; i < _positions.Length; i++)
        {
            if (_locked[i])
                continue;

            _accelerations[i] += Gravity;

            if (windAffected)
            {
                var point = _positions[i].ToTileCoordinates();

                float windStrength = Main.instance.TilesRenderer.GetWindCycle(point.X, point.Y, Main.instance.TilesRenderer._grassWindCounter);

                if (!Main.instance.TilesRenderer.InAPlaceWithWind(point.X, point.Y, 1, 1))
                    windStrength = 0f;

                Main.instance.TilesRenderer.GetWindGridPush2Axis(point.X, point.Y, 20, 0.35f, out var pushX, out var pushY);

                // following 4 lines r from slr
                var phase = Main.windCounter * 0.12f;
                var frequency = 0.01f;
                var amplitude = 0.8f;
                pushX += (Main.windSpeedCurrent + MathF.Sin(phase + _positions[i].X * frequency) * Main.windSpeedCurrent * amplitude);

                _accelerations[i] += new Vector2(pushX, pushY) / _segmentMass;
            }

            var oldPosition = _positions[i];
            var velocity = _positions[i] - _oldPositions[i];
            velocity += _accelerations[i];
            velocity *= velocityMult; // dampen movement

            if (collideWithTiles)
                velocity = TileCollision(_positions[i], velocity, false);

            _positions[i] += velocity;
            _oldPositions[i] = oldPosition;
            _accelerations[i] = Vector2.Zero;
        }

        // apply distance constraints
        for (int j = 0; j < steps; j++)
        {
            for (int i = 1; i < _positions.Length - 1; i++)
            {
                ConstrainPoints(i, i - 1, collideWithTiles);
                ConstrainPoints(i + 1, i, collideWithTiles);
            }
        }

        // apply angular constraints
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ConstrainPoints(int i1, int i2, bool tileCollide)
    {
        var vectorFrom = _positions[i1] - _positions[i2];
        float distance = Vector2.Distance(_positions[i1], _positions[i2]);
        float signedDistance = 0;

        if (distance > 0)
            signedDistance = (_segmentLength / distance) - 1f;

        Vector2 translation = vectorFrom * (signedDistance * 0.5f);

        if (tileCollide)
        {
            if (!_locked[i1])
                _positions[i1] += TileCollision(_positions[i1], translation, true);

            if (!_locked[i2])
                _positions[i2] += TileCollision(_positions[i2], -translation, true);
        }
        else
        {
            if (!_locked[i1])
                _positions[i1] += translation;

            if (!_locked[i2])
                _positions[i2] -= translation;
        }
    }

    private static Vector2 TileCollision(Vector2 position, Vector2 velocity, bool fallThrough)
    {
        Vector2 newVel = Terraria.Collision.noSlopeCollision(position - new Vector2(3, 3), velocity, 6, 6, fallThrough, fallThrough);

        if (Math.Abs(newVel.X) < Math.Abs(velocity.X))
            velocity.X *= 0;

        if (Math.Abs(newVel.Y) < Math.Abs(velocity.Y))
            velocity.Y *= 0;

        return velocity;
    }
}

