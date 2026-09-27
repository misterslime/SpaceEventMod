using Daybreak.Common.Mathematics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace SpaceEventMod.Common.Physics;

// feature ideas:
// rotation constraints
// spring constraints instead of rigid constraints?
// replace locking with bias?
// tile collision and wind grid physics
// ability to apply linear and angular forces?
internal class VerletString
{
    private float _segmentLength;
    private int _numSegments;
    private Vector2[] _positions;
    private Vector2[] _oldPositions;
    private Vector2[] _accelerations;
    private bool[] _locked;
    private float[] _angles;

    public VerletString(Vector2 startPos, int numSegments, float segmentLength, float startRotation)
    {
        _segmentLength = segmentLength;
        _positions = new Vector2[numSegments];
        _oldPositions = new Vector2[numSegments];
        _accelerations = new Vector2[numSegments];
        _angles = new float[numSegments];
        _locked = new bool[numSegments];
        _numSegments = numSegments;

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
    
    public void Update(int steps, float damping = 0f, bool collideWithTiles = false)
    {
        float velocityMult = 1f - damping;

        // integrate
        for (int i = 0; i < _positions.Length; i++)
        {
            if (_locked[i])
                continue;

            _accelerations[i] += Gravity;

            var oldPosition = _positions[i];
            var velocity = _positions[i] - _oldPositions[i];
            velocity *= velocityMult; // dampen movement
            velocity += _accelerations[i];

            if (collideWithTiles)
                velocity = TileCollision(_positions[i], velocity, true);

            _positions[i] += velocity;
            _oldPositions[i] = oldPosition;
            _accelerations[i] = Vector2.Zero;
        }

        // apply distance constraints
        for (int j = 0; j < steps; j++)
        {
            for (int i = 1; i < _positions.Length - 1; i++)
            {
                var midPoint = (_positions[i] + _positions[i + 1]) * 0.5f;
                var direction = (_positions[i] - _positions[i + 1]).SafeNormalize(Vector2.Zero);

                ConstrainPointTo(i + 1, midPoint - direction * _segmentLength * 0.5f, collideWithTiles);
                ConstrainPointTo(i, midPoint + direction * _segmentLength * 0.5f, collideWithTiles);

                midPoint = (_positions[i - 1] + _positions[i]) * 0.5f;
                direction = (_positions[i - 1] - _positions[i]).SafeNormalize(Vector2.Zero);

                ConstrainPointTo(i - 1, midPoint + direction * _segmentLength * 0.5f, collideWithTiles);
                ConstrainPointTo(i, midPoint - direction * _segmentLength * 0.5f, collideWithTiles);
            }
        }

        // apply angular constraints
    }

    private void ConstrainPointTo(int i, Vector2 position, bool tileCollide)
    {
        if (_locked[i])
            return;
        
        if (tileCollide)
            _positions[i] += TileCollision(_positions[i], position - _positions[i], true);
        else
            _positions[i] = position;
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

