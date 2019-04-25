using FlatRedBall;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRBEngineTest.Screens
{
    public class SpriteTest : FlatRedBall.Screens.Screen
    {
        const float movementVelocity = 25f;
        public SpriteTest() : base("SpriteTest") { }

        List<Sprite> sprites;

        public override void Initialize(bool addToManagers)
        {
            base.Initialize(addToManagers);

            sprites = new List<Sprite>();

            for(var i = 0; i < 5000; i++)
            {
                var angle = FlatRedBallServices.Random.Between(-3.14f, 3.14f);
                var s = SpriteManager.AddSprite(@"content/icon.png");
                s.TextureScale = FlatRedBallServices.Random.Between(0.2f, 0.4f);
                s.Alpha = FlatRedBallServices.Random.Between(0.1f, 0.75f);
                s.RotationZVelocity = FlatRedBallServices.Random.Between(-1.5f, 1.5f);
                s.Position.X = FlatRedBallServices.Random.Between(Camera.Main.AbsoluteLeftXEdgeAt(0), Camera.Main.AbsoluteRightXEdgeAt(0));
                s.Position.Y = FlatRedBallServices.Random.Between(Camera.Main.AbsoluteBottomYEdgeAt(0), Camera.Main.AbsoluteTopYEdgeAt(0));
                s.Velocity.X = (float)Math.Sin(angle) * movementVelocity;
                s.Velocity.Y = (float)Math.Cos(angle) * movementVelocity;
                sprites.Add(s);
            }
        }

        public override void Activity(bool firstTimeCalled)
        {
            base.Activity(firstTimeCalled);
        }

        public override void Destroy()
        {
            for(var i = sprites.Count - 1; i > -1; i--)
            {
                SpriteManager.RemoveSprite(sprites[i]);
                sprites[i] = null;
                sprites.RemoveAt(i);
            }

            base.Destroy();
        }
    }
}
