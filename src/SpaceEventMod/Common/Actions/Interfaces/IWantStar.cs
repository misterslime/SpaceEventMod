using SpaceEventMod.Core.GameObjects.Stars;

namespace SpaceEventMod.Common.Actions.Interfaces;

public interface IWantStar
{
    public Star FoundStar { get; set; }
}
