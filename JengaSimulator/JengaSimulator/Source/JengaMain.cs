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

namespace JengaSimulator
{
    public class App1 : Microsoft.Xna.Framework.Game
    {
        private float cameraDistance = 13;
        private float rotationAngle = 0;
        private float heightAngle = 0;

        private IViewManager _viewManager;
        private IInputManager _inputManager;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteBatch buttonBatch;

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
       
            Content.RootDirectory = "Content";
        }

        #region Initialization

        private void InitVariables()
        {

            _ScreenHeight = GraphicsDevice.PresentationParameters.Bounds.Height;
            _ScreenWidth = GraphicsDevice.PresentationParameters.Bounds.Width;
            _resetFlag = false;

            //Create rectangles for slider sprites
            _rotationSideSliderRectangle = new Rectangle(_ScreenWidth - 150, _ScreenHeight / 4, 150, _ScreenHeight / 2);
            _rotationBottomSliderRectangle = new Rectangle(_ScreenWidth / 4, _ScreenHeight - 200, _ScreenWidth / 2, 150);
            _rotationSideSliderBallRectangle = new Rectangle(_ScreenWidth - 130, _ScreenHeight / 4, 50, 50);
            _rotationBottomSliderBallRectangle = new Rectangle(_ScreenWidth / 4, _ScreenHeight - 195, 75, 75);

            _viewManager.Position = new Vector3(15f, 0f, 5f);     

        }
        private void CreateScene()
        {
            InitVariables();

            _physics.Clear();
            _physics.Gravity = new Vector3(0f, 0f, -9.8f);
            
           


            Model cubeModel = this.Content.Load<Model>("models/jenga_block");
            Model tableModel = this.Content.Load<Model>("models/table");

            var table = new SolidThing(this, tableModel);
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
                        cube.SetWorld(scale, new Vector3(1, i - 1, j * 0.5f), rotation);
                        
                    }
                    else
                    {
                        rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(0));
                        cube.SetWorld(scale, new Vector3(i, 0, j * 0.5f), rotation);
                    }
                    _physics.Add(cube);
                }
            }

            
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
            _viewManager.Position = new Vector3(15f, 0f, 5f);       
            _viewManager.UpAxis = Vector3.UnitZ;
            //_viewManager.Pitch = MathHelper.ToRadians(-15f);
            
            _viewManager.ForwardAxis = -Vector3.UnitX;
            _viewManager.MinPitch = MathHelper.ToRadians(-89.9f);
            _viewManager.MaxPitch = MathHelper.ToRadians(89.9f);

            CreateScene();
        }

        /// <summary>
        /// LoadContent will be called once per app and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            buttonBatch = new SpriteBatch(GraphicsDevice);
            _resetButtonTexture = Content.Load<Texture2D>(@"Sprites/easy");
            _viewButtonTexture = Content.Load<Texture2D>(@"Sprites/hard");
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

        //Theta is angle in radians, radius is radius of sphere
        private void updateCameraPosition(float rotationAngle, float heightAngle, float radius)
        {
            Console.Out.WriteLine(MathHelper.ToDegrees(rotationAngle));
            Console.Out.WriteLine(MathHelper.ToDegrees(heightAngle));
            Console.Out.WriteLine(radius);

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

            //_viewManager.Position = (Vector3.Add(_viewManager.Position , new Vector3(0.0f,0.1f,0.0f)));



            //BUTTON CODE - WILL NOT WORK WITH ACTUAL TOUCH
            if (touches.Count == 1)
            {
                _touchPosition = touches[0];

                    //Pressed reset button
                    if ((_touchPosition.CenterX < 165)&&(_touchPosition.CenterY < 70) && (_resetFlag == false))
                    {
                        _resetFlag = true;
                        CreateScene();
                       
                    }

                    
                    
                    Point p = new Point((int)_touchPosition.CenterX, (int)_touchPosition.CenterY);
                    if (_rotationSideSliderRectangle.Contains(p))
                    {
                        //Console.Out.WriteLine(p);

                        float dFromTop;
                        dFromTop = (float)(p.Y - _rotationSideSliderRectangle.Y);

                        float propOfSlider;
                        propOfSlider = dFromTop / _rotationSideSliderRectangle.Height;

                        int distance = (int)(((_ScreenHeight / 4) + dFromTop)-(50*propOfSlider));
                        _rotationSideSliderBallRectangle = new Rectangle(_ScreenWidth - 130, distance, 50, 50);

                        double radians = System.Convert.ToDouble(MathHelper.ToRadians((propOfSlider * 90)));
                        this.heightAngle = (float)radians;
                        updateCameraPosition(rotationAngle, heightAngle, cameraDistance);
                    }

                    if (_rotationBottomSliderRectangle.Contains(p))
                    {
                        //Console.Out.WriteLine(p);

                        float dFromLeft;
                        dFromLeft = (float)(p.X - _rotationBottomSliderRectangle.X);

                        float propOfSlider;
                        propOfSlider = dFromLeft / _rotationBottomSliderRectangle.Width;

                        int distance = (int)(((_ScreenWidth / 4) + dFromLeft) - (propOfSlider * 75));
                        _rotationBottomSliderBallRectangle = new Rectangle(distance, _ScreenHeight - 195, 75, 75);

                        double radians = System.Convert.ToDouble(MathHelper.ToRadians((propOfSlider * 360)));
                        this.rotationAngle = (float)radians;
                        updateCameraPosition(rotationAngle, heightAngle, cameraDistance);
                    }
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


         
            
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            base.Draw(gameTime);

            buttonBatch.Begin();
            buttonBatch.Draw(_resetButtonTexture, Vector2.Zero, Color.White);
            
            //buttonBatch.Draw(_viewButtonTexture, viewButtonPos, Color.White);

            buttonBatch.Draw(_rotationSideSliderTexture, _rotationSideSliderRectangle, Color.White);
            buttonBatch.Draw(_rotationBottomSliderTexture, _rotationBottomSliderRectangle, Color.White);
            buttonBatch.Draw(_rotationSliderBallTexture, _rotationSideSliderBallRectangle, Color.White);
            buttonBatch.Draw(_rotationSliderBallTexture, _rotationBottomSliderBallRectangle, Color.White);

            buttonBatch.End();

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
