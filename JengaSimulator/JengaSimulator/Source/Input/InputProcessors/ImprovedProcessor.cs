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
        private Dictionary<int, TouchPoint> activeTouchPoints;      //All active touch points in the original config (they do not update!)
        private List<int> holdingTouchPointIds;                //All touchpoints that are in contact with the selected block.

        private TouchPoint holdingTouchPoint;

        //Reference to actual object, initial orientation, pickeddistance and picked object offset
        private Tuple <SolidThing, Quaternion, float, Vector3> selectedBrick;        
        private Tuple <TouchPoint, long> previousTap;
        //Id of touchpoint. Offset to block coordinates (So we pick up block from edge or whereever user clicked on block)
        
        private int holdingTouchPointID;
        private bool rotateOrZoom;

        public ImprovedProcessor(Game game, IViewManager viewManager, PhysicsManager physics )
        {
            this._game = game;
            this._viewManager = viewManager;
            this._physics = physics;

            selectedBrick = null;
            previousTap = null;
            rotateOrZoom = false;
            this.holdingTouchPointID = -1;
            this.activeTouchPoints = new Dictionary<int, TouchPoint>();
            this.holdingTouchPointIds = new List<int>();
            this.holdingTouchPoint = null;

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
            float x = 0,y = 0;
            int count = 0;

            foreach (TouchPoint activeTouchPoint in touches)
            {
                x += activeTouchPoint.X;
                y += activeTouchPoint.Y;
                count++;
            }

            x = x / count;
            y = y / count;

            Tuple<SolidThing, Quaternion, float, Vector3> middleBlock = null;
 
            if (count > 0)
                middleBlock = getTouchedBlock(x, y);
            
            //Rotation or zoom block
            if (selectedBrick != null && middleBlock != null && selectedBrick.Item1.Equals(middleBlock.Item1) && activeTouchPoints.Count > 1)
            {
                //holdingTouchPointID = -1;
                List<Manipulator2D> manipulatorList = new List<Manipulator2D>();
                foreach (TouchPoint t in touches)
                {    
                    manipulatorList.Add(new Manipulator2D(t.Id, t.X, t.Y));
                }
                Manipulator2D[] manipulators = null;
                manipulators = manipulatorList.ToArray();

                try
                {
                    rotateOrZoom = true;                    
                    manipulationProcessor.Pivot.X = selectedBrick.Item1.Position.X;
                    manipulationProcessor.Pivot.Y = selectedBrick.Item1.Position.Y;                    
                    manipulationProcessor.ProcessManipulators(Timestamp, manipulators);         //TODO FIXED COLLECTION MODIFIED                   
                }
                catch (NullReferenceException e) {}
            }
            //Otherwise if we arent moving the block, move camera
            else if (holdingTouchPointID == -1)
            {
                rotateOrZoom = false;
                if (activeTouchPoints.Count > 0)
                {
                    Manipulator2D[] manipulators = null;

                    int id = activeTouchPoints.ElementAt(0).Value.Id;       // TODO FIX INVALUD OPERATION EXCEPTION
                    foreach (TouchPoint t in touches)
                    {
                        if (t.Id == id)
                        {
                            manipulators = new Manipulator2D[]{
                                new Manipulator2D(t.Id, t.X, t.Y)
                            };
                            break;
                        }
                    }
                    try
                    {
                        manipulationProcessor.ProcessManipulators(Timestamp, manipulators);
                    }
                    catch (NullReferenceException e) { }
                }
            }
        }

        /** Manipulation Events ********/
        #region ManipulationEvents
        private void OnManipulationStarted(object sender, Manipulation2DStartedEventArgs e){ }
        private void OnManipulationDelta(object sender, Manipulation2DDeltaEventArgs e)
        {
            if (!rotateOrZoom)
            {
                float newHeightAngle = MathHelper.ToRadians((MathHelper.ToDegrees(_viewManager.HeightAngle)
                    + (JengaConstants.HEIGHT_REVERSED * e.Delta.TranslationY / JengaConstants.PAN_SPEED_DIVISOR)));
                float newRotationAngle = MathHelper.ToRadians((MathHelper.ToDegrees(_viewManager.RotationAngle)
                    + (JengaConstants.ROTATE_REVERSED * e.Delta.TranslationX / JengaConstants.PAN_SPEED_DIVISOR)));

                _viewManager.updateCameraPosition(newRotationAngle, newHeightAngle, _viewManager.CameraDistance);
            }else{              
                //Rotations================================================
                float toRotate = MathHelper.ToDegrees(e.Delta.Rotation);
                Quaternion q = selectedBrick.Item1.Orientation;

                double yaw = Math.Atan2(2.0 * (q.Y * q.Z + q.W * q.X), q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);
                double pitch = Math.Asin(-2.0 * (q.X * q.Z - q.W * q.Y));
                double roll = Math.Atan2(2.0 * (q.X * q.Y + q.W * q.Z), q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);

                float currentRotation = MathHelper.ToDegrees((float)roll);
                float finalRotation = currentRotation - toRotate;
                if (finalRotation < 0)
                {
                    finalRotation = 360 + finalRotation;
                }
                if (finalRotation > 360)
                {
                    finalRotation = 360 - finalRotation;
                }

                float totalRotation = MathHelper.ToRadians(finalRotation);
                Quaternion finalOrientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1.0f), totalRotation);

                selectedBrick = new Tuple<SolidThing, Quaternion, float, Vector3>
                        (selectedBrick.Item1, finalOrientation, selectedBrick.Item3, selectedBrick.Item4);

                //Zooms==================================================
                Vector3 newPosition = selectedBrick.Item1.Position;
                Vector3 direction = _viewManager.Position - selectedBrick.Item1.Position;                               
                direction.Normalize();
                float deltaAdd = 1 - e.Delta.ScaleX;
                deltaAdd *= JengaConstants.FORWARD_BACK_BLOCK_SPEED;
                newPosition = Vector3.Add(Vector3.Multiply(direction, deltaAdd), selectedBrick.Item1.Position);

                selectedBrick.Item1.SetWorld(newPosition, selectedBrick.Item2);

                /*
                float distance;
                if (e.Delta.ScaleX > 1){
                    distance = (2 - JengaConstants.FORWARD_BACK_SCALE_CONSTANT) * selectedBrick.Item3;
                }else if (e.Delta.ScaleX < 1){
                    distance = JengaConstants.FORWARD_BACK_SCALE_CONSTANT * selectedBrick.Item3;             
                }else{
                    distance = selectedBrick.Item3;
                }                
                
                selectedBrick = new Tuple<SolidThing, Quaternion, float, Vector3>
                        (selectedBrick.Item1, finalOrientation,distance , selectedBrick.Item4);
                 */
            }
        }
        private void OnManipulationCompleted(object sender, Manipulation2DCompletedEventArgs e) {}
        #endregion

        #region TouchEvents
        public void TouchDown(object sender, TouchEventArgs e)
        {
            TouchPoint t = e.TouchPoint;
            this.activeTouchPoints.Add(t.Id, t);

            Tuple<SolidThing, Quaternion, float, Vector3> touchedBlock = getTouchedBlock(t.X, t.Y);
            
            //Conditions for camera movement
            if (selectedBrick == null || touchedBlock == null || !selectedBrick.Item1.Equals(touchedBlock.Item1))
            {
                this.manipulationProcessor.CompleteManipulation(Timestamp);
            }
            //Else we are moving a block (or possibly selecting it)
            else
            {               
                if (this.selectedBrick != null && touchedBlock.Item1.Equals(this.selectedBrick.Item1))
                {
                    this.selectedBrick = touchedBlock;
                    this.holdingTouchPointIds.Add(t.Id);
                    this.holdingTouchPointID = t.Id;
                    this.holdingTouchPoint = t;
                }
            }
        }
        public void TouchHoldGesture(object sender, TouchEventArgs e)
        {
        }
        public void TouchMove(object sender, TouchEventArgs e)
        {            
            TouchPoint t = e.TouchPoint;

            //MOVING BLOCKS
            if (t.Id == this.holdingTouchPointID)
            {
                try
                {

                    this.holdingTouchPoint = t;
                    rotateOrZoom = false;

                    Segment s;
                    s.P1 = _game.GraphicsDevice.Viewport.Unproject(new Vector3(t.X, t.Y, 0f),
                        _viewManager.Projection, _viewManager.DefaultView, Matrix.Identity);
                    s.P2 = _game.GraphicsDevice.Viewport.Unproject(new Vector3(t.X, t.Y, 1f),
                        _viewManager.Projection, _viewManager.DefaultView, Matrix.Identity);

                    Vector3 diff, point;
                    Vector3.Subtract(ref s.P2, ref s.P1, out diff);
                    Vector3.Multiply(ref diff, this.selectedBrick.Item3, out diff);         //TODO FIX NULL REFERENCE(selectedblock)
                    Vector3.Add(ref s.P1, ref diff, out point);

                    Vector3 position = Vector3.Add(point, this.selectedBrick.Item4);

                    selectedBrick.Item1.SetVelocity(Vector3.Zero, Vector3.Zero);
                    selectedBrick.Item1.SetWorld(position, selectedBrick.Item2);
                    selectedBrick.Item1.IsActive = true;
                }
                catch (NullReferenceException) { 
                    //IDK why it was throwing null here.
                }
            }            
        }
        public void TouchTapGesture(object sender, TouchEventArgs e)
        {
            TouchPoint t = e.TouchPoint;
            Tuple<TouchPoint, long> currentTap = new Tuple<TouchPoint, long>(t, DateTime.Now.Ticks);
           
            bool doubleTap = wasDoubleTap(this.previousTap, currentTap);
            if (doubleTap)
            {
            Tuple<SolidThing, Quaternion, float, Vector3> brick = getTouchedBlock(t.X, t.Y);
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
            }
            else
            {
                this.previousTap = currentTap;
            }            
        }
        public void TouchUp(object sender, TouchEventArgs e)
        {
            TouchPoint t = e.TouchPoint;
            holdingTouchPointIds.Remove(t.Id);

            //Remove touchpoint from active point lists
            lock (activeTouchPoints)
            {
                activeTouchPoints.Remove(t.Id);
            }
            if (activeTouchPoints.Count == 0)
            {
                this.manipulationProcessor.CompleteManipulation(Timestamp);
            }
            
            if (t.Id == this.holdingTouchPointID)
            {
                this.holdingTouchPointID = -1;
                this.holdingTouchPoint = null;
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

        private Tuple<SolidThing, Quaternion, float, Vector3> getTouchedBlock(float x, float y)
        {
            Segment s;            
            s.P1 = _game.GraphicsDevice.Viewport.Unproject(new Vector3(x, y, 0f),
                _viewManager.Projection, _viewManager.DefaultView, Matrix.Identity);
            s.P2 = _game.GraphicsDevice.Viewport.Unproject(new Vector3(x, y, 1f),
                _viewManager.Projection, _viewManager.DefaultView, Matrix.Identity);
            float scalar;
            Vector3 point;
            var c = _physics.BroadPhase.Intersect(ref s, out scalar, out point);
            
            if (c != null && c is BodySkin && !((SolidThing)((BodySkin)c).Owner).getIsTable())
            {
                SolidThing selectedObject = (SolidThing)(((BodySkin)c).Owner);
                Quaternion initialRotation = selectedObject.Orientation;
                float pickedDistance = scalar;
                Vector3 offset = selectedObject.Position - point;

                return new Tuple<SolidThing, Quaternion, float, Vector3>(selectedObject, initialRotation, pickedDistance,offset);                
            }
            return null;
        }

        #endregion
    }
}
