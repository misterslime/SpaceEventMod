using Microsoft.Xna.Framework;

namespace SpaceEventMod.Common.Actions.Interfaces;

public interface ISquidInk
{
    public int Mana { get; set; }
    public int MaxMana { get; }
    public bool IsSpraying { get; set; }
    public Vector2 CloudPosition { get; set; }
}
