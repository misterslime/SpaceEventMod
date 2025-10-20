using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Animation.Tweening;

internal enum EaseCurve : byte
{
    Linear,
    Sine,
    Quad,
    Cubic,
    Quart,
    Quint,
    Expo,
    Circ,
    Elastic,
    Back,
    Bounce,
    Custom
}

internal enum EaseType : byte
{
    In,
    Out,
    InOut
}

