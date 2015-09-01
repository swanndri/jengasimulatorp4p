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
using System.Windows.Input.Manipulations;
using JengaSimulator.Source.Input.InputProcessors;

namespace JengaSimulator.Source.Input.InputProcessors
{
    class ImprovedProcessor : InputProcessor
    {
        private Game _game;
        private IViewManager _viewManager;
        private PhysicsManager _physics;

        private ManipulationProcessor2D manipulationProcessor;
        private SolidThing selectedBrick;

        public ImprovedProcessor(Game game, IViewManager viewManager, PhysicsManager physics )
        {
            this._game = game;
            this._viewManager = viewManager;
            this._physics = physics;

            selectedBrick = null;

            Manipulations2D enabledManipulations = Manipulations2D.Rotate | Manipulations2D.Scale | Manipulations2D.Translate;
            manipulationProcessor = new ManipulationProcessor2D(enabledManipulations);

            manipulationProcessor.Pivot = new ManipulationPivot2D();
            manipulationProcessor.Pivot.Radius = 10;

            manipulationProcessor.Started += OnManipulationStarted;
            manipulationProcessor.Delta += OnManipulationDelta;
            manipulationProcessor.Completed += OnManipulationCompleted;
        }

        public void processTouchPoints(ReadOnlyTouchPointCollection touches, List<BlobPair> blobPairs)
        {
            /*
             * WORKS TO ROTATE THE CAMERA
             * 
             * if (touches.Count == 1)
            {
                Manipulator2D[] manipulators;
                manipulators = new Manipulator2D[] { 
                    new Manipulator2D(1, touches[0].X, touches[0].Y)
                };
                manipulationProcessor.ProcessManipulators(Timestamp, manipulators);
            }*/           
        }

        /** Manipulation Events ********/
        #region ManipulationEvents
        private void OnManipulationStarted(object sender, Manipulation2DStartedEventArgs e)
        {
        }

        private void OnManipulationDelta(object sender, Manipulation2DDeltaEventArgs e)
        {
            /*
             * WORKS TO ROTATE CAMERA
             * 
            float newHeightAngle = MathHelper.ToRadians((MathHelper.ToDegrees(_viewManager.HeightAngle)
                + (JengaConstants.HEIGHT_REVERSED * e.Delta.TranslationY / JengaConstants.PAN_SPEED_DIVISOR)));
            float newRotationAngle = MathHelper.ToRadians((MathHelper.ToDegrees(_viewManager.RotationAngle)
                + (JengaConstants.ROTATE_REVERSED * e.Delta.TranslationX / JengaConstants.PAN_SPEED_DIVISOR)));

            _viewManager.updateCameraPosition(newRotationAngle, newHeightAngle, _viewManager.CameraDistance); 
             */
        }

        private void OnManipulationCompleted(object sender, Manipulation2DCompletedEventArgs e) { }
        #endregion

        #region TouchEvents
        public void TouchDown(object sender, TouchEventArgs e)
        {
        }
        public void TouchHoldGesture(object sender, TouchEventArgs e)
        {
        }
        public void TouchMove(object sender, TouchEventArgs e)
        {
        }

        public void TouchTapGesture(object sender, TouchEventArgs e)
        {
            TouchPoint t = e.TouchPoint;
            Segment s;
            s.P1 = _game.GraphicsDevice.Viewport.Unproject(new Vector3(t.X, t.Y, 0f),
                _viewManager.Projection, _viewManager.View, Matrix.Identity);
            s.P2 = _game.GraphicsDevice.Viewport.Unproject(new Vector3(t.X, t.Y, 1f),
                _viewManager.Projection, _viewManager.View, Matrix.Identity);
            float scalar;
            Vector3 point;
            var c = _physics.BroadPhase.Intersect(ref s, out scalar, out point);

            if (c != null && c is BodySkin && !((SolidThing)((BodySkin)c).Owner).getIsTable())
            {
                if(selectedBrick != null)
                    selectedBrick._isSelected = false;
                selectedBrick = (SolidThing)(((BodySkin)c).Owner);
                selectedBrick.IsWeightless = true;
                selectedBrick._isSelected = true;
            }
        }
        public void TouchUp(object sender, TouchEventArgs e)
        {
            /*
             * 
             * WORKS TO ROTATE CAMERA
            manipulationProcessor.CompleteManipulation(Timestamp);
             * 
             */
        }
        #endregion

        private long Timestamp
        {
            get
            {
                // Get timestamp in 100-nanosecond units. 
                double nanosecondsPerTick = 1000000000.0 / System.Diagnostics.Stopwatch.Frequency;
                return (long)(System.Diagnostics.Stopwatch.GetTimestamp() / nanosecondsPerTick / 100.0);
            }
        }
    }
}
