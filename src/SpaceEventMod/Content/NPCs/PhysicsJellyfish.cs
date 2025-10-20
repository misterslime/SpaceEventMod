using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.NPCs.Droplings;
using SpaceEventMod.Core.Animation.Splines;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Components.SoftBodies;
using SpaceEventMod.Core.Physics.Interfaces;
using SpaceEventMod.Core.Physics.Joints;
using SpaceEventMod.Core.Physics.Passes;
using SpaceEventMod.Core.Physics.Passes.Collision;
using SpaceEventMod.Core.Physics.Passes.Integrators;
using SpaceEventMod.Core.Physics.Passes.NPCs;
using SpaceEventMod.Core.Physics.Passes.SoftBodies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Animations;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs;

internal class PhysicsJellyfish : ModNPC
{
    private static PhysicsSolver s_jellyfishSolver;

    private ref float Timer => ref NPC.ai[1];

    private PhysicsObject _softBody;
    private PhysicsObject _verletTail1;
    private PhysicsObject _verletTail2;
    private PhysicsObject _verletTail3;
    private PhysicsObject _verletTail4;
    private PhysicsObject _verletTail5;

    public override void SetStaticDefaults()
    {
        s_jellyfishSolver = new PhysicsSolver();

        s_jellyfishSolver.AddPhysicsPass(new PointsRepulsion(1));
        s_jellyfishSolver.AddPhysicsPass(new Gravity(new Vector2(0, 0.45f), 1));
        s_jellyfishSolver.AddPhysicsPass(new ConserveVolume(1));
        s_jellyfishSolver.AddPhysicsPass(new TileCollision(1));
        s_jellyfishSolver.AddPhysicsPass(new ProjectileCollision(1));
        s_jellyfishSolver.AddPhysicsPass(new BouncePlayers());
        s_jellyfishSolver.AddPhysicsPass(new VerletIntegration());
        s_jellyfishSolver.AddPhysicsPass(new AnchorShape());
        s_jellyfishSolver.AddPhysicsPass(new AnchorNPC());
        s_jellyfishSolver.AddPhysicsPass(new JointPhysics(5));
        s_jellyfishSolver.AddPhysicsPass(new DampenVelocity(1));
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

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (_softBody.HasComponent<NoGravity>())
            _softBody.RemoveComponent<NoGravity>();

        if (_softBody.HasComponent<NPCReference>())
            _softBody.RemoveComponent<NPCReference>();

        if (_softBody.HasComponent<AnchorObjectCentroid>())
            _softBody.RemoveComponent<AnchorObjectCentroid>();

        NPC.immortal = true;
        NPC.friendly = true;
        NPC.life = NPC.lifeMax;
    }

    public override void OnSpawn(IEntitySource source)
    {
        CreateSoftBody();

        CreateTail(ref _verletTail1);
        CreateTail(ref _verletTail2);
        CreateTail(ref _verletTail3);
        CreateTail(ref _verletTail4);
        CreateTail(ref _verletTail5);
    }

    private void CreateSoftBody()
    {
        int numPoints = 16;
        float radius = 64f;

        List<PhysicsPoint> points = new List<PhysicsPoint>();
        List<IJoint> joints = new List<IJoint>();

        float angle = MathHelper.TwoPi / numPoints;

        for (int i = 0; i < numPoints; i++)
        {
            float length = 22f;

            Vector2 pointPosition = Vector2.UnitX.RotatedBy(angle * i) * radius;

            pointPosition += NPC.Center;

            points.Add(new PhysicsPoint(pointPosition));

            if (i == 0)
                joints.Add(new DistanceConstraint(new(IndexType.Point, i), new(IndexType.ChildPosition, 0), length));

            else if (i == 2)
                joints.Add(new DistanceConstraint(new(IndexType.Point, i), new(IndexType.ChildPosition, 1), length));

            else if (i == 3)
                joints.Add(new DistanceConstraint(new(IndexType.Point, i), new(IndexType.ChildPosition, 2), length));

            else if (i == 4)
                joints.Add(new DistanceConstraint(new(IndexType.Point, i), new(IndexType.ChildPosition, 3), length));

            else if (i == 6)
                joints.Add(new DistanceConstraint(new(IndexType.Point, i), new(IndexType.ChildPosition, 4), length));
        }

        for (int i = 0; i < numPoints; i++)
        {
            int prevIndex = (i - 1 + numPoints) % numPoints;

            float length = (points[i].Position - points[prevIndex].Position).Length();

            joints.Add(new DistanceConstraint(new(IndexType.Point, i), new(IndexType.Point, prevIndex), length));
        }

        //joints.Add(new DistanceConstraint(new(IndexType.Point, 0), new(IndexType.Point, numPoints - 1), length));

        _softBody = new PhysicsObject(new(NPC.Center));

        _softBody.AddComponent(new PhysicsShape(points.ToArray(), true));
        _softBody.AddComponent(new PhysicsJoints(joints.ToArray()));
        _softBody.AddComponent(new NPCReference(NPC.whoAmI));
        _softBody.AddComponent(new AnchorObjectCentroid(false));
        _softBody.AddComponent(new DoesntRepel());
        _softBody.AddComponent(new NoGravity());

        PhysicsShape shape = _softBody.GetComponent<PhysicsShape>();

        _softBody.AddComponent(new GasFilledSoftBody(shape.GetArea(), 0, 0.0015f));
    }

