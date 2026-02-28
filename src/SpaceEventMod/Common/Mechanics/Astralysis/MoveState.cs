namespace SpaceEventMod.Common.Mechanics.Astralysis;

internal enum MoveState : byte
{
    Floor,
    LeftWall,
    RightWall,
    Ceiling,
    Falling,
    Jumping,
    KickedOut
}
