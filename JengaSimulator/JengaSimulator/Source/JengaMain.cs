using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Henge3D;
using Henge3D.Physics;
using JengaSimulator.Source.UI;

namespace JengaSimulator
{
    public class App1 : Microsoft.Xna.Framework.Game, ButtonListener, SliderListener
    {
        private float cameraDistance = 13;
        private float rotationAngle;
        private float heightAngle;

        private IViewManager _viewManager;
        private IInputManager _inputManager;        
        private PhysicsManager _physics;
        private Overlay _HUD;
        private GestureRecognizer _gestureRecognizer;
        private Tangibles _tangibles;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;       

        private TouchTarget touchTarget;        

        private Color backgroundColor = Color.CornflowerBlue;
        private Matrix screenTransform = Matrix.Identity;

        private Texture2D _resetButtonTexture;
        private Texture2D _exitButtonTexture;
        private Texture2D _viewButtonTexture;        
        private Texture2D _rotationSideSliderTexture;
        private Texture2D _rotationBottomSliderTexture;
        private Texture2D _rotationSliderBallTexture;

        public Game instance;
        
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            this.IsFixedTimeStep = false;
            TaskManager.IsThreadingEnabled = false;
            instance = this;

            Content.RootDirectory = "Content";            

            graphics = new GraphicsDeviceManager(this);
            graphics.SynchronizeWithVerticalRetrace = false;

            _viewManager = new ViewManager(this);
            _viewManager.BackgroundColor = backgroundColor;

            _inputManager = new InputManager(this);

            _physics = new PhysicsManager(this);
            this.Components.Add(new PhysicsScene(this, _physics));

            _gestureRecognizer = new GestureRecognizer(this, _viewManager, _physics);
            _tangibles = new Tangibles(this, _viewManager, _physics);
            
        }

        public static Game Instance
        {
            get { return Instance; }
        }

        #region UIlistenercallbacks

        /// <summary>
        /// Called when a button that is registered is pressed. Called once on down press.
        /// </summary>
        public void onButtonDown(String buttonName) {
            if (buttonName == "reset_button") {
                CreateScene();
            }
            /*if (buttonName == "exit_button")
            {
                this.Exit();
            }*/
        }

        /// <summary>
        /// Called anytime user touches somewhere on a slider bar
        /// </summary>
        public void onSlide(String sliderName, float slideRatio)
        {
            if (sliderName == "side_slider")
            {
                double radians = System.Convert.ToDouble(MathHelper.ToRadians((slideRatio * 89)));
                this.heightAngle = (float)radians;
                _viewManager.updateCameraPosition(rotationAngle, heightAngle, cameraDistance);
            }
            else if (sliderName == "bottom_slider") {
                double radians = System.Convert.ToDouble(MathHelper.ToRadians((slideRatio * 360)));
                this.rotationAngle = (float)radians;
                _viewManager.updateCameraPosition(rotationAngle, heightAngle, cameraDistance);
            }
        }

        #endregion

        #region Initialization
        
        /// <summary>
        /// The target receiving all surface input for the application.
        /// </summary>
        protected TouchTarget TouchTarget
        {
            get { return touchTarget; }
        }

        /// <summary>
        /// Moves and sizes the window to cover the input surface.
        /// </summary>
        private void SetWindowOnSurface()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before SetWindowOnSurface is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;

