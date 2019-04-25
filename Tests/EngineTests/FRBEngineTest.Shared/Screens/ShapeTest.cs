using FlatRedBall;
using FlatRedBall.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRBEngineTest.Screens
{
    public class ShapeTest : FlatRedBall.Screens.Screen
    {
        const float movementVelocity = 25f;

        public ShapeTest() : base("ShapeTest") { }

        List<Circle> circles = new List<Circle>();
        List<AxisAlignedRectangle> rectangles = new List<AxisAlignedRectangle>();
        List<Polygon> polygons = new List<Polygon>();

        public override void Initialize(bool addToManagers)
        {
            base.Initialize(addToManagers);

            for(var i = 0; i < 1000; i++)
            {
                var angle = FlatRedBallServices.Random.Between(-3.14f, 3.14f);
                var c = ShapeManager.AddCircle();
                c.Radius = FlatRedBallServices.Random.Between(4f, 15f);
                c.Position.X = FlatRedBallServices.Random.Between(Camera.Main.AbsoluteLeftXEdgeAt(0), Camera.Main.AbsoluteRightXEdgeAt(0));
                c.Position.Y = FlatRedBallServices.Random.Between(Camera.Main.AbsoluteBottomYEdgeAt(0), Camera.Main.AbsoluteTopYEdgeAt(0));
                c.Color = Microsoft.Xna.Framework.Color.Aquamarine;
                c.Velocity.X = (float)Math.Sin(angle) * movementVelocity;
                c.Velocity.Y = (float)Math.Cos(angle) * movementVelocity;
                circles.Add(c);
            }

            for(var i = 0; i < 1000; i++)
            {
                var angle = FlatRedBallServices.Random.Between(-3.14f, 3.14f);
                var r = ShapeManager.AddAxisAlignedRectangle();
                r.Width = FlatRedBallServices.Random.Between(4f, 15f);
                r.Height = FlatRedBallServices.Random.Between(4f, 15f);
                r.Position.X = FlatRedBallServices.Random.Between(Camera.Main.AbsoluteLeftXEdgeAt(0), Camera.Main.AbsoluteRightXEdgeAt(0));
                r.Position.Y = FlatRedBallServices.Random.Between(Camera.Main.AbsoluteBottomYEdgeAt(0), Camera.Main.AbsoluteTopYEdgeAt(0));
                r.Color = Microsoft.Xna.Framework.Color.GreenYellow;
                r.Velocity.X = (float)Math.Sin(angle) * movementVelocity;
                r.Velocity.Y = (float)Math.Cos(angle) * movementVelocity;
                rectangles.Add(r);
            }

            for(var i = 0; i < 1000; i++)
            {
                var angle = FlatRedBallServices.Random.Between(-3.14f, 3.14f);
                var p = ShapeManager.AddPolygon();
                p.Points = new List<Point>()
                {
                    new Point(-8, 8),
                    new Point(8,8),
                    new Point(8, -8),
                    new Point(-8, -8),
                    new Point(-8, 8)
                };
                p.Position.X = FlatRedBallServices.Random.Between(Camera.Main.AbsoluteLeftXEdgeAt(0), Camera.Main.AbsoluteRightXEdgeAt(0));
                p.Position.Y = FlatRedBallServices.Random.Between(Camera.Main.AbsoluteBottomYEdgeAt(0), Camera.Main.AbsoluteTopYEdgeAt(0));
                p.Color = Microsoft.Xna.Framework.Color.Pink;
                p.Velocity.X = (float)Math.Sin(angle) * movementVelocity;
                p.Velocity.Y = (float)Math.Cos(angle) * movementVelocity;
                p.RotationZVelocity = FlatRedBallServices.Random.Between(-1.5f, 1.5f);
                polygons.Add(p);
            }
        }

        public override void Activity(bool firstTimeCalled)
        {
            base.Activity(firstTimeCalled);
        }

        public override void Destroy()
        {
            for(var i = circles.Count - 1; i > -1; i--)
            {
                ShapeManager.Remove(circles[i]);
                circles[i] = null;
                circles.RemoveAt(i);
            }

            for (var i = rectangles.Count - 1; i > -1; i--)
            {
                ShapeManager.Remove(rectangles[i]);
                rectangles[i] = null;
                rectangles.RemoveAt(i);
            }

            for (var i = polygons.Count - 1; i > -1; i--)
            {
                ShapeManager.Remove(polygons[i]);
                polygons[i] = null;
                polygons.RemoveAt(i);
            }

            base.Destroy();
        }
    }
}
