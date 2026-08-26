using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Geometry;

internal interface IGeometry<T>
{
    public T GetPoint(int index);
    public void SetPoint(T point, int index);
}