            // Get the window sized right.
            Program.InitializeWindow(Window);
            // Set the graphics device buffers.
            graphics.PreferredBackBufferWidth = Program.WindowSize.Width;
            graphics.PreferredBackBufferHeight = Program.WindowSize.Height;
            //graphics.IsFullScreen = true;
            graphics.ApplyChanges();
            // Make sure the window is in the right location.
            Program.PositionWindow();
        }

        /// <summary>
        /// Initializes the surface input system. This should be called after any window
        /// initialization is done, and should only be called once.
        /// </summary>
        private void InitializeSurfaceInput()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before InitializeSurfaceInput is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;
            System.Diagnostics.Debug.Assert(touchTarget == null,
                "Surface input already initialized");
            if (touchTarget != null)
                return;

            // Create a target for surface input.
            touchTarget = new TouchTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);
            touchTarget.EnableInput();
        }

        #endregion

        #region Overridden Game Methods

        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            IsMouseVisible = true; // easier for debugging not to "lose" mouse
            SetWindowOnSurface();
            InitializeSurfaceInput();            
            base.Initialize();

            _viewManager.SetProjection(0.1f, 100f, MathHelper.ToRadians(45f));
            _viewManager.UpAxis = Vector3.UnitZ;
            
            _viewManager.ForwardAxis = -Vector3.UnitX;
            _viewManager.MinPitch = MathHelper.ToRadians(-89.9f);
            _viewManager.MaxPitch = MathHelper.ToRadians(89.9f);

            rotationAngle = 0;
            heightAngle = MathHelper.ToRadians(1);            
            _viewManager.updateCameraPosition(rotationAngle, heightAngle, cameraDistance);

            CreateScene();
            CreateHUD();
            
        }

        /// <summary>
        /// LoadContent will be called once per app and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            _resetButtonTexture = Content.Load<Texture2D>(@"Sprites/reset_button");
            _exitButtonTexture = Content.Load<Texture2D>(@"Sprites/reset_button_pressed");
            _viewButtonTexture = Content.Load<Texture2D>(@"Sprites/reset_button_pressed");
            _rotationSideSliderTexture = Content.Load<Texture2D>(@"Sprites/Rotation Slider");
            _rotationBottomSliderTexture = Content.Load<Texture2D>(@"Sprites/Rotation Slider - Bottom");
            _rotationSliderBallTexture = Content.Load<Texture2D>(@"Sprites/circle");
        }

        /// <summary>
        /// UnloadContent will be called once per app and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the app to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 

        protected override void Update(GameTime gameTime)
        {
            ReadOnlyTouchPointCollection touches = touchTarget.GetState();

            if (touches.Count == 0)            
                _HUD.checkHitUI(null);
            
            foreach (TouchPoint t in touches)                            
                _HUD.checkHitUI(t);
            
            _gestureRecognizer.processTouchPoints(touches);
            _tangibles.processTouchPoints(touches);
           
            _inputManager.CaptureMouse = this.IsActive && _inputManager.MouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            
            _physics.Integrate((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
            
        }

        /// <summary>
        /// This is called when the app should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(backgroundColor);

            base.Draw(gameTime);
        }

        #endregion

        #region Helper Methods

        private void CreateScene()
        {
            _physics.Clear();
            _physics.Gravity = new Vector3(0f, 0f, -9.8f);

            Model cubeModel = this.Content.Load<Model>("models/jenga_block");
            Model tableModel = this.Content.Load<Model>("models/table");

            SolidThing table = new SolidThing(this, tableModel, false);

            float tableScale = 3f;
            Vector3 tablePosition = new Vector3(0, 0, -1f);
            Quaternion tableRotation = Quaternion.Identity;

            table.SetWorld(tableScale, tablePosition, tableRotation);
            table.Freeze();
            _physics.Add(table);

            Random random = new Random();

            for (int j = 0; j < 6; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    var cube = new SolidThing(this, cubeModel);
                    float scale = 1.0f;
                    Quaternion rotation;

                    if (j % 2 == 1)
                    {
                        rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(90));
                        cube.SetWorld(scale, new Vector3(0, i - 1, j * 0.5f), rotation);

                    }
                    else
                    {
                        rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(0));
                        cube.SetWorld(scale, new Vector3(i - 1, 0, j * 0.5f), rotation);
                    }
                    _physics.Add(cube);
                }
            }


        }



        private void CreateHUD()
        {
            int _ScreenHeight, _ScreenWidth;

            _ScreenHeight = GraphicsDevice.PresentationParameters.Bounds.Height;
            _ScreenWidth = GraphicsDevice.PresentationParameters.Bounds.Width;

            Rectangle _rotationSideSliderRectangle = new Rectangle(_ScreenWidth - 180, _ScreenHeight / 4, _rotationSideSliderTexture.Width, _ScreenHeight / 2);
            Rectangle _rotationBottomSliderRectangle = new Rectangle(_ScreenWidth / 4, _ScreenHeight - 200, _ScreenWidth / 2, _rotationBottomSliderTexture.Height);

            _HUD = new Overlay(this);

            Button resetButton = new Button(_resetButtonTexture, new Rectangle(0, 0, 165, 70), "reset_button");
            resetButton.addButtonListener(this);
            _HUD.addUIComponent(resetButton);

            /*Button exitButton = new Button(_exitButtonTexture, new Rectangle(0,80,165,70), "exit_button");
            exitButton.addButtonListener(this);
            _HUD.addUIComponent(exitButton);*/



            SliderBar sideSlider = new SliderBar(_rotationSideSliderTexture, _rotationSliderBallTexture, _rotationSideSliderRectangle, "side_slider", true);
            sideSlider.addSliderListener(this);
            _HUD.addUIComponent(sideSlider);

            SliderBar bottomSlider = new SliderBar(_rotationBottomSliderTexture, _rotationSliderBallTexture, _rotationBottomSliderRectangle, "bottom_slider", false);
            bottomSlider.addSliderListener(this);
            _HUD.addUIComponent(bottomSlider);


            this.Components.Add(_HUD);
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.
                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null)
                {
                    graphicsDispose.Dispose();
                }
                if (touchTarget != null)
                {
                    touchTarget.Dispose();
                    touchTarget = null;
                }
            }

            // Release unmanaged Resources.

            // Set large objects to null to facilitate garbage collection.

            base.Dispose(disposing);
        }

        #endregion
    }
}
