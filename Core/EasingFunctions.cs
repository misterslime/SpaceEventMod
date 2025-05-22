using System;

namespace SpaceEventMod.Core;

public static class EasingFunctions
{
    public static float LinearEase(float completionValue) => completionValue;

    public static float SineEaseIn(float completionValue) => 1 - (float)Math.Cos(completionValue * Math.PI / 2D);

    public static float SineEaseOut(float completionValue) => (float)Math.Sin(completionValue * Math.PI / 2D);

    public static float SineEaseInOut(float completionValue) => (float)(-(Math.Cos(Math.PI * completionValue) - 1) / 2D);

    public static float CubicEaseIn(float completionValue) => (float)Math.Pow(completionValue, 3D);

    public static float CubicEaseOut(float completionValue) => (float)(1 - Math.Pow(1 - completionValue, 3D));

    public static float CubicEaseInOut(float completionValue) => completionValue < 0.5f ? 4f * (float)Math.Pow(completionValue, 3D) : (float)(1 - Math.Pow(-2D * completionValue + 2D, 3D) / 2D);

    public static float QuintEaseIn(float completionValue) => (float)Math.Pow(completionValue, 5D);

    public static float QuintEaseOut(float completionValue) => (float)(1 - Math.Pow(1 - completionValue, 5D));

    public static float QuintEaseInOut(float completionValue) => completionValue < 0.5f ? 16f * (float)Math.Pow(completionValue, 5D) : (float)(1 - Math.Pow(-2D * completionValue + 2D, 5D) / 2D);

    public static float CircEaseIn(float completionValue) => 1 - (float)Math.Sqrt(1 - Math.Pow(completionValue, 2D));

    public static float CircEaseOut(float completionValue) => (float)Math.Sqrt(1 - Math.Pow(completionValue - 1, 2D));

    public static float CircEaseInOut(float completionValue) => (float)(completionValue < 0.5 ? (1 - (float)Math.Sqrt(1 - Math.Pow(2D * completionValue, 2D))) / 2D : (float)(Math.Sqrt(1 - Math.Pow(-2D * completionValue + 2D, 2D)) + 1) / 2);

    public static float QuadEaseIn(float completionValue) => (float)Math.Pow(completionValue, 2D);

    public static float QuadEaseOut(float completionValue) => 1 - (float)Math.Pow(1 - completionValue, 2D);

    public static float QuadEaseInOut(float completionValue) => (float)(completionValue < 0.5 ? 2D * (float)Math.Pow(completionValue, 2D) : 1 - (float)Math.Pow(-2D * completionValue + 2D, 2D) / 2D);

    public static float QuartEaseIn(float completionValue) => (float)Math.Pow(completionValue, 4D);

    public static float QuartEaseOut(float completionValue) => 1 - (float)Math.Pow(1 - completionValue, 4D);

    public static float QuartEaseInOut(float completionValue) => (float)(completionValue < 0.5 ? 8 * (float)Math.Pow(completionValue, 4D) : 1 - (float)Math.Pow(-2D * completionValue + 2D, 4D) / 2D);

    public static float ExpoEaseIn(float completionValue) => completionValue == 0 ? 0 : (float)Math.Pow(2D, 10D * completionValue - 10D);

    public static float ExpoEaseOut(float completionValue) => completionValue == 1 ? 1 : 1 - (float)Math.Pow(2D, -10D * completionValue);

    public static float ExpoEaseInOut(float completionValue) => (float)(completionValue == 0 ? 0 : completionValue == 1 ? 1 : completionValue < 0.5 ? (float)Math.Pow(2D, 20 * completionValue - 10D) / 2D : 2D - (float)Math.Pow(2D, -20 * completionValue + 10D) / 2D);
}
