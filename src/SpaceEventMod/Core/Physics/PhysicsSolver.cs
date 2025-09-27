using Microsoft.Xna.Framework;
using SpaceEventMod.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria;

namespace SpaceEventMod.Core.Physics;

internal record struct PhysicsPassData(
    string Name,
    int Steps,
    Func<PhysicsPoint, SimulationContext, PhysicsPoint> Action
);

internal record struct SimulationContext(
    int Index,
    ReadOnlyDictionary<string, ParameterValue> GlobalData,
    PhysicsObject PhysicsObject,
    object IntegrationParameters
);

internal sealed class PhysicsSolver(Func<PhysicsPoint, object, PhysicsPoint> integrator, int timesConstrained = 8)
{
    private readonly Dictionary<string, ParameterValue> _globalData = new Dictionary<string, ParameterValue>{ ["timesConstrained"] = timesConstrained };
    private readonly List<PhysicsPassData> _preIntegrationPhysicsPasses = new List<PhysicsPassData>();
    private readonly List<PhysicsPassData> _postIntegrationPhysicsPasses = new List<PhysicsPassData>();
    private readonly Func<PhysicsPoint, object, PhysicsPoint> _integrator = integrator;

    public PhysicsSolver AddGlobalData(string key, ParameterValue value)
    {
        _globalData.Add(key, value);
        return this;
    }

    public PhysicsSolver AddGlobalData(params ReadOnlySpan<(string name, ParameterValue value)> values)
    {
        foreach (var value in values)
            _globalData.Add(value.name, value.value);

        return this;
    }

    public PhysicsSolver AddPhysicsPass(string name, bool preIntegration, Func<PhysicsPoint, SimulationContext, PhysicsPoint> action, int steps = 1)
    {
        if (preIntegration)
            _preIntegrationPhysicsPasses.Add(new PhysicsPassData(name, steps, action));
        else 
            _postIntegrationPhysicsPasses.Add(new PhysicsPassData(name, steps, action));

        return this;
    }

    public PhysicsSolver AddPhysicsPasses(bool preIntegration, params ReadOnlySpan<(string Name, int Steps, Func<PhysicsPoint, SimulationContext, PhysicsPoint> Action)> values)
    {
        foreach (var value in values)
            AddPhysicsPass(value.Name, preIntegration, value.Action, value.Steps);

        return this;
    }

    public PhysicsPoint RunSimulation(PhysicsPoint point, in object integrationParameters = null)
    {
        SimulationContext context = new SimulationContext()
        {
            GlobalData = _globalData.AsReadOnly(),
            IntegrationParameters = integrationParameters
        };

        PhysicsData data = RunSimulation(new PhysicsData([point], []), context);

        return data.GetPoint(0);
    }

    public PhysicsObject RunSimulation(PhysicsObject physicsObject, in object integrationParameters = null)
    {
        SimulationContext context = new SimulationContext()
        {
            PhysicsObject = physicsObject,
            GlobalData = _globalData.AsReadOnly(),
            IntegrationParameters = integrationParameters
        };

        List<PhysicsData> data = new List<PhysicsData>();

        for (int i = 0; i < physicsObject.PhysicsData.Length; i++)
            data.Add(RunSimulation(physicsObject.PhysicsData[i], context));

        return new PhysicsObject(data.ToArray());
    }

    public PhysicsData RunSimulation(PhysicsData physicsData, in SimulationContext context)
    {
        PhysicsData newPhysicsData = physicsData;

        // run pre-integration physics passes
        newPhysicsData = RunPhysicsPasses(newPhysicsData, in _preIntegrationPhysicsPasses, context);

        // integrate motion
        for (int j = 0; j < physicsData.PointCount; j++)
            newPhysicsData.SetPoint(j, _integrator(physicsData.GetPoint(j), context.IntegrationParameters));

        // run post-integration physics passes
        newPhysicsData = RunPhysicsPasses(newPhysicsData, in _postIntegrationPhysicsPasses, context);

        // if the physics data doesn't have links, end
        if (newPhysicsData.LinkCount <= 0)
            return newPhysicsData;

        // constrain linked points
        for (var i = 0; i < _globalData["timesConstrained"].Int; i++)
        {
            for (int j = 0; j < newPhysicsData.LinkCount; j++)
                newPhysicsData = DistanceConstraint(newPhysicsData.GetLink(j), newPhysicsData, context);
        }

        return newPhysicsData;
    }

    private PhysicsData RunPhysicsPasses(PhysicsData physicsData, in List<PhysicsPassData> physicsPasses, SimulationContext context)
    {
        if (physicsPasses.Count <= 0)
            return physicsData;

        PhysicsData newPhysicsData = physicsData;

        foreach (var physicsPass in physicsPasses)
        {
            for (var i = 0; i < physicsPass.Steps; i++)
            {
                for (int j = 0; j < physicsData.PointCount; j++)
                {
                    context.Index = j;
                    newPhysicsData.SetPoint(j, physicsPass.Action(newPhysicsData.GetPoint(j), context));
                }
            }
        }

        return newPhysicsData;
    }

    private PhysicsData DistanceConstraint(ILink link, PhysicsData data, SimulationContext context)
    {
        if (link is PhysicsLink physicsLink)
            return DistanceConstraint(physicsLink, data, context);
        else if (link is ControlledPhysicsLink controlledPhysicsLink)
            return DistanceConstraint(controlledPhysicsLink, data, context);

        return data;
    }

    private PhysicsData DistanceConstraint(PhysicsLink link, PhysicsData data, SimulationContext context)
    {
        PhysicsData newPhysicsData = data;

        int point1Index = link.GetPointIndex(true);
        int point2Index = link.GetPointIndex(false);

        var point1 = data.GetPoint(point1Index);
        var point2 = data.GetPoint(point2Index);

        var midPoint = (point1.Position + point2.Position) * 0.5f;
        var projection = (point1.Position - point2.Position).SafeNormalize(Vector2.Zero) * link.TargetDistance * 0.5f;

        point1.Position = midPoint + projection;
        point2.Position = midPoint - projection;

        newPhysicsData.SetPoint(point1Index, point1);
        newPhysicsData.SetPoint(point2Index, point2);

        return newPhysicsData;
    }

    private PhysicsData DistanceConstraint(ControlledPhysicsLink link, PhysicsData data, SimulationContext context)
    {
        PhysicsData newPhysicsData = data;

        string controlIndex = link.GetPointIndex(true);
        int pointIndex = link.GetPointIndex(false);

        PhysicsPoint physicsPoint = data.GetPoint(pointIndex);
        Vector2 controlPoint = context.PhysicsObject.LocalData[controlIndex].Vector2;

        var projection = (physicsPoint.Position - controlPoint).SafeNormalize(Vector2.Zero) * link.TargetDistance;

        physicsPoint.Position = controlPoint + projection;

        newPhysicsData.SetPoint(pointIndex, physicsPoint);

        return newPhysicsData;
    }
}
