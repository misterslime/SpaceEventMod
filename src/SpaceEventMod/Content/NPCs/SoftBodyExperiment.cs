using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.NPCs.Droplings;
using SpaceEventMod.Core.DataStructures;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Collision;
using SpaceEventMod.Core.Utilities;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Animations;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Content.NPCs;

internal class SoftBodyExperiment : ModNPC
{
    private static PhysicsSolver s_physicsSolver;

    private PhysicsObject _physicsObject;

    public override void SetStaticDefaults()
    {
        s_physicsSolver = new PhysicsSolver(Integrators.VerletIntegration)
            .AddGlobalData(
                ("gravity", new Vector2(0, 0.075f)))
            .AddPhysicsPasses(true,
                ("gravity", 1, (PhysicsPoint point, SimulationContext context) =>
                {
                    point.Acceleration += context.GlobalData["gravity"].Vector2;

                    return point;
                }),
                ("conserveVolume", 1, (PhysicsPoint point, SimulationContext context) =>
                {
                    context.PhysicsObject.LocalData.TryGetValue("desiredArea", out ParameterValue desiredArea);
                    context.PhysicsObject.LocalData.TryGetValue("currentArea", out ParameterValue currentArea);
                    context.PhysicsObject.LocalData.TryGetValue("scaleFactor", out ParameterValue scaleFactor);

                    float dilation = scaleFactor.Float * (desiredArea.Float - currentArea.Float);

                    ReadOnlySpan<PhysicsPoint> data = context.PhysicsObject.PhysicsData[0].Points;

                    int leftIndex = (context.Index - 1 + data.Length) % data.Length;
                    int rightIndex = (context.Index + 1) % data.Length;

                    Vector2 point1 = data[leftIndex].Position;
                    Vector2 point2 = data[rightIndex].Position;

                    Vector2 vector = point1 - point2;

                    vector = new Vector2(-vector.Y, vector.X).SafeNormalize(Vector2.Zero) * dilation;

                    point.Acceleration += vector;

                    return point;
                }),
                ("tileCollisions", 1, (PhysicsPoint point, SimulationContext context) => TileCollisionHelper.CheckPoint(point, 6, 16))
            );
    }



    private float ShapeArea(ReadOnlySpan<PhysicsPoint> physicsPoints)
    {
        float area = 0;

        for (int i = 0; i < physicsPoints.Length; i++)
        {
            //int leftIndex = (i - 1 + physicsPoints.Length) % physicsPoints.Length;
            int rightIndex = (i + 1) % physicsPoints.Length;

            Vector2 point1 = physicsPoints[i].Position;
            Vector2 point2 = physicsPoints[rightIndex].Position;

            float width = point2.X - point1.X;
            float length = (point1.Y + point2.Y) * 0.5f;

            area += width * length;
        }

        //Main.NewText(area);

        return area;
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
        List<ILink> links = new List<ILink>();

        float angle = MathHelper.TwoPi / numPoints;

        float length = ((Vector2.UnitX.RotatedBy(angle * 1) * radius) - (Vector2.UnitX.RotatedBy(angle * 2) * radius)).Length();

        float desiredArea = -12540;

        for (int i = 0; i < numPoints; i++)
        {
            Vector2 pointPosition = Vector2.UnitX.RotatedBy(angle * i) * radius;

            pointPosition += NPC.Center;

            points.Add(new PhysicsPoint(pointPosition));

            if (i > 0)
                links.Add(new PhysicsLink(i - 1, i, length));
        }

        links.Add(new PhysicsLink(0, numPoints - 1, length));

        _physicsObject = new PhysicsObject([new(points.ToArray(), links.ToArray())])
            .AddLocalData(
                ("desiredArea", desiredArea),
                ("currentArea", desiredArea),
                ("scaleFactor", 1f));
    }

    public override void AI()
    {
        _physicsObject = s_physicsSolver.RunSimulation(_physicsObject);
        _physicsObject = _physicsObject.SetLocalData("desiredArea", 12540f);
        _physicsObject = _physicsObject.SetLocalData("currentArea", -ShapeArea(_physicsObject.PhysicsData[0].Points));
        _physicsObject = _physicsObject.SetLocalData("scaleFactor", 0.00075f);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        PhysicsObject psgmdomf = _physicsObject;

        if (psgmdomf.Equals(default(PhysicsObject)))
            return false;

        ReadOnlySpan<PhysicsPoint> physicsPoints = _physicsObject.PhysicsData[0].Points;
        List<Vector2> positions = new List<Vector2>();

        for (int i = 0; i < physicsPoints.Length; i++)
        {
            positions.Add(physicsPoints[i].Position);
        }

        positions.Add(physicsPoints[0].Position);

        Graphics.BeginPipeline(0.5f)
            .DrawBasicTrail(
                positions.ToArray(),
                _ => 2f,
                Assets.Assets.Textures.WhitePixel.Value,
                Color.White)
            .ApplyOutline(Color.Black)
            .Schedule(RenderLayer.AfterNPCs);

        /*for (int i = 0; i < _physicsObject.PhysicsData[0].LinkCount; i++)
        {
            Tuple<PhysicsPoint, PhysicsPoint> physicsPoints = _physicsObject.GetLinkPoints(0, i);

            Vector2 point1Position = physicsPoints.Item1.Position;
            Vector2 point2Position = physicsPoints.Item2.Position;

            spriteBatch.DrawLine(point1Position - screenPos, point2Position - screenPos, Color.White, 2);
        }*/

        return false;
    }
}
