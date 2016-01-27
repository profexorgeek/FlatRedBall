#region Using

using System;
using System.Collections.Generic;
using System.Reflection;
using FlatRedBall.Graphics;
using FlatRedBall.ManagedSpriteGroups;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Gui;
using FlatRedBall.Utilities;
using FlatRedBall.IO;
#if !SILVERLIGHT
#endif

#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;
#endif

#endregion

namespace FlatRedBall.Screens
{

    public static partial class ScreenManager
    {
        #region Fields

        static bool? mWasFixedTimeStep = null;
        static double? mLastTimeFactor = null;

        private static Screen mCurrentScreen;

        private static bool mSuppressStatePush = false;

        private static bool mWarnIfNotEmptyBetweenScreens = true;

        private static int mNumberOfFramesSinceLastScreenLoad = 0;

        private static Layer mNextScreenLayer;

        // The ScreenManager can be told to ignore certain objects which
        // we recognize will persist from screen to screen.  This should
        // NOT be used as a solution to get around the ScreenManager's check.
        private static PositionedObjectList<Camera> mPersistentCameras = new PositionedObjectList<Camera>();

        private static PositionedObjectList<SpriteFrame> mPersistentSpriteFrames = new PositionedObjectList<SpriteFrame>();

        private static PositionedObjectList<Text> mPersistentTexts = new PositionedObjectList<Text>();

        private static List<IDrawableBatch> mPersistentDrawableBatches = new List<IDrawableBatch>();

        #endregion

        #region Properties

        public static Assembly MainAssembly { get; private set; }

        public static Screen CurrentScreen
        {
            get { return mCurrentScreen; }
        }

        public static Layer NextScreenLayer
        {
            get { return mNextScreenLayer; }
        }

        public static PositionedObjectList<Camera> PersistentCameras
        {
            get { return mPersistentCameras; }
        }

        public static PositionedObjectList<SpriteFrame> PersistentSpriteFrames
        {
            get { return mPersistentSpriteFrames; }
        }

        public static List<IDrawableBatch> PersistentDrawableBatches
        {
            get
            {
                return mPersistentDrawableBatches;
            }
        }

        public static PositionedObjectList<Text> PersistentTexts
        {
            get { return mPersistentTexts; }
        }

        public static bool WarnIfNotEmptyBetweenScreens
        {
            get { return mWarnIfNotEmptyBetweenScreens; }
            set { mWarnIfNotEmptyBetweenScreens = value; }
        }
		
		public static bool ShouldActivateScreen
        {
            get;
            set;
        }
		
		public static Action<string> RehydrateAction
		{
			get;
			set;
		}
		
        #endregion

        #region Methods

        #region Public Methods

        #region XML Docs
        /// <summary>
        /// Calls activity on the current screen and checks to see if screen
        /// activity is finished.  If activity is finished, the current Screen's
        /// NextScreen is loaded.
        /// </summary>
        #endregion
        public static void Activity()
        {
            if (mCurrentScreen == null) return;

            mCurrentScreen.Activity(false);

            mCurrentScreen.ActivityCallCount++;

            if (mCurrentScreen.ActivityCallCount == 2 && mWasFixedTimeStep.HasValue)
            {
#if !FRB_MDX
                FlatRedBallServices.Game.IsFixedTimeStep = mWasFixedTimeStep.Value;
                TimeManager.TimeFactor = mLastTimeFactor.Value;
#endif
            }

            if (mCurrentScreen.IsActivityFinished)
            {
#if !FRB_MDX
                mWasFixedTimeStep = FlatRedBallServices.Game.IsFixedTimeStep;
                mLastTimeFactor = TimeManager.TimeFactor;

                FlatRedBallServices.Game.IsFixedTimeStep = false;
                TimeManager.TimeFactor = 0;
#endif

                GuiManager.Cursor.IgnoreNextClick = true;
                string type = mCurrentScreen.NextScreen;
                Screen asyncLoadedScreen = mCurrentScreen.mNextScreenToLoadAsync;

                mCurrentScreen.Destroy();

                // check to see if there is any leftover data
                CheckAndWarnIfNotEmpty();

                // Let's perform a GC here.  
                GC.Collect();

				// Not sure why this started to freeze on Android in the automated test project
				// on April 22, 2015. I'm commenting it out because I don't think we need to wait
				// for finalizers, and we can just continue on. Maybe try to bring the code back
				// on Android in the future too.
				#if !ANDROID
                GC.WaitForPendingFinalizers();
				#endif

                if (asyncLoadedScreen == null)
                {

                    // Loads the Screen, suspends input for one frame, and
                    // calls Activity on the Screen.
                    // The Activity call is required for objects like SpriteGrids
                    // which need to be managed internally.

                    // No need to assign mCurrentScreen - this is done by the 4th argument "true"
                    //mCurrentScreen = 
                    LoadScreen(type, null, true, true);

                    mNumberOfFramesSinceLastScreenLoad = 0;

                }
                else
                {

                    mCurrentScreen = asyncLoadedScreen;

                    mCurrentScreen.AddToManagers();

                    mCurrentScreen.Activity(true);


                    mCurrentScreen.ActivityCallCount++;
                    mNumberOfFramesSinceLastScreenLoad = 0;
                }
            }
            else
            {
                mNumberOfFramesSinceLastScreenLoad++;
            }
        }


