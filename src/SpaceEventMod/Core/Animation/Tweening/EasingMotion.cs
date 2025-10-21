using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceEventMod.Core.Animation.Tweening;

/// <summary>
/// A set of chained easing curves that constitutes a full motion/animation curve.
/// </summary>
internal class EasingMotion()
{
    private record struct MotionSegment(float EndTime, float EndValue, Ease Curve);

    private MotionSegment[] _segments = [];

    private float _startValue = 0f;

    private float _endValue = 0f;
    private float _duration = 0f;

    private int _loops = -1;
    private LoopType _loopType = LoopType.Repeat;

    /// <summary>
    /// Set the value the animation will start with.
    /// </summary>
    /// <param name="startValue">Value the animation should start at.</param>
    /// <returns></returns>
    public EasingMotion SetStart(float startValue)
    {
        _startValue = startValue;
        return this;
    }

    /// <summary>
    /// Set the type of looping the motion does, along with the number of loops.
    /// </summary>
    /// <param name="type">Looping type.</param>
    /// <param name="loops">Amount of looping before completion</param>
    /// <returns></returns>
    public EasingMotion SetLoops(LoopType type, int loops = -1)
    {
        _loops = loops;
        _loopType = type;

        return this;
    }

    /// <summary>
    /// Sets a delay period after the previous easing curve segment.
    /// </summary>
    /// <param name="duration">How long the delay lasts.</param>
    /// <returns></returns>
    public EasingMotion DelayMotion(float duration)
    {
        if (_segments.Length == 0)
        {
            this.ChainMotion(duration, _startValue, Ease.Delay);
            return this;
        }

        this.ChainMotion(duration, _endValue, Ease.Delay);
        return this;
    }

    /// <summary>
    /// Adds a new easing curve into the overall easing motion.
    /// </summary>
    /// <param name="duration">How long this segment lasts.</param>
    /// <param name="endValue">The end value of the segment when completed.</param>
    /// <param name="curve">Type of easing curve this segment has.</param>
    /// <param name="type">The type/shape of the curve.</param>
    /// <returns></returns>
    public EasingMotion ChainMotion(float duration, float endValue, Ease curve)
    {
        _endValue = endValue;
        _duration += duration;

        MotionSegment segment = new MotionSegment(_duration, endValue, curve);

        if (_segments.Length == 0)
        {
            _segments = [ segment ];

            return this;
        }

        List<MotionSegment> segments = _segments.ToList();

        segments.Add(segment);

        _segments = segments.ToArray();

        return this;
    }

    /// <summary>
    /// Evaluates the result of the chained easing curves.
    /// </summary>
    /// <param name="time">Amount of time that has passed since this animation's beginning. Can be ticks, seconds, anything.</param>
    /// <param name="completed">Whether the animation is finished. Only relevant if there are a finite number of loops.</param>
    /// <returns>A float value given by easing 2 easing curves in this motion.</returns>
    public float Evaluate(float time, out bool completed)
    {
        completed = false;

        if (time >= _duration * _loops && _loops != -1)
        {
            completed = true;
            return _endValue;
        }

        // loop time
        float timeLooped = _loopType switch
        {
            LoopType.Repeat => time % _duration,
            LoopType.Yoyo => MathF.Abs((time % (2f * _duration)) - _duration)
        };

        float previousEndTime = 0f;
        float previousEndValue = _startValue;
        foreach (var segment in _segments)
        {
            if (timeLooped > segment.EndTime)
            {
                previousEndTime = segment.EndTime;
                previousEndValue = segment.EndValue;
                continue;
            }

            float normalized = (timeLooped - previousEndTime) / (segment.EndTime - previousEndTime);

            float easing = segment.Curve.Evaluate(normalized);

            return MathHelper.Lerp(previousEndValue, segment.EndValue, easing);
        }

        throw new Exception("Easing motion has no segments.");
    }
}
