using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.NPCs.Droplings;
using SpaceEventMod.Core.Animation;
using SpaceEventMod.Core.DataStructures;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Collision;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Components.SoftBodies;
using SpaceEventMod.Core.Physics.Interfaces;
using SpaceEventMod.Core.Physics.Joints;
using SpaceEventMod.Core.Physics.Passes;
using SpaceEventMod.Core.Physics.Passes.Collision;
using SpaceEventMod.Core.Physics.Passes.Integrators;
using SpaceEventMod.Core.Physics.Passes.NPCs;
using SpaceEventMod.Core.Physics.Passes.SoftBodies;
using SpaceEventMod.Core.Utilities;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Animations;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Content.NPCs;

internal class SoftBodyExperiment : ModNPC
{
    private static PhysicsSolver s_softBodySolver;

    private PhysicsObject _physicsObject;

    public override void SetStaticDefaults()
    {
        s_softBodySolver = new PhysicsSolver();

        s_softBodySolver.AddPhysicsPass(new Gravity(new Vector2(0, 0.075f), 1));
        s_softBodySolver.AddPhysicsPass(new ConserveVolume(1));
        s_softBodySolver.AddPhysicsPass(new TileCollision(1));
        s_softBodySolver.AddPhysicsPass(new VerletIntegration());
        s_softBodySolver.AddPhysicsPass(new JointPhysics(4));
        s_softBodySolver.AddPhysicsPass(new AnchorShape());
        s_softBodySolver.AddPhysicsPass(new AnchorNPC());
    }

    public override void SetDefaults()
    {
        NPC.width = 46;
        NPC.height = 42;
        NPC.damage = 0;
        NPC.defense = 16;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
        NPC.aiStyle = -1;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        int numPoints = 16;
        float radius = 64f;

        List<PhysicsPoint> points = new List<PhysicsPoint>();
        List<IJoint> joints = new List<IJoint>();

        float angle = MathHelper.TwoPi / numPoints;

        //float length = ((Vector2.UnitX.RotatedBy(angle * 1) * radius) - (Vector2.UnitX.RotatedBy(angle * 2) * radius)).Length();

        for (int i = 0; i < numPoints; i++)
        {
            Vector2 pointPosition = Vector2.UnitX.RotatedBy(angle * i) * radius;

            pointPosition += NPC.Center;

            points.Add(new PhysicsPoint(pointPosition));
        }

        for (int i = 0; i < numPoints; i++)
        {
            int prevIndex = (i - 1 + numPoints) % numPoints;

            float length = (points[i].Position - points[prevIndex].Position).Length();

            joints.Add(new DistanceConstraint(new(IndexType.Point, i), new(IndexType.Point, prevIndex), length));
        }

        //joints.Add(new DistanceConstraint(new(IndexType.Point, 0), new(IndexType.Point, numPoints - 1), length));

        _physicsObject = new PhysicsObject(new(NPC.Center));

        _physicsObject.AddComponent(new PhysicsShape(points.ToArray()));
        _physicsObject.AddComponent(new PhysicsJoints(joints.ToArray()));
        _physicsObject.AddComponent(new NPCReference(NPC.whoAmI));
        _physicsObject.AddComponent(new AnchorObjectCentroid(true));

        PhysicsShape shape = _physicsObject.GetComponent<PhysicsShape>();

        _physicsObject.AddComponent(new GasFilledSoftBody(shape.GetArea(), 0, 0.0015f));
    }

    public override void AI()
    {
        s_softBodySolver.RunPhysicsPasses([_physicsObject]);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (_physicsObject is null)
            return false;

        ReadOnlySpan<PhysicsPoint> physicsPoints = _physicsObject.GetComponent<PhysicsShape>().Points;
        List<Vector2> positions = new List<Vector2>();

        for (int i = 0; i < physicsPoints.Length; i++)
            positions.Add(physicsPoints[i].Position);

        positions.Add(physicsPoints[0].Position);
        positions.Add(physicsPoints[1].Position);

        var trailPoints = new List<Vector2>();

        ReadOnlySpan<Vector2> controlPoints = positions.ToArray();

        trailPoints = new CatmullRomCurve(controlPoints, true).GetPoints(4);

        Graphics.BeginPipeline(0.5f)
            .DrawBasicTrail(
                trailPoints.ToArray(),
                _ => 3f,
                Assets.Assets.Textures.WhitePixel.Value,
                Color.White)
            .ApplyOutline(Color.Black)
            .Schedule(RenderLayer.AfterNPCs);

        return false;
    }
}