        public static Screen LoadScreen(string screen, bool createNewLayer)
        {
            if (createNewLayer)
            {
                return LoadScreen(screen, SpriteManager.AddLayer());
            }
            else
            {
                return LoadScreen(screen, (Layer)null);
            }
        }


        public static T LoadScreen<T>(Layer layerToLoadScreenOn) where T : Screen
        {
            mNextScreenLayer = layerToLoadScreenOn;

#if XBOX360
            T newScreen = (T)Activator.CreateInstance(typeof(T));
#else
            T newScreen = (T)Activator.CreateInstance(typeof(T), new object[0]);
#endif

            FlatRedBall.Input.InputManager.CurrentFrameInputSuspended = true;

            newScreen.Initialize(true);

            newScreen.Activity(true);

            newScreen.ActivityCallCount++;

            return newScreen;
        }


        public static Screen LoadScreen(string screen, Layer layerToLoadScreenOn)
        {
            return LoadScreen(screen, layerToLoadScreenOn, true, false);
        }

        public static Screen LoadScreen(string screen, Layer layerToLoadScreenOn, bool addToManagers, bool makeCurrentScreen)
        {
            mNextScreenLayer = layerToLoadScreenOn;

            Screen newScreen = null;

            Type typeOfScreen = MainAssembly.GetType(screen);

            if (typeOfScreen == null)
            {
                throw new System.ArgumentException("There is no " + screen + " class defined in your project or linked assemblies.");
            }

            if (screen != null && screen != "")
            {
#if XBOX360
                newScreen = (Screen)Activator.CreateInstance(typeOfScreen);
#else
                newScreen = (Screen)Activator.CreateInstance(typeOfScreen, new object[0]);
#endif
            }

            if (newScreen != null)
            {
                FlatRedBall.Input.InputManager.CurrentFrameInputSuspended = true;

                if (addToManagers)
                {
                    // We do this so that new Screens are the CurrentScreen in Activity.
                    // This is useful in custom logic.
                    if (makeCurrentScreen)
                    {
                        mCurrentScreen = newScreen;
                    }
                    newScreen.Initialize(addToManagers);
                }
                mSuppressStatePush = false;

                if (addToManagers)
                {					
                    newScreen.Activity(true);


                    newScreen.ActivityCallCount++;
                }
            }

            return newScreen;
        }

        public static void Start<T>() where T : Screen, new()
        {
            mCurrentScreen = LoadScreen<T>(null);
        }

        #region XML Docs
        /// <summary>
        /// Loads a screen.  Should only be called once during initialization.
        /// </summary>
        /// <param name="screenToStartWith">Qualified name of the class to load.</param>
        #endregion
        public static void Start(Type screenToStartWithType)
        {

#if WINDOWS_8
            MainAssembly =
                screenToStartWithType.GetTypeInfo().Assembly;
#else
            MainAssembly =
                screenToStartWithType.Assembly;
#endif
            string screenToStartWith = screenToStartWithType.FullName;

            if (mCurrentScreen != null)
            {
                throw new InvalidOperationException("You can't call Start if there is already a Screen.  Did you call Start twice?");
            }
            else
            {
#if !FRB_MDX
                StateManager.Current.Initialize();

#endif

                if (ShouldActivateScreen && RehydrateAction != null)
                {
					RehydrateAction(screenToStartWith);
                }
                else
                {
                    mCurrentScreen = LoadScreen(screenToStartWith, null, true, true);

                    ShouldActivateScreen = false;
                }
            }
        }

        public static new string ToString()
        {
            if (mCurrentScreen != null)
                return mCurrentScreen.ToString();
            else
                return "No Current Screen";
        }

