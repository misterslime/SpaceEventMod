using SpaceEventMod.Core.GameObjects.Stars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Actions.Interfaces;

public interface IWantStar
{
    public Star ObservedStar { get; set; }
}
