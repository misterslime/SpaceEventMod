using SpaceEventMod.Core.GameObjects.Asteroids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.GameObjects.Alert;

public class AlertSystem : ModSystem
{
    public static List<Alert> alerts = new List<Alert>();

    public override void OnWorldUnload()
    {
        alerts.Clear();
    }

    public override void PostUpdateNPCs()
    {
        for (int i = 0; i < alerts.Count; i++)
        {
            Alert alert = alerts[i];

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
