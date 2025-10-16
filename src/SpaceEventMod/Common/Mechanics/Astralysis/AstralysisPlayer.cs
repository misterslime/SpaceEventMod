using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Mechanics.StarsapCoating;
using SpaceEventMod.Content.Events.Space.LevelElements;
using SpaceEventMod.Core.DataStructures;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Common.Mechanics.Astralysis;

internal record struct MoveStateTransition(
    MoveState From,
    MoveState To,
    params List<(string Condition, TransitionValue Value)> Conditions);

internal class AstralysisPlayer : ModPlayer
{
    private static List<MoveStateTransition> s_stateTransitions = new List<MoveStateTransition>();

    public override void Load()
    {
        #region Floor to wall
        s_stateTransitions.Add(new(
            MoveState.Floor,
            MoveState.RightWall,
            ("bottom", StarsapTile.Coated),
            ("right", StarsapTile.Coated),
            ("controlRight", true)));

        s_stateTransitions.Add(new(
            MoveState.Floor,
            MoveState.RightWall,
            ("bottom", StarsapTile.Empty),
            ("bottomRight", StarsapTile.Coated),
            ("controlLeft", true)));

        s_stateTransitions.Add(new(
            MoveState.Floor,
            MoveState.LeftWall,
            ("bottom", StarsapTile.Coated),
            ("left", StarsapTile.Coated),
            ("controlLeft", true)));

        s_stateTransitions.Add(new(
            MoveState.Floor,
            MoveState.LeftWall,
            ("bottom", StarsapTile.Empty),
            ("bottomLeft", StarsapTile.Coated),
            ("controlRight", true)));
        #endregion

        #region Wall to floor
        s_stateTransitions.Add(new(
            MoveState.RightWall,
            MoveState.Floor,
            ("right", StarsapTile.Coated),
            ("bottom", StarsapTile.Coated),
            ("controlLeft", true)));

        s_stateTransitions.Add(new(
            MoveState.LeftWall,
            MoveState.Floor,
            ("left", StarsapTile.Coated),
            ("bottom", StarsapTile.Coated),
            ("controlRight", true)));

        s_stateTransitions.Add(new(
            MoveState.RightWall,
            MoveState.Floor,
            ("right", StarsapTile.Empty),
            ("bottomRight", StarsapTile.Coated),
            ("controlRight", true)));

        s_stateTransitions.Add(new(
            MoveState.LeftWall,
            MoveState.Floor,
            ("left", StarsapTile.Empty),
            ("bottomLeft", StarsapTile.Coated),
            ("controlLeft", true)));
        #endregion

        #region Ceiling to wall
        s_stateTransitions.Add(new(
            MoveState.Ceiling,
            MoveState.RightWall,
            ("top", StarsapTile.Coated),
            ("right", StarsapTile.Coated),
            ("controlLeft", true)));

        s_stateTransitions.Add(new(
            MoveState.Ceiling,
            MoveState.RightWall,
            ("top", StarsapTile.Empty),
            ("topRight", StarsapTile.Coated),
            ("controlRight", true)));

        s_stateTransitions.Add(new(
            MoveState.Ceiling,
            MoveState.LeftWall,
            ("top", StarsapTile.Coated),
            ("left", StarsapTile.Coated),
            ("controlRight", true)));

        s_stateTransitions.Add(new(
            MoveState.Ceiling,
            MoveState.LeftWall,
            ("top", StarsapTile.Empty),
            ("topLeft", StarsapTile.Coated),
            ("controlLeft", true)));
        #endregion

        #region Wall to ceiling
        s_stateTransitions.Add(new(
            MoveState.RightWall,
            MoveState.Ceiling,
            ("right", StarsapTile.Coated),
            ("top", StarsapTile.Coated),
            ("controlRight", true)));

        s_stateTransitions.Add(new(
            MoveState.LeftWall,
            MoveState.Ceiling,
            ("left", StarsapTile.Coated),
            ("top", StarsapTile.Coated),
            ("controlLeft", true)));

        s_stateTransitions.Add(new(
            MoveState.RightWall,
            MoveState.Ceiling,
            ("right", StarsapTile.Empty),
            ("topRight", StarsapTile.Coated),
            ("controlLeft", true)));

        s_stateTransitions.Add(new(
            MoveState.LeftWall,
            MoveState.Ceiling,
            ("left", StarsapTile.Empty),
            ("topLeft", StarsapTile.Coated),
            ("controlRight", true)));
        #endregion

        #region Grounded to falling
        s_stateTransitions.Add(new(
            MoveState.Floor,
            MoveState.Falling,
            ("bottom", StarsapTile.Empty),
            ("bottomLeft", StarsapTile.Empty),
            ("bottomRight", StarsapTile.Empty)));

        s_stateTransitions.Add(new(
            MoveState.Ceiling,
            MoveState.Falling,
            ("top", StarsapTile.Empty),
            ("topLeft", StarsapTile.Empty),
            ("topRight", StarsapTile.Empty)));

        s_stateTransitions.Add(new(
            MoveState.RightWall,
            MoveState.Falling,
            ("right", StarsapTile.Empty),
            ("topRight", StarsapTile.Empty),
            ("bottomRight", StarsapTile.Empty)));

        s_stateTransitions.Add(new(
            MoveState.LeftWall,
            MoveState.Falling,
            ("left", StarsapTile.Empty),
            ("bottomLeft", StarsapTile.Empty),
            ("topLeft", StarsapTile.Empty)));
        #endregion

        #region Falling to grounded
        s_stateTransitions.Add(new(
            MoveState.Falling,
            MoveState.Floor,
            ("bottom", StarsapTile.Coated)));

        s_stateTransitions.Add(new(
            MoveState.Falling,
            MoveState.Ceiling,
            ("top", StarsapTile.Coated)));

        s_stateTransitions.Add(new(
            MoveState.Falling,
            MoveState.RightWall,
            ("right", StarsapTile.Coated)));

        s_stateTransitions.Add(new(
            MoveState.Falling,
            MoveState.LeftWall,
            ("left", StarsapTile.Coated)));
        #endregion

        #region Jumping
        s_stateTransitions.Add(new(
            MoveState.Ceiling,
            MoveState.Jumping,
            ("controlJump", true)));

        s_stateTransitions.Add(new(
            MoveState.Floor,
            MoveState.Jumping,
            ("controlJump", true)));

        s_stateTransitions.Add(new(
            MoveState.RightWall,
            MoveState.Jumping,
            ("controlJump", true)));

        s_stateTransitions.Add(new(
            MoveState.LeftWall,
            MoveState.Jumping,
            ("controlJump", true)));

        s_stateTransitions.Add(new(
            MoveState.Jumping,
            MoveState.Falling));
        #endregion
    }

