using Microsoft.Xna.Framework;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

/// <summary>
/// Makes the prop able to be grappled to with a hook.<br/>
/// Requires the <see cref="Hitbox"/> component to function.
/// </summary>
public class Grappleable : Component
{
}

public class GrappleableSystem : ComponentSystem<Grappleable>
{
    public override void Load()
    {
        On_Projectile.AI_007_GrapplingHooks += GrappleMiscObjects;
    }

    public override void Unload()
    {
        On_Projectile.AI_007_GrapplingHooks -= GrappleMiscObjects;
    }

    private void GrappleMiscObjects(On_Projectile.orig_AI_007_GrapplingHooks orig, Projectile self)
    {
        orig(self);

        if (self.ai[0] == 2)
            return;

        foreach (Grappleable component in components)
        {
            Rectangle colliderBox = component.GetComponent<Hitbox>().GetBoundingBox();

            if (!(self.position.X + self.width > colliderBox.Left && self.position.X < colliderBox.Right) || !self.Hitbox.Intersects(colliderBox))
                continue;

            SetGrapple(self.position, self);
            return;
        }
    }

    /// <summary>
    /// Makes a grappling hook think it's grappled onto an object.
    /// This function was written by @Impaxim on discord. Thank you Impaxim!
    /// </summary>
    /// <param name="position">The position you want the grappling hook to grapple to.</param>
    /// <param name="grapple">The grappling hook projectile.</param>
    private void SetGrapple(Vector2 position, Projectile grapple)
    {
        //grapple.tileCollide = true;
        grapple.ai[0] = 2;
        Main.player[grapple.owner].grappling[Main.player[grapple.owner].grapCount] = grapple.whoAmI;
        Main.player[grapple.owner].grapCount++;
        grapple.velocity = Vector2.Zero;
        grapple.netUpdate = true;
        //Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, grapple.Center);
    }
}
