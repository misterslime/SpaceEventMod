using SpaceEventMod.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Animation.Tweening;

internal static partial class EasingFunctions
{
    private delegate float EasingFunction(float interpolant);

    public static float Evaluate(float t, EaseCurve curve, EaseType type)
    {
        return curve switch
        {
            EaseCurve.Sine => Evaluate(t, type, InSine, OutSine, InOutSine),
            EaseCurve.Quad => Evaluate(t, type, InQuad, OutQuad, InOutQuad),
            EaseCurve.Cubic => Evaluate(t, type, InCubic, OutCubic, InOutCubic),
            EaseCurve.Quart => Evaluate(t, type, InQuart, OutQuart, InOutQuart),
            EaseCurve.Quint => Evaluate(t, type, InQuint, OutQuint, InOutQuint),
            EaseCurve.Expo => Evaluate(t, type, InExpo, OutExpo, InOutExpo),
            EaseCurve.Circ => Evaluate(t, type, InCirc, OutCirc, InOutCirc),
            EaseCurve.Elastic => Evaluate(t, type, InElastic, OutElastic, InOutElastic),
            EaseCurve.Back => Evaluate(t, type, InBack, OutBack, InOutBack),
            EaseCurve.Bounce => Evaluate(t, type, InBounce, OutBounce, InOutBounce),
            _ => t,
        };
    }

    private static float Evaluate(
        float t,
        EaseType type, 
        in EasingFunction inFunction, 
        in EasingFunction outFunction, 
        in EasingFunction inOutFunction)
    {
        return type switch
        {
            EaseType.In => inFunction(t),
            EaseType.Out => outFunction(t),
            EaseType.InOut => inOutFunction(t),
            _ => t,
        };
    }
}
