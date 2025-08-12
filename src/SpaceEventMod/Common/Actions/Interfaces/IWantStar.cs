using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Events.FirmamentTide.Stars;

namespace SpaceEventMod.Common.Actions.Interfaces;

public interface IWantStar
{
    public Star ObservedStar { get; set; }

    public Vector2 RelativePosition { get; set; }

    public bool DrinkAnimation();
}
