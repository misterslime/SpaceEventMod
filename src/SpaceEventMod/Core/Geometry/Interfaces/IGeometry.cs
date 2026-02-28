namespace SpaceEventMod.Core.Geometry.Interfaces;

internal interface IGeometry<T>
{
    public T GetPoint(int index);
    public void SetPoint(T point, int index);
}
