using System;

namespace SpaceEventMod.Core.Animation.Tweening;

public enum Ease : Byte
{
    Delay,
    Linear,

    Stepped1,
    Stepped2,
    Stepped3,
    Stepped4,
    Stepped5,
    Stepped10,

    InQuad,
    OutQuad,
    InOutQuad,

    InCubic,
    OutCubic,
    InOutCubic,

    InQuart,
    OutQuart,
    InOutQuart,

    InQuint,
    OutQuint,
    InOutQuint,

    InSextic,
    OutSextic,
    InOutSextic,

    InSine,
    OutSine,
    InOutSine,

    InExpo,
    OutExpo,
    InOutExpo,

    InCirc,
    OutCirc,
    InOutCirc,

    InElastic,
    OutElastic,
    InOutElastic,

    InBack,
    OutBack,
    InOutBack,

    InBounce,
    OutBounce,
    InOutBounce,
}
