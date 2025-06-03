using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.GameObjects.Alert;

public enum AlertType
{
    MiningStar
}

public struct Alert(AlertType type, int sourceEntity, int lifespan = 1)
{
    public AlertType alertType = type;
    public int sourceEntity = sourceEntity;
    public int lifespan = lifespan;
}
