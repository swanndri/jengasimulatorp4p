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
        private IViewManager _viewManager;
        private IInputManager _inputManager;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private PhysicsManager _physics;

        private RigidBody _pickedObject;
        private WorldPointConstraint _pickedForce;
        private float _pickedDistance;

        private TouchPoint _touchPosition, _lastTouchPosition;
        private TouchTarget touchTarget;

        private Color backgroundColor = Color.CornflowerBlue;
        private Matrix screenTransform = Matrix.Identity;

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

       
            Content.RootDirectory = "Content";
        }

        #region Initialization

        private void CreateScene()
        {
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
            _viewManager.Pitch = MathHelper.ToRadians(-15f);
            
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
        Vector3 cameraPosition = new Vector3(0, 6, 9);
        int rotateTime = 201666730;
        bool _shiftCamera = false;

        protected override void Update(GameTime gameTime)
        {
            ReadOnlyTouchPointCollection touches = touchTarget.GetState();
            _lastTouchPosition = _touchPosition;
            
            /*
            if (touches.Count == 1)
            {      
                _touchPosition = touches[0];
                //First time touch
                if (_lastTouchPosition == null)
                {
                    Segment s;
                    s.P1 = GraphicsDevice.Viewport.Unproject(new Vector3(_touchPosition.CenterX, _touchPosition.CenterY, 0f),
                        _viewManager.Projection, _viewManager.View, Matrix.Identity);
                    s.P2 = GraphicsDevice.Viewport.Unproject(new Vector3(_touchPosition.CenterX, _touchPosition.CenterY, 1f),
                        _viewManager.Projection, _viewManager.View, Matrix.Identity);
                    float scalar;
                    Vector3 point;
                    var c = _physics.BroadPhase.Intersect(ref s, out scalar, out point);
                            
                    if (c != null && c is BodySkin)
                    {
                        _pickedObject = ((BodySkin)c).Owner;

                        _pickedForce = new GrabConstraint(_pickedObject, point);
                        _physics.Add(_pickedForce);
                        _pickedDistance = scalar;
                        _pickedObject.IsActive = true;
                        _shiftCamera = false;
                    }
                    else
                    {
                        _shiftCamera = true;
                    }
                }else if (_shiftCamera == true){
                    _viewManager.Pitch  += (_lastTouchPosition.CenterY - _touchPosition.CenterY) * _inputManager.MouseSensitivity;
                    _viewManager.Yaw += (_lastTouchPosition.CenterX - _touchPosition.CenterX) * _inputManager.MouseSensitivity;
                }
                else if (_pickedObject != null)
                {
                    Segment s;
                    s.P1 = GraphicsDevice.Viewport.Unproject(new Vector3(_touchPosition.CenterX, _touchPosition.CenterY, 0f),
                        _viewManager.Projection, _viewManager.View, Matrix.Identity);
                    s.P2 = GraphicsDevice.Viewport.Unproject(new Vector3(_touchPosition.CenterX, _touchPosition.CenterY, 1f),
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
                    _shiftCamera = false;
                }
            }
            else if (touches.Count == 3)
            {
                Vector3 movement = Vector3.Zero;
                movement.Y -= 1f;
                _viewManager.Move(movement);
            }
            else if (touches.Count == 4)
            {
                Vector3 movement = Vector3.Zero;
                movement.Y += 1f;
                _viewManager.Move(movement);
            }
            else if (_pickedObject != null)
            {
                _physics.Remove(_pickedForce);
                _pickedObject = null;
                _touchPosition = null;
                _lastTouchPosition = null;
                _shiftCamera = false;
            }
            else
            {
                _touchPosition = null;
            }
            */
                          
                
            _inputManager.CaptureMouse = this.IsActive && _inputManager.MouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            
            
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
