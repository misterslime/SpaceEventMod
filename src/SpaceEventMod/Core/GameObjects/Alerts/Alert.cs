namespace SpaceEventMod.Core.GameObjects.Alerts;

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
