using SpaceEventMod.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Animation.Tweening;

internal static partial class EasingFunctions
{
    public static float Evaluate(this Ease curve, float interpolant)
    {
        return curve switch
        {
            Ease.Delay => 0f,

            Ease.InQuad => InQuad(interpolant),
            Ease.OutQuad => OutQuad(interpolant),
            Ease.InOutQuad => InOutQuad(interpolant),

            Ease.InCubic => InCubic(interpolant),
            Ease.OutCubic => OutCubic(interpolant),
            Ease.InOutCubic => InOutCubic(interpolant),

            Ease.InQuart => InQuart(interpolant),
            Ease.OutQuart => OutQuart(interpolant),
            Ease.InOutQuart => InOutQuart(interpolant),

            Ease.InQuint => InQuint(interpolant),
            Ease.OutQuint => OutQuint(interpolant),
            Ease.InOutQuint => InOutQuint(interpolant),

            Ease.InSine => InSine(interpolant),
            Ease.OutSine => OutSine(interpolant),
            Ease.InOutSine => InOutSine(interpolant),

            Ease.InExpo => InExpo(interpolant),
            Ease.OutExpo => OutExpo(interpolant),
            Ease.InOutExpo => InOutExpo(interpolant),

            Ease.InCirc => InCirc(interpolant),
            Ease.OutCirc => OutCirc(interpolant),
            Ease.InOutCirc => InOutCirc(interpolant),

            Ease.InElastic => InElastic(interpolant),
            Ease.OutElastic => OutElastic(interpolant),
            Ease.InOutElastic => InOutElastic(interpolant),

            Ease.InBack => InBack(interpolant),
            Ease.OutBack => OutBack(interpolant),
            Ease.InOutBack => InOutBack(interpolant),

            Ease.InBounce => InBounce(interpolant),
            Ease.OutBounce => OutBounce(interpolant),
            Ease.InOutBounce => InOutBounce(interpolant),

            _ => interpolant,
        };
    }
}