    private MoveState _lastGrounded;
    private MoveState _state;
    private Point _desiredPosition;
    private bool _active = false;
    private int _speed = 0;
    private float _progress = 1f;
    private Vector2 _astralysisVelocity = Vector2.Zero;
    private Vector2 _cameraPosition = Vector2.Zero;

    public override void ModifyScreenPosition()
    {
        if (!_active)
            return;

        var visualThing = GetSlidingVelocity(_state, _progress);
        visualThing *= _progress;

        Main.screenPosition = _cameraPosition - Main.ScreenSize.ToVector2() * 0.5f;

        _cameraPosition = Vector2.Lerp(_cameraPosition, _desiredPosition.ToWorldCoordinates() + visualThing, 0.14f);
    }

    public override void PostUpdate()
    {
        if (!_active)
            return;

        PushOut();

        var newState = GetState(new AdjacencyData<StarsapTile>(_desiredPosition.X, _desiredPosition.Y, GetStarsap));

        if (newState != _state && (newState == MoveState.Falling || newState == MoveState.Jumping))
        {
            _lastGrounded = _state;
        }

        _state = newState;

        if (_state == MoveState.Falling)
        {
            FallingBehavior();
        }
        else if (_state == MoveState.Jumping)
        {
            JumpingBehavior();
        }
        else
        {
            SlidingBehavior();
        }

        Player.velocity *= 0;
        Player.gfxOffY = 0;
    }

    private void FallingBehavior()
    {
        _astralysisVelocity += Vector2.UnitY * 0.5f;

        if (Player.controlLeft)
        {
            if (_astralysisVelocity.X > 1)
                _astralysisVelocity.X = MathHelper.Lerp(_astralysisVelocity.X, 0, 0.2f);

            _astralysisVelocity.X -= 0.25f;
        }
        else if (Player.controlRight)
        {
            if (_astralysisVelocity.X < 1)
                _astralysisVelocity.X = MathHelper.Lerp(_astralysisVelocity.X, 0, 0.2f);

            _astralysisVelocity.X += 0.25f;
        }
        else
            _astralysisVelocity.X = MathHelper.Lerp(_astralysisVelocity.X, 0, 0.01f);

        _astralysisVelocity.X = Math.Clamp(_astralysisVelocity.X, -12, 12);
        _astralysisVelocity.Y = Math.Clamp(_astralysisVelocity.Y, -16, 16);

        Player.Center += _astralysisVelocity + Player.velocity;
        _desiredPosition = Player.Center.ToTileCoordinates();
        _speed = (int)(_astralysisVelocity.X * 100);

        _speed = Math.Clamp(_speed, -75, 75);
    }

    private void JumpingBehavior()
    {
        PushInDirection(_lastGrounded, 2);
        if (_lastGrounded == MoveState.Floor)
        {
            _astralysisVelocity = -Vector2.UnitY * 16f;
        }
        else if (_lastGrounded == MoveState.LeftWall)
        {
            _astralysisVelocity = Vector2.UnitX * 16f;
        }
        else if (_lastGrounded == MoveState.RightWall)
        {
            _astralysisVelocity = -Vector2.UnitX * 16f;
        }
        else if (_lastGrounded == MoveState.Ceiling)
        {
            _astralysisVelocity = Vector2.UnitY * 16f;
        }

        _astralysisVelocity += GetSlidingVelocity(_lastGrounded, _speed * 0.01f);

        Player.Center = _desiredPosition.ToWorldCoordinates();
    }

