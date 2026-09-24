using Microsoft.Xna.Framework;
using Terraria;

namespace WorldGenSandbox.Physics;

internal class Tentacle
{
    private float _segmentLength;
    private int _numSegments;
    private Vector2[] _positions;
    private Vector2[] _oldPositions;
    private Vector2[] _accelerations;
    private bool[] _locked;
    private int _time;

    public Tentacle(Vector2 startPos, int numSegments, float segmentLength, float startRotation)
    {
        _segmentLength = segmentLength;
        _positions = new Vector2[numSegments];
        _oldPositions = new Vector2[numSegments];
        _accelerations = new Vector2[numSegments];
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

        _time = 0;
    }

    public Vector2 AnchorStart { get => _positions[0]; set => _positions[0] = value; }
    public Vector2 AnchorEnd { get => _positions[_positions.Length - 1]; set => _positions[_positions.Length - 1] = value; }
    public Vector2 Gravity { get; set; }

    public int Count { get => _numSegments; }


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

    public void Update(int steps, float velocityDamping)
    {
        // joint physics
        for (int j = 0; j < steps; j++)
        {
            for (int i = 1; i < _positions.Length - 1; i++)
            {
                var midPoint = (_positions[i] + _positions[i + 1]) * 0.5f;
                var direction = (_positions[i] - _positions[i + 1]).SafeNormalize(Vector2.Zero);

                _positions[i] = _locked[i] ? _positions[i] : midPoint + direction * _segmentLength * 0.5f;
                _positions[i + 1] = _locked[i + 1] ? _positions[i + 1] : midPoint - direction * _segmentLength * 0.5f;

                midPoint = (_positions[i - 1] + _positions[i]) * 0.5f;
                direction = (_positions[i - 1] - _positions[i]).SafeNormalize(Vector2.Zero);

                _positions[i - 1] = _locked[i - 1] ? _positions[i - 1] : midPoint + direction * _segmentLength * 0.5f;
                _positions[i] = _locked[i] ? _positions[i] : midPoint - direction * _segmentLength * 0.5f;
            }
        }

        // damp tentacle movement
        for (int i = 0; i < _positions.Length; i++)
        {
            _accelerations[i] -= (_positions[i] - _oldPositions[i]) * velocityDamping;
        }

        // integrate
        for (int i = 0; i < _positions.Length; i++)
        {
            var newOldPositions = _positions[i];
            var newPosition = 2 * _positions[i] - _oldPositions[i] + _accelerations[i] + Gravity;

            _positions[i] = newPosition;
            _oldPositions[i] = newOldPositions;
            _accelerations[i] = Vector2.Zero;
        }

        _time++;
    }
}
