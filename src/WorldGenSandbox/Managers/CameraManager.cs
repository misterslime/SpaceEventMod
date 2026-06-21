using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldGenSandbox.Managers;

// https://community.monogame.net/t/simple-2d-camera/9135
// will probby rewrite this later
internal class CameraManager
{
    public float Zoom { get; set; }
    public Vector2 Position { get; set; }
    public Rectangle Bounds { get; protected set; }
    public Rectangle VisibleArea { get; protected set; }
    public Matrix Transform { get; protected set; }

    private float currentMouseWheelValue, previousMouseWheelValue;

    public CameraManager(Viewport viewport)
    {
        Bounds = viewport.Bounds;
        Zoom = 1f;
        Position = Vector2.Zero;
    }

    public void MoveCamera(Vector2 movePosition)
    {
        Vector2 newPosition = Position + movePosition;
        Position = newPosition;
    }

    private void UpdateMatrix()
    {
        Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                Matrix.CreateScale(Zoom) *
                Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0));
        UpdateVisibleArea();
    }

    private void UpdateVisibleArea()
    {
        var inverseViewMatrix = Matrix.Invert(Transform);

        var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
        var tr = Vector2.Transform(new Vector2(Bounds.X, 0), inverseViewMatrix);
        var bl = Vector2.Transform(new Vector2(0, Bounds.Y), inverseViewMatrix);
        var br = Vector2.Transform(new Vector2(Bounds.Width, Bounds.Height), inverseViewMatrix);

        var min = new Vector2(
            MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
            MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
        var max = new Vector2(
            MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
            MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
        VisibleArea = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
    }

    public void AdjustZoom(float zoomAmount)
    {
        Zoom += zoomAmount;
        if (Zoom < .2f)
            Zoom = .2f;
        if (Zoom > 16f)
            Zoom = 16f;
    }

    public void UpdateCamera(Viewport bounds)
    {
        Bounds = bounds.Bounds;
        UpdateMatrix();

        Vector2 cameraMovement = Vector2.Zero;
        int moveSpeed;

        if (Zoom > .8f)
            moveSpeed = 15;
        else if (Zoom < .8f && Zoom >= .6f)
            moveSpeed = 20;
        else if (Zoom < .6f && Zoom > .35f)
            moveSpeed = 25;
        else if (Zoom <= .35f)
            moveSpeed = 30;
        else
            moveSpeed = 10;

        if (Keyboard.GetState().IsKeyDown(Keys.W))
            cameraMovement.Y = -moveSpeed;

        if (Keyboard.GetState().IsKeyDown(Keys.S))
            cameraMovement.Y = moveSpeed;

        if (Keyboard.GetState().IsKeyDown(Keys.A))
            cameraMovement.X = -moveSpeed;

        if (Keyboard.GetState().IsKeyDown(Keys.D))
            cameraMovement.X = moveSpeed;

        previousMouseWheelValue = currentMouseWheelValue;
        currentMouseWheelValue = Mouse.GetState().ScrollWheelValue;

        if (currentMouseWheelValue > previousMouseWheelValue)
            AdjustZoom(.2f);

        if (currentMouseWheelValue < previousMouseWheelValue)
            AdjustZoom(-.2f);

        MoveCamera(cameraMovement);
    }
}
