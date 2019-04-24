using FlatRedBall;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRBEngineTest.Screens
{
    public class SpriteTest : FlatRedBall.Screens.Screen
    {

        public SpriteTest() : base("SpriteTest") { }

        List<Sprite> sprites;

        public override void Initialize(bool addToManagers)
        {
            base.Initialize(addToManagers);

            Camera.Main.BackgroundColor = Microsoft.Xna.Framework.Color.CornflowerBlue;

            sprites = new List<Sprite>();

            for(var i = 0; i < 5000; i++)
            {
                var s = SpriteManager.AddSprite(@"content/icon.png");
                s.TextureScale = FlatRedBallServices.Random.Between(0.1f, 0.25f);
                s.RotationZVelocity = FlatRedBallServices.Random.Between(-1.5f, 1.5f);
                s.Position.X = FlatRedBallServices.Random.Between(Camera.Main.AbsoluteLeftXEdgeAt(0), Camera.Main.AbsoluteRightXEdgeAt(0));
                s.Position.Y = FlatRedBallServices.Random.Between(Camera.Main.AbsoluteBottomYEdgeAt(0), Camera.Main.AbsoluteTopYEdgeAt(0));
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
                sprites.RemoveAt(i);
            }

            base.Destroy();
        }
    }
}