        #endregion

        #region Internal Methods

        internal static void Draw()
        {
            if(mCurrentScreen != null)
            {
                mCurrentScreen.HasDrawBeenCalled = true;
            }

        }


        #endregion

        #region Private Methods
        public static void CheckAndWarnIfNotEmpty()
        {

            if (WarnIfNotEmptyBetweenScreens)
            {
                List<string> messages = new List<string>();
                // the user wants to make sure that the Screens have cleaned up everything
                // after being destroyed.  Check the data to make sure it's all empty.

                // Currently we're not checking the GuiManager - do we want to?

                #region Make sure there's only 1 non-persistent Camera left
                if (SpriteManager.Cameras.Count > 1)
                {
                    int count = SpriteManager.Cameras.Count;

                    foreach (Camera camera in mPersistentCameras)
                    {
                        if (SpriteManager.Cameras.Contains(camera))
                        {
                            count--;
                        }
                    }

                    if (count > 1)
                    {
                        messages.Add("There are " + count +
                            " Cameras in the SpriteManager (excluding ignored Cameras).  There should only be 1.  See \"FlatRedBall.SpriteManager.Cameras\"");
                    }
                }
                #endregion

                #region Make sure that the Camera doesn't have any extra layers

                if (SpriteManager.Camera.Layers.Count > 1)
                {
                    messages.Add("There are " + SpriteManager.Camera.Layers.Count +
                        " Layers on the default Camera.  There should only be 1.  See \"FlatRedBall.SpriteManager.Camera.Layers\"");
                }

                #endregion

                #region Automatically updated Sprites
                if (SpriteManager.AutomaticallyUpdatedSprites.Count != 0)
                {
                    int spriteCount = SpriteManager.AutomaticallyUpdatedSprites.Count;

                    foreach (SpriteFrame spriteFrame in mPersistentSpriteFrames)
                    {
                        foreach (Sprite sprite in SpriteManager.AutomaticallyUpdatedSprites)
                        {
                            if (spriteFrame.IsSpriteComponentOfThis(sprite))
                            {
                                spriteCount--;
                            }
                        }
                    }

                    if (spriteCount != 0)
                    {
                        messages.Add("There are " + spriteCount +
                            " AutomaticallyUpdatedSprites in the SpriteManager. See \"FlatRedBall.SpriteManager.AutomaticallyUpdatedSprites\"");
                    }

                }
                #endregion

                #region Manually updated Sprites
                if (SpriteManager.ManuallyUpdatedSpriteCount != 0)
                {
                    messages.Add("There are " + SpriteManager.ManuallyUpdatedSpriteCount +
                        " ManuallyUpdatedSprites in the SpriteManager.  See \"SpriteManager.ManuallyUpdatedSpriteCount\"");
                }
                #endregion

                #region Ordered by distance Sprites

                if (SpriteManager.OrderedSprites.Count != 0)
                {
                    int spriteCount = SpriteManager.OrderedSprites.Count;

                    foreach (SpriteFrame spriteFrame in mPersistentSpriteFrames)
                    {
                        foreach (Sprite sprite in SpriteManager.OrderedSprites)
                        {
                            if (spriteFrame.IsSpriteComponentOfThis(sprite))
                            {
                                spriteCount--;
                            }
                        }
                    }

                    if (spriteCount != 0)
                    {
                        messages.Add("There are " + spriteCount +
                            " Ordered (Drawn) Sprites in the SpriteManager.  See \"FlatRedBall.SpriteManager.OrderedSprites\"");
                    }

                }

                #endregion

                #region Drawable Batches

                if (SpriteManager.DrawableBatches.Count != 0)
                {
                    int drawableBatchCount = 0;
                    foreach(var item in SpriteManager.DrawableBatches)
                    {
                        if(!PersistentDrawableBatches.Contains(item))
                        {
                            drawableBatchCount++;
                        }
                    }

                    if (drawableBatchCount > 0)
                    {
                        messages.Add("There are " + drawableBatchCount +
                            " DrawableBatches in the SpriteManager.  " +
                            "See  \"FlatRedBall.SpriteManager.DrawableBatches\"");
                    }
                }

                #endregion

                #region Managed Positionedobjects
                if (SpriteManager.ManagedPositionedObjects.Count != 0)
                {
                    messages.Add("There are " + SpriteManager.ManagedPositionedObjects.Count +
                        " Managed PositionedObjects in the SpriteManager.  See \"FlatRedBall.SpriteManager.ManagedPositionedObjects\"");
                }
                #endregion

                #region Layers
                if (SpriteManager.LayerCount != 0)
                {
                    
                    messages.Add("There are " + SpriteManager.LayerCount +
                        " Layers in the SpriteManager.  See \"FlatRedBall.SpriteManager.Layers\"");
                }
                #endregion

                #region TopLayer

                if (SpriteManager.TopLayer.Sprites.Count != 0)
                {
                    messages.Add("There are " + SpriteManager.TopLayer.Sprites.Count +
                        " Sprites in the SpriteManager's TopLayer.  See \"FlatRedBall.SpriteManager.TopLayer.Sprites\"");
                }

                #endregion

                #region Particles
                if (SpriteManager.ParticleCount != 0)
                {
                    messages.Add("There are " + SpriteManager.ParticleCount +
                        " Particle Sprites in the SpriteManager.  See \"FlatRedBall.SpriteManager.AutomaticallyUpdatedSprites\"");

                }
                #endregion

                #region SpriteFrames
                if (SpriteManager.SpriteFrames.Count != 0)
                {
                    int spriteFrameCount = SpriteManager.SpriteFrames.Count;

                    foreach (SpriteFrame spriteFrame in mPersistentSpriteFrames)
                    {
                        if (SpriteManager.SpriteFrames.Contains(spriteFrame))
                        {
                            spriteFrameCount--;
                        }
                    }

                    if (spriteFrameCount != 0)
                    {
                        messages.Add("There are " + spriteFrameCount +
                            " SpriteFrames in the SpriteManager.  See \"FlatRedBall.SpriteManager.SpriteFrames\"");
                    }

                }
                #endregion

                #region Text objects
                if (TextManager.AutomaticallyUpdatedTexts.Count != 0)
                {
                    int textCount = TextManager.AutomaticallyUpdatedTexts.Count;

                    foreach (Text text in mPersistentTexts)
                    {
                        if (TextManager.AutomaticallyUpdatedTexts.Contains(text))
                        {
                            textCount--;
                        }
                    }

                    if (textCount != 0)
                    {
                        messages.Add("There are " + textCount +
                            "automatically updated Texts in the TextManager.  See \"FlatRedBall.Graphics.TextManager.AutomaticallyUpdatedTexts\"");
                    }
                }
                #endregion

                #region Managed Shapes
                if (ShapeManager.AutomaticallyUpdatedShapes.Count != 0)
                {
                    messages.Add("There are " + ShapeManager.AutomaticallyUpdatedShapes.Count +
                        " Automatically Updated Shapes in the ShapeManager.  See \"FlatRedBall.Math.Geometry.ShapeManager.AutomaticallyUpdatedShapes\"");
                }
                #endregion

                #region  Visible Circles
                if (ShapeManager.VisibleCircles.Count != 0)
                {
                    messages.Add("There are " + ShapeManager.VisibleCircles.Count +
                        " visible Circles in the ShapeManager.  See \"FlatRedBall.Math.Geometry.ShapeManager.VisibleCircles\"");
                }
                #endregion

                #region Visible Rectangles

                if (ShapeManager.VisibleRectangles.Count != 0)
                {
                    messages.Add("There are " + ShapeManager.VisibleRectangles.Count +
                        " visible AxisAlignedRectangles in the VisibleRectangles.  See \"FlatRedBall.Math.Geometry.ShapeManager.VisibleRectangles\"");
                }
                #endregion

                #region Visible Polygons

                if (ShapeManager.VisiblePolygons.Count != 0)
                {
                    messages.Add("There are " + ShapeManager.VisiblePolygons.Count +
                        " visible Polygons in the ShapeManager.  See \"FlatRedBall.Math.Geometry.ShapeManager.VisiblePolygons\"");
                }
                #endregion

                #region Visible Lines

                if (ShapeManager.VisibleLines.Count != 0)
                {
                    messages.Add("There are " + ShapeManager.VisibleLines.Count +
                        " visible Lines in the ShapeManager.  See \"FlatRedBall.Math.Geometry.ShapeManager.VisibleLines\"");
                }
                #endregion

                if (messages.Count != 0)
                {
                    string errorString = "The Screen that was just unloaded did not clean up after itself:";
                    foreach (string s in messages)
                        errorString += "\n" + s;

                    throw new System.Exception(errorString);
                }
            }
        }
        #endregion

        #endregion
    }
}
