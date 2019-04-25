using System;
using System.Collections.Generic;
using System.Reflection;

using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Screens;
using Microsoft.Xna.Framework;

using System.Linq;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FRBEngineTest
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const float secondsPerScreen = 10f;
        List<Type> testScreens = new List<Type>()
        {
            typeof(Screens.SpriteTest),
            typeof(Screens.ShapeTest)
        };

        GraphicsDeviceManager graphics;
        float screenTimeRemaining = secondsPerScreen;
        int screenIndex = 0;

        public Game1() : base()
        {
            graphics = new GraphicsDeviceManager(this);

#if WINDOWS_PHONE || ANDROID || IOS

            // Frame rate is 30 fps by default for Windows Phone,
            // so let's keep that for other phones too
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
#elif WINDOWS || DESKTOP_GL
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
#endif


#if WINDOWS_8
            FlatRedBall.Instructions.Reflection.PropertyValuePair.TopLevelAssembly = 
                this.GetType().GetTypeInfo().Assembly;
#endif

        }

        protected override void Initialize()
        {
            #if IOS
            var bounds = UIKit.UIScreen.MainScreen.Bounds;
            var nativeScale = UIKit.UIScreen.MainScreen.Scale;
            var screenWidth = (int)(bounds.Width * nativeScale);
            var screenHeight = (int)(bounds.Height * nativeScale);
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            #endif
        
            FlatRedBallServices.InitializeFlatRedBall(this, graphics);

            Camera.Main.UsePixelCoordinates3D(0);
            IsMouseVisible = true;


            ScreenManager.Start(typeof(Screens.SpriteTest));

            base.Initialize();
        }


        protected override void Update(GameTime gameTime)
        {
            FlatRedBallServices.Update(gameTime);
            ScreenManager.Activity();

            ScreenTransitionActivity();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            FlatRedBallServices.Draw();

            base.Draw(gameTime);
        }

        private void ScreenTransitionActivity()
        {
            if(screenTimeRemaining <= 0)
            {
                screenIndex++;
                if(screenIndex > testScreens.Count - 1)
                {
                    screenIndex = 0;
                }
                screenTimeRemaining = secondsPerScreen;
                ScreenManager.MoveToScreen(testScreens[screenIndex]);
            }

            screenTimeRemaining -= TimeManager.SecondDifference;
        }
    }
}
