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

        private List<TouchPoint> activeTouchPoints;
        //Reference to actual object, initial orientation, pickeddistance and picked object offset
        private Tuple <SolidThing, Quaternion, float, Vector3> selectedBrick;        
        private Tuple <TouchPoint, long> previousTap;
        //Id of touchpoint. Offset to block coordinates (So we pick up block from edge or whereever user clicked on block)
        private int holdingTouchPointID;
        
        public ImprovedProcessor(Game game, IViewManager viewManager, PhysicsManager physics )
        {
            this._game = game;
            this._viewManager = viewManager;
            this._physics = physics;

            selectedBrick = null;
            previousTap = null;
            this.holdingTouchPointID = -1;
            this.activeTouchPoints = new List<TouchPoint>();

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
                Tuple<SolidThing, Quaternion, float, Vector3> touchedBlock = getTouchedBlock(t);
                if (touchedBlock != null)
                {
                    if (touchedBlock.Item1.Equals(this.selectedBrick.Item1))
                    {
                        this.selectedBrick = touchedBlock;
                        this.holdingTouchPointID = t.Id;
                    }
                }
                else
                {
                    this.manipulationProcessor.CompleteManipulation(Timestamp);
                }
            }
            else
            {
                this.manipulationProcessor.CompleteManipulation(Timestamp);
            }
        }
        public void TouchHoldGesture(object sender, TouchEventArgs e)
        {
        }
        public void TouchMove(object sender, TouchEventArgs e)
        {
            TouchPoint t = e.TouchPoint;            
            //MOVING CAMERA
            if (t.Id != this.holdingTouchPointID)
            {
                Manipulator2D[] manipulators;
                manipulators = new Manipulator2D[] { 
                    new Manipulator2D(1, t.X, t.Y)
                };
                manipulationProcessor.ProcessManipulators(Timestamp, manipulators);
            }
            //MOVING BLOCKS
            else
            {
                Segment s;
                s.P1 = _game.GraphicsDevice.Viewport.Unproject(new Vector3(t.X, t.Y, 0f),
                    _viewManager.Projection, _viewManager.View, Matrix.Identity);
                s.P2 = _game.GraphicsDevice.Viewport.Unproject(new Vector3(t.X, t.Y, 1f),
                    _viewManager.Projection, _viewManager.View, Matrix.Identity);

                Vector3 diff, point;
                Vector3.Subtract(ref s.P2, ref s.P1, out diff);
                Vector3.Multiply(ref diff, this.selectedBrick.Item3, out diff);
                Vector3.Add(ref s.P1, ref diff, out point);

                Vector3 position = Vector3.Add(point, this.selectedBrick.Item4);
                selectedBrick.Item1.SetVelocity(Vector3.Zero, Vector3.Zero);
                selectedBrick.Item1.SetWorld(position, selectedBrick.Item2);
                selectedBrick.Item1.IsActive = true;
            }
        }
        public void TouchTapGesture(object sender, TouchEventArgs e)
        {   
            TouchPoint t = e.TouchPoint;
            Tuple<TouchPoint, long> currentTap = new Tuple<TouchPoint, long>(t, DateTime.Now.Ticks);
            bool doubleTap = wasDoubleTap(this.previousTap, currentTap);
            //if (doubleTap)
            //{
                Tuple<SolidThing, Quaternion, float, Vector3> brick = getTouchedBlock(t);
                //If we touched a block
                if (brick != null)
                {
                    //If we are currently holding a block
                    if (selectedBrick != null)
                    {                        
                        //if we touched the same block we want to deselct it
                        if (selectedBrick.Item1.Equals(brick.Item1))
                        {
                            selectedBrick.Item1.IsWeightless = false;
                            selectedBrick.Item1.IsActive = true;
                            selectedBrick.Item1._isSelected = false;
                            selectedBrick = null;
                        }
                        //If we touched a new block with an block currently selected.
                        else
                        {
                            selectedBrick.Item1.IsWeightless = false;
                            selectedBrick.Item1._isSelected = false;
                            selectedBrick.Item1.IsActive = true;
                            selectedBrick = brick;
                            selectedBrick.Item1.IsWeightless = true;
                            selectedBrick.Item1._isSelected = true;
                        }
                    }
                    //If no block selected then select this one
                    else
                    {
                        selectedBrick = brick;
                        selectedBrick.Item1.IsWeightless = true;
                        selectedBrick.Item1._isSelected = true;
                    }

                }
                this.previousTap = null;
            //}
            //else
            //{
            //    this.previousTap = currentTap;
            //}            
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

        private Tuple<SolidThing, Quaternion, float, Vector3> getTouchedBlock(TouchPoint t)
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
                SolidThing selectedObject = (SolidThing)(((BodySkin)c).Owner);
                Quaternion initialRotation = selectedObject.Orientation;
                float pickedDistance = scalar;
                Vector3 offset = selectedObject.Position - point;;
                return new Tuple<SolidThing, Quaternion, float, Vector3>(selectedObject, initialRotation, pickedDistance,offset);                
            }
            return null;
        }

        #endregion
    }
}