    private void CreateTail(ref PhysicsObject physicsObject)
    {
        List<PhysicsPoint> points = new List<PhysicsPoint>();
        List<IJoint> joints = new List<IJoint>();

        float length = 22f;
        int segments = 10;

        Vector2 startVector = Vector2.UnitY;

        for (int j = 0; j < segments; j++)
        {
            points.Add(new PhysicsPoint(NPC.Center + startVector * length * (j + 1)));

            if (j > 0)
            {
                JointIndex index1 = new(IndexType.Point, j - 1);
                JointIndex index2 = new(IndexType.Point, j);

                joints.Add(new DistanceConstraint(index1, index2, length));
            }
        }

        JointIndex controlIndex = new(IndexType.ObjectPosition, 0);
        JointIndex pointIndex = new(IndexType.Point, 0);

        joints.Add(new DistanceConstraint(controlIndex, pointIndex, length, true));

        physicsObject = new PhysicsObject(new(NPC.Center));
        physicsObject.AddComponent(new PhysicsShape(points.ToArray()));
        physicsObject.AddComponent(new PhysicsJoints(joints.ToArray()));
        physicsObject.AddComponent(new NPCReference(NPC.whoAmI));

        _softBody.AddChild(physicsObject);
    }

    public override void AI()
    {
        s_jellyfishSolver.RunPhysicsPasses([_softBody, _verletTail1, _verletTail2, _verletTail3, _verletTail4, _verletTail5]);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Pipeline pipeline = Graphics.BeginPipeline(0.5f);

        DrawSoftBody(in pipeline, _softBody);
        DrawTail(in pipeline, _verletTail1, 0);
        DrawTail(in pipeline, _verletTail2, 2);
        DrawTail(in pipeline, _verletTail3, 3);
        DrawTail(in pipeline, _verletTail4, 4);
        DrawTail(in pipeline, _verletTail5, 6);

        pipeline
            .ApplyOutline(Color.Black)
            .Schedule(RenderLayer.AfterNPCs);

        return false;
    }

    private void DrawSoftBody(in Pipeline pipeline, PhysicsObject physicsObject)
    {
        if (physicsObject == null)
            return;

        ReadOnlySpan<PhysicsPoint> physicsPoints = physicsObject.GetComponent<PhysicsShape>().Points;
        List<Vector2> positions = new List<Vector2>();

        for (int i = 0; i < physicsPoints.Length; i++)
            positions.Add(physicsPoints[i].Position);

        positions.Add(physicsPoints[0].Position);
        positions.Add(physicsPoints[1].Position);

        var trailPoints = new List<Vector2>();

        ReadOnlySpan<Vector2> controlPoints = positions.ToArray();

        trailPoints = new CatmullRomCurve(controlPoints, true).GetPoints(4);

        pipeline.DrawBasicTrail(
            trailPoints.ToArray(),
            _ => 3f,
            Assets.Assets.Textures.WhitePixel.Value,
            Color.White);
    }

    private void DrawTail(in Pipeline pipeline, PhysicsObject physicsObject, int index)
    {
        if (physicsObject == null)
            return;

        PhysicsShape shape = physicsObject.GetComponent<PhysicsShape>();
        PhysicsShape softBody = physicsObject.GetComponent<PhysicsShape>();

        List<Vector2> points = new List<Vector2>();

        points.Add(softBody.Points[index].Position);

        for (int i = 0; i < shape.Points.Length; i++)
            points.Add(shape.Points[i].Position);

        points.Add(softBody.Points[index].Position);

        //points.Add(softBody.Points[index].Position);
        //points.Add(physicsObject.Center.Position);
        //points.Add(shape.Points[0].Position);
        //points.Add(shape.Points[1].Position);
        //points.Add(shape.Points[2].Position);

        var trailPoints = new List<Vector2>();

        //trailPoints.Add(softBody.Points[index].Position);

        ReadOnlySpan<Vector2> controlPoints = points.ToArray();
        trailPoints = new CatmullRomCurve(controlPoints, true).GetPoints(4);


        //trailPoints.Add(softBody.Points[index].Position);

        pipeline.DrawBasicTrail(
            trailPoints.ToArray(),
            _ => 2f,
            Assets.Assets.Textures.WhitePixel.Value,
            Color.White);
    }
}
