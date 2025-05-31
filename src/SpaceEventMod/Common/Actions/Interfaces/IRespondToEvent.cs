using System;

namespace SpaceEventMod.Common.Actions.Interfaces;

public interface IRespondToEvent
{
    public Guid EventProp { get; set; }
}
