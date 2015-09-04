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
        private int holdingTouchPointID;
        private Tuple<TouchPoint, long> previousTap;

        public ImprovedProcessor(Game game, IViewManager viewManager, PhysicsManager physics )
        {
            this._game = game;
            this._viewManager = viewManager;
            this._physics = physics;

            selectedBrick = null;
            previousTap = null;
            this.holdingTouchPointID = -1;

            Manipulations2D enabledManipulations = Manipulations2D.Rotate | Manipulations2D.Scale | Manipulations2D.Translate;
            manipulationProcessor = new ManipulationProcessor2D(enabledManipulations);

            manipulationProcessor.Pivot = new ManipulationPivot2D();
            manipulationProcessor.Pivot.Radius = 10;

            manipulationProcessor.Started += OnManipulationStarted;
            manipulationProcessor.Delta += OnManipulationDelta;
            manipulationProcessor.Completed += OnManipulationCompleted;
        }

        public void processTouchPoints(ReadOnlyTouchPointCollection touches, List<BlobPair> blobPairs, GameTime gameTime)
        {
        }

        /** Manipulation Events ********/
        #region ManipulationEvents
        private void OnManipulationStarted(object sender, Manipulation2DStartedEventArgs e)
        {
        }

        private void OnManipulationDelta(object sender, Manipulation2DDeltaEventArgs e)
        {
            if (this.holdingTouchPointID == -1)
            {
                float newHeightAngle = MathHelper.ToRadians((MathHelper.ToDegrees(_viewManager.HeightAngle)
                    + (JengaConstants.HEIGHT_REVERSED * e.Delta.TranslationY / JengaConstants.PAN_SPEED_DIVISOR)));
                float newRotationAngle = MathHelper.ToRadians((MathHelper.ToDegrees(_viewManager.RotationAngle)
                    + (JengaConstants.ROTATE_REVERSED * e.Delta.TranslationX / JengaConstants.PAN_SPEED_DIVISOR)));

                _viewManager.updateCameraPosition(newRotationAngle, newHeightAngle, _viewManager.CameraDistance);
            }
        }

        private void OnManipulationCompleted(object sender, Manipulation2DCompletedEventArgs e) { }
        #endregion

        #region TouchEvents
        public void TouchDown(object sender, TouchEventArgs e)
        {
            TouchPoint t = e.TouchPoint;

            if (selectedBrick != null)
            {
                SolidThing touchedBlock = getTouchedBlock(t);
                if (touchedBlock != null){
                    if (touchedBlock.Equals(this.selectedBrick))
                    {
                        this.holdingTouchPointID = t.Id;
                    }
                }               
            }
            
        }
        public void TouchHoldGesture(object sender, TouchEventArgs e)
        {
        }
        public void TouchMove(object sender, TouchEventArgs e)
        {
            TouchPoint t = e.TouchPoint;
            if (t.Id != this.holdingTouchPointID)
            {
                Manipulator2D[] manipulators;
                manipulators = new Manipulator2D[] { 
                    new Manipulator2D(1, t.X, t.Y)
                };
                manipulationProcessor.ProcessManipulators(Timestamp, manipulators);
            }
        }
        public void TouchTapGesture(object sender, TouchEventArgs e)
        {   
            TouchPoint t = e.TouchPoint;
            Tuple<TouchPoint, long> currentTap = new Tuple<TouchPoint, long>(t, DateTime.Now.Ticks);
            bool doubleTap = wasDoubleTap(this.previousTap, currentTap);
            if (doubleTap)
            {
                SolidThing brick = getTouchedBlock(t);
                if (brick != null)
                {
                    if (selectedBrick != null)
                    {
                        selectedBrick._isSelected = false;
                        if (selectedBrick.Equals(brick))
                        {
                            selectedBrick.IsWeightless = false;
                            selectedBrick = null;
                        }
                        else
                        {
                            selectedBrick = brick;
                            selectedBrick.IsWeightless = true;
                            selectedBrick._isSelected = true;
                        }
                    }
                    else
                    {
                        selectedBrick = brick;
                        selectedBrick.IsWeightless = true;
                        selectedBrick._isSelected = true;
                    }

                }
                this.previousTap = null;
            }
            else
            {
                this.previousTap = currentTap;
            }            
        }
        public void TouchUp(object sender, TouchEventArgs e)
        {
            
            TouchPoint t = e.TouchPoint;
            manipulationProcessor.CompleteManipulation(Timestamp);
           
            if (t.Id == this.holdingTouchPointID)
            {
                this.holdingTouchPointID = -1;                   
            }
        }
        #endregion

        #region Helper Methods
        private long Timestamp
        {
            get
            {
                // Get timestamp in 100-nanosecond units. 
                double nanosecondsPerTick = 1000000000.0 / System.Diagnostics.Stopwatch.Frequency;
                return (long)(System.Diagnostics.Stopwatch.GetTimestamp() / nanosecondsPerTick / 100.0);
            }
        }

        private bool wasDoubleTap(Tuple<TouchPoint, long> previous, Tuple<TouchPoint, long> current){
            if (previous == null || current == null)
                return false;
            TouchPoint previousPoint = previous.Item1;
            TouchPoint currentPoint = current.Item1;
            if (currentPoint.CenterX > previousPoint.CenterX - JengaConstants.BOUNDS_BUFFER_SIZE
                && currentPoint.CenterX < previousPoint.CenterX + JengaConstants.BOUNDS_BUFFER_SIZE
                && currentPoint.CenterY > previousPoint.CenterY - JengaConstants.BOUNDS_BUFFER_SIZE
                && currentPoint.CenterY < previousPoint.CenterY + JengaConstants.BOUNDS_BUFFER_SIZE
                && (current.Item2 - previous.Item2) < JengaConstants.MAX_TICK_DIFFERENCE)
            {
                this.previousTap = null;
                return true;
            }          
            return false;
        }

        private SolidThing getTouchedBlock(TouchPoint t)
        {
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
                return (SolidThing)(((BodySkin)c).Owner);                
            }
            return null;
        }

        #endregion
    }
}
