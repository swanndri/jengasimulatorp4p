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

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private PhysicsManager _physics;

        private TouchPoint _touchPosition, _lastTouchPosition;
        private TouchTarget touchTarget;
        private GestureRecognizer gestureRecognizer;

        private Color backgroundColor = Color.CornflowerBlue;
        private Matrix screenTransform = Matrix.Identity;

        private Texture2D _resetButtonTexture;
        private Texture2D _viewButtonTexture;
        
        private Texture2D _rotationSideSliderTexture;
        private Rectangle _rotationSideSliderRectangle;
        private Texture2D _rotationBottomSliderTexture;
        private Rectangle _rotationBottomSliderRectangle;

        private Texture2D _rotationSliderBallTexture;
        private Rectangle _rotationSideSliderBallRectangle;
        private Rectangle _rotationBottomSliderBallRectangle;

        private Boolean _resetFlag;

        private int _ScreenHeight;
        private int _ScreenWidth;

        private Overlay _HUD;
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {   
            graphics = new GraphicsDeviceManager(this);
            graphics.SynchronizeWithVerticalRetrace = false;
            
            _viewManager = new ViewManager(this);
            _viewManager.BackgroundColor = backgroundColor;

            _inputManager = new InputManager(this);            

            _physics = new PhysicsManager(this);            
            this.Components.Add(new PhysicsScene(this, _physics));
            
            gestureRecognizer = new GestureRecognizer(this, _viewManager, _physics);
            _resetFlag = false;
            Content.RootDirectory = "Content";
        }

        #region UIlistenercallbacks
        //These methods should really all be made into listeners and but cbf

        public void onButtonDown(String buttonName) {
            if (buttonName == "reset_button") {
                CreateScene();
            }            
        }

        public void onSlide(String sliderName, float slideRatio)
        {
            if (sliderName == "side_slider")
            {
                double radians = System.Convert.ToDouble(MathHelper.ToRadians((slideRatio * 89)));
                this.heightAngle = (float)radians;
                updateCameraPosition(rotationAngle, heightAngle, cameraDistance);
            }
            else if (sliderName == "bottom_slider") {
                double radians = System.Convert.ToDouble(MathHelper.ToRadians((slideRatio * 360)));
                this.rotationAngle = (float)radians;
                updateCameraPosition(rotationAngle, heightAngle, cameraDistance);
            }
        }

        #endregion

        #region Initialization

        private void InitVariables()
        {

            _ScreenHeight = GraphicsDevice.Viewport.Bounds.Height;
            _ScreenWidth = GraphicsDevice.Viewport.Bounds.Width;
            

            //Create rectangles for slider sprites
            _rotationSideSliderRectangle = new Rectangle(_ScreenWidth - 180, _ScreenHeight / 4, 150, _ScreenHeight / 2);
            _rotationBottomSliderRectangle = new Rectangle(_ScreenWidth / 4, _ScreenHeight - 200, _ScreenWidth / 2, 150);
            _rotationSideSliderBallRectangle = new Rectangle(_ScreenWidth - 130, _ScreenHeight / 4, 50, 50);
            _rotationBottomSliderBallRectangle = new Rectangle(_ScreenWidth / 4, _ScreenHeight - 165, 75, 75);

            rotationAngle = 0;
            heightAngle = MathHelper.ToRadians(1);

            //_viewManager.Position = new Vector3(15f, 0f, 5f); 
            updateCameraPosition(rotationAngle, heightAngle, cameraDistance);

        }
        
        
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
            _viewButtonTexture = Content.Load<Texture2D>(@"Sprites/reset_button_pressed");
            _rotationSideSliderTexture = Content.Load<Texture2D>(@"Sprites/Rotation Slider");
            _rotationBottomSliderTexture = Content.Load<Texture2D>(@"Sprites/Rotation Slider - Bottom");
            _rotationSliderBallTexture = Content.Load<Texture2D>(@"Sprites/circle");
        }

        private void CreateScene()
        {
            InitVariables();

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

                    //int randomNumber = random.Next(90, 100);
                    //float random1 = (float)randomNumber;

                    //float scale = (random1 / 100) * 1.0f;
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

        private void CreateHUD() {

            _HUD = new Overlay(this);

            Button resetButton = new Button(_resetButtonTexture, new Rectangle(0,0,165,70), "reset_button");
            resetButton.addButtonListener(this);
            _HUD.addUIComponent(resetButton);

            SliderBar sideSlider = new SliderBar(_rotationSideSliderTexture, _rotationSliderBallTexture, _rotationSideSliderRectangle,"side_slider", true);
            sideSlider.addSliderListener(this);
            _HUD.addUIComponent(sideSlider);

            SliderBar bottomSlider = new SliderBar(_rotationBottomSliderTexture, _rotationSliderBallTexture, _rotationBottomSliderRectangle, "bottom_slider", false);
            bottomSlider.addSliderListener(this);
            _HUD.addUIComponent(bottomSlider);


            this.Components.Add(_HUD);
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

        //Theta is angle in radians, radius is radius of sphere
        private void updateCameraPosition(float rotationAngle, float heightAngle, float radius)
        {

            double x = radius * Math.Sin(heightAngle) * Math.Cos(rotationAngle);
            double y = radius * Math.Sin(heightAngle) * Math.Sin(rotationAngle);
            double z = radius * Math.Cos(heightAngle);

            Vector3 cameraPosition = new Vector3((float)x, (float)y, (float)z);
            _viewManager.Position = cameraPosition;
        }

        protected override void Update(GameTime gameTime)
        {
            ReadOnlyTouchPointCollection touches = touchTarget.GetState();
            _lastTouchPosition = _touchPosition;

            if (touches.Count == 0)
            {
                _HUD.checkHitUI(null);
            }
            foreach (TouchPoint t in touches)
            {                
                _HUD.checkHitUI(t);
            }
   
            if (touches.Count == 0)
            {
                _resetFlag = false;
            }

            //BUTTON CODE - WILL NOT WORK WITH ACTUAL TOUCH
            if (touches.Count == 1)
            {
                _touchPosition = touches[0];                    
                    
                    Point p = new Point((int)_touchPosition.CenterX, (int)_touchPosition.CenterY);
                
                /*
                    if (_rotationBottomSliderRectangle.Contains(p))
                    {
                        //Console.Out.WriteLine(p);

                        float dFromLeft;
                        dFromLeft = (float)(p.X - _rotationBottomSliderRectangle.X);

                        float propOfSlider;
                        propOfSlider = dFromLeft / _rotationBottomSliderRectangle.Width;

                        int distance = (int)(((_ScreenWidth / 4) + dFromLeft) - (propOfSlider * 75));
                        _rotationBottomSliderBallRectangle = new Rectangle(distance, _ScreenHeight - 165, 75, 75);

                        double radians = System.Convert.ToDouble(MathHelper.ToRadians((propOfSlider * 360)));
                        this.rotationAngle = (float)radians;
                        updateCameraPosition(rotationAngle, heightAngle, cameraDistance);
                    }*/
            }


            gestureRecognizer.processTouchPoints(touches);
                
            _inputManager.CaptureMouse = this.IsActive && _inputManager.MouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            
            //COMMENT OUT FROM HERE
            /*
            // object picking
            if (_inputManager.WasPressed(MouseButton.MiddleButton))
            {
                Segment s;
                s.P1 = GraphicsDevice.Viewport.Unproject(new Vector3(_inputManager.MouseState.X, _inputManager.MouseState.Y, 0f),
                    _viewManager.Projection, _viewManager.View, Matrix.Identity);
                s.P2 = GraphicsDevice.Viewport.Unproject(new Vector3(_inputManager.MouseState.X, _inputManager.MouseState.Y, 1f),
                    _viewManager.Projection, _viewManager.View, Matrix.Identity);
                float scalar;
                Vector3 point;
                var c = _physics.BroadPhase.Intersect(ref s, out scalar, out point);
                if (c != null && c is BodySkin)
                {
                    _pickedObject = ((BodySkin)c).Owner;

                    _pickedForce = new WorldPointConstraint(_pickedObject, point);
                    _physics.Add(_pickedForce);
                    _pickedDistance = scalar;
                    _pickedObject.IsActive = true;
                }
            }
            else if (_inputManager.MouseState.MiddleButton == ButtonState.Pressed && _pickedObject != null)
            {
                Segment s;
                s.P1 = GraphicsDevice.Viewport.Unproject(new Vector3(_inputManager.MouseState.X, _inputManager.MouseState.Y, 0f),
                    _viewManager.Projection, _viewManager.View, Matrix.Identity);
                s.P2 = GraphicsDevice.Viewport.Unproject(new Vector3(_inputManager.MouseState.X, _inputManager.MouseState.Y, 1f),
                    _viewManager.Projection, _viewManager.View, Matrix.Identity);
                Vector3 diff, point;
                Vector3.Subtract(ref s.P2, ref s.P1, out diff);
                Vector3.Multiply(ref diff, _pickedDistance, out diff);
                Vector3.Add(ref s.P1, ref diff, out point);
                _pickedForce.WorldPoint = point;
                _pickedObject.IsActive = true;
            }
            else if (_pickedObject != null)
            {
                _physics.Remove(_pickedForce);
                _pickedObject = null;
            }
            */
            //COMMENT OUT TILL HERE

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
