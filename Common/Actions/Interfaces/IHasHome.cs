using Microsoft.Xna.Framework;

namespace SpaceEventMod.Common.Actions.Interfaces;

public interface IHasHome
{
    Vector2 HomePosition { get; set; }
}