    private void SlidingBehavior()
    {
        if (Player.controlLeft)
            _speed = MathHelper.Clamp(_speed - 7, -75, 75);
        else if (Player.controlRight)
            _speed = MathHelper.Clamp(_speed + 7, -75, 75);
        else if (_speed != 0)
            _speed = (int)MathHelper.Lerp(_speed, 0, 0.1f);

        _progress += _speed * 0.01f;

        var newPosition = _desiredPosition;
        if (_progress > 1f)
        {
            _progress -= 1f;

            newPosition = GetMovement(1);
        }
        else if (_progress < 0)
        {
            _progress += 1f;

            newPosition = GetMovement(-1);
        }
        _desiredPosition = newPosition;

        var visualThing = GetSlidingVelocity(_state, _progress);

        Player.Center = _desiredPosition.ToWorldCoordinates() + visualThing * _progress;
    }

    private Vector2 GetSlidingVelocity(MoveState state, float speed)
    {
        var unitVector = Vector2.Zero;

        if (state == MoveState.Floor)
        {
            unitVector = Vector2.UnitX * 16f;
        }
        else if (state == MoveState.LeftWall)
        {
            unitVector = Vector2.UnitY * 16f;
        }
        else if (state == MoveState.RightWall)
        {
            unitVector = -Vector2.UnitY * 16f;
        }
        else if (state == MoveState.Ceiling)
        {
            unitVector = -Vector2.UnitX * 16f;
        }

        return unitVector * speed;
    }

    private MoveState GetState(in AdjacencyData<StarsapTile> data)
    {
        foreach (var item in s_stateTransitions)
        {
            if (item.From != _state)
                continue;

            var shouldTransition = true;

            foreach(var condition in item.Conditions)
            {
                if (!CheckCondition(in data, condition.Condition, condition.Value))
                    shouldTransition = false;
            }

            if (shouldTransition)
                return item.To;
        }

        return _state;
    }

    private bool CheckCondition(in AdjacencyData<StarsapTile> data, string condition, TransitionValue value)
    {
        return condition switch
        {
            "top" => data.Top == value.StarsapTile,
            "bottom" => data.Bottom == value.StarsapTile,
            "left" => data.Left == value.StarsapTile,
            "right" => data.Right == value.StarsapTile,
            "topLeft" => data.TopLeft == value.StarsapTile,
            "topRight" => data.TopRight == value.StarsapTile,
            "bottomLeft" => data.BottomLeft == value.StarsapTile,
            "bottomRight" => data.BottomRight == value.StarsapTile,
            "controlLeft" => _speed < 0,
            "controlRight" => _speed > 0,
            "controlUp" => Player.controlUp == value.Bool,
            "controlDown" => Player.controlDown == value.Bool,
            "controlJump" => Player.controlJump == value.Bool,
            _ => throw new InvalidOperationException("awwfgaet;rgs")
        };
    }

    private void PushOut()
    {
        var tile = Framing.GetTileSafely(_desiredPosition);

        if (IsTileActive(tile))
        {
            PushInDirection(_state, 1);
        }
    }

    private void PushInDirection(MoveState state, int amount = 1)
    {
        if (state == MoveState.Floor)
        {
            _desiredPosition.Y -= amount;
        }
        else if (state == MoveState.LeftWall)
        {
            _desiredPosition.X += amount;
        }
        else if (state == MoveState.RightWall)
        {
            _desiredPosition.X -= amount;
        }
        else if (state == MoveState.Ceiling)
        {
            _desiredPosition.Y += amount;
        }
    }

    private Point GetMovement(int moveSpeed)
    {
        var newPosition = _desiredPosition;

        if (_state == MoveState.Floor)
        {
            newPosition.X += moveSpeed;
        }
        else if (_state == MoveState.LeftWall)
        {
            newPosition.Y += moveSpeed;
        }
        else if (_state == MoveState.RightWall)
        {
            newPosition.Y -= moveSpeed;
        }
        else if (_state == MoveState.Ceiling)
        {
            newPosition.X -= moveSpeed;
        }

        return newPosition;
    }

    public void ToggleAstralysis()
    {
        _state = MoveState.Falling;
        _active = !_active;
        _desiredPosition = Player.Center.ToTileCoordinates();
        _cameraPosition = Player.Center;
        _astralysisVelocity = Vector2.Zero;
    }

    private bool IsTileActive(Tile tile)
    {
        return tile.active() && Main.tileSolid[tile.type];
    }

    private StarsapTile GetStarsap(Tile tile)
    {
        if (!IsTileActive(tile))
            return StarsapTile.Empty;

        return StarsapTile.Coated;
    }
}
