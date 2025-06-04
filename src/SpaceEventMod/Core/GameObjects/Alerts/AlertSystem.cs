using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.GameObjects.Alerts;

public class AlertSystem : ModSystem
{
    public static List<Alert> alerts = new List<Alert>();

    public override void OnWorldUnload()
    {
        alerts.Clear();
    }

    public override void PostUpdateNPCs()
    {
        for (var i = 0; i < alerts.Count; i++)
        {
            var alert = alerts[i];

            Main.NewText(alert.sourceEntity);

            if (alert.lifespan > 0)
                alert.lifespan--;
            else
            {
                alerts.RemoveAt(i);
                i--;
                continue;
            }

            alerts[i] = alert;
        }
    }
}
