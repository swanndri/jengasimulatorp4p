using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private List<int> holdingTouchPointIds;                     //All touchpoints that are in contact with the selected block.
        
        //SolidThing Object, Initial Orientation, Picked Distance and TouchPoint Offset
        private Tuple <SolidThing, Quaternion, float, Vector3> selectedBrick;        
        private Tuple <TouchPoint, long> previousTap;

        private Tuple<BlobPair, long> blockTangibleLastInContact;

        private BlobPair lastFineCameraInformation;
        private int holdingTouchPointID;                //Id of touchpoint.
        private float lastCorkScrewOrientation;
        private bool rotateOrZoom;
        private long begin;

        public ImprovedProcessor(Game game, IViewManager viewManager, PhysicsManager physics )
        {
            this._game = game;
            this._viewManager = viewManager;
            this._physics = physics;
            initialize();
        }

        public void initialize()
        {
            this.selectedBrick = null;
            this.previousTap = null;
            this.blockTangibleLastInContact = null;
            this.lastFineCameraInformation = null;

            this.rotateOrZoom = false;
            this.holdingTouchPointID = -1;

            this.activeTouchPoints = new Dictionary<int, TouchPoint>();
            this.holdingTouchPointIds = new List<int>();

            this.lastCorkScrewOrientation = -1;

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
            bool corkScrewOnTable = false;
            bool fineCameraOnTable = false;
            foreach (BlobPair bp in blobPairs)
            {             
                switch (bp.thisBlobPairTangible.Name){
                    case("Jenga Block"):
                        processBlockTangible(bp);
                        break;
                     case("Cork Screw"):
                        processCorkScrewTangible(bp);
                        corkScrewOnTable = true;
                        break;
                    case("Fine Camera"):
                        processFineCamera(bp);
                        fineCameraOnTable = true;
                        break;
                }
            }

            if (!corkScrewOnTable)
                this.lastCorkScrewOrientation = -1;
            if (!fineCameraOnTable)
                this.lastFineCameraInformation = null;

            //==========================================================================
            foreach (TouchPoint t in touches)
            {
                if (t.IsTagRecognized)
                {
                    switch (t.Tag.Value)
                    {
                        case JengaConstants.STACK_SIDE_0:
                            _viewManager.rotateToSide(0);
                            break;
                        case JengaConstants.STACK_SIDE_1:
                            _viewManager.rotateToSide(1);
                            break;
                        case JengaConstants.STACK_SIDE_2:
                            _viewManager.rotateToSide(2);
                            break;
                        case JengaConstants.STACK_SIDE_3:
                            _viewManager.rotateToSide(3);
                            break;
                        case JengaConstants.STACK_SIDE_4:
                            _viewManager.rotateToSide(4);
                            break;

                    }
                }
            }
            //==========================================================================
            //Get center positions of all finger touchpoints
            float x = 0, y = 0;
            int count = 0;
            int firstID = -1;

            foreach (TouchPoint activeTouchPoint in touches)
            {
                if (activeTouchPoint.IsFingerRecognized){
                    x += activeTouchPoint.X;
                    y += activeTouchPoint.Y;
                    firstID = activeTouchPoint.Id;
                    count++;
                }
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
                    if (t.IsFingerRecognized)
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
                catch (NullReferenceException e) {
                }
            }
            //Otherwise if we arent moving the block, move camera
            else if (holdingTouchPointID == -1)
            {
                rotateOrZoom = false;
                if (activeTouchPoints.Count > 0 && count > 0)
                {
                    Manipulator2D[] manipulators = null;
                    manipulators = new Manipulator2D[]{new Manipulator2D(firstID, x, y)};
                    try
                    {
                        manipulationProcessor.ProcessManipulators(Timestamp, manipulators);
                    }
                    catch (NullReferenceException e) { }
                    catch (InvalidOperationException e) { }
                }
            }
        }

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

                selectedBrick.Item1.IsActive = true;
                selectedBrick.Item1.SetWorld(newPosition, selectedBrick.Item2);
            }
        }
        private void OnManipulationCompleted(object sender, Manipulation2DCompletedEventArgs e) {}
        #endregion

        #region TouchEvents
        public void TouchDown(object sender, TouchEventArgs e)
        {
            TouchPoint t = e.TouchPoint;
            //bool fakeTap = wasFakeTap(t);

            //if (t.IsFingerRecognized && !fakeTap)ake
            if (t.IsFingerRecognized)
            {
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
                    }
                }
            }
            else if (!t.IsTagRecognized)
            {
                Tuple<SolidThing, Quaternion, float, Vector3> brick = getTouchedBlock(t.X, t.Y);
                if (selectedBrick == null && brick != null)
                {
                    selectedBrick = brick;
                    selectedBrick.Item1.IsWeightless = true;
                    selectedBrick.Item1._isSelected = true;
                    begin = Timestamp;
                }
            }
        }
        public void TouchHoldGesture(object sender, TouchEventArgs e)
        {
        }
        public void TouchMove(object sender, TouchEventArgs e)
        {            
            TouchPoint t = e.TouchPoint;
            long currentTime = Timestamp;

            if (t.IsFingerRecognized)
            {
                //MOVING BLOCKS
                if (t.Id == this.holdingTouchPointID && !rotateOrZoom)
                //if(t.Id == this.holdingTouchPointID)
                {
                    try
                    {
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
                    catch (NullReferenceException)
                    {
                        //IDK why it was throwing null here.
                    }
                }
            }
        }
        public void TouchTapGesture(object sender, TouchEventArgs e)
        {
            TouchPoint t = e.TouchPoint;
            if (t.IsFingerRecognized)
            {
                Tuple<TouchPoint, long> currentTap = new Tuple<TouchPoint, long>(t, DateTime.Now.Ticks);

                bool doubleTap = wasDoubleTap(this.previousTap, currentTap);
                
                if (doubleTap)
                {
                    //Faketap detects if double tap was triggered by tangible
                    bool fakeTap = wasFakeTap(t);
                    if (!fakeTap)
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

                    }
                }
                else
                {
                    this.previousTap = currentTap;
                }
            }
        }
        public void TouchUp(object sender, TouchEventArgs e)
        {
            TouchPoint t = e.TouchPoint;
            if (t.IsFingerRecognized)
            {
                holdingTouchPointIds.Remove(t.Id);

                //Remove touchpoint from active point lists
                lock (activeTouchPoints)
                {
                    activeTouchPoints.Remove(t.Id);
                }
                this.manipulationProcessor.CompleteManipulation(Timestamp);

                if (t.Id == this.holdingTouchPointID)                
                    this.holdingTouchPointID = -1;
            }            
        }
        #endregion

        #region Tangible Processing Helpers
        private void processBlockTangible(BlobPair bp)
        {
            this.blockTangibleLastInContact = new Tuple<BlobPair, long>(bp, Timestamp);
            if (selectedBrick != null)
            {
                Quaternion q = selectedBrick.Item1.Orientation;

                double yaw = Math.Atan2(2.0 * (q.Y * q.Z + q.W * q.X), q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);
                double pitch = Math.Asin(-2.0 * (q.X * q.Z - q.W * q.Y));
                double roll = Math.Atan2(2.0 * (q.X * q.Y + q.W * q.Z), q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);

                //float scaleFactor = ((Timestamp - begin) / JengaConstants.TIME_FOR_BLOCK_TO_CENTER);
                //scaleFactor = scaleFactor > 1 ? 1 : scaleFactor;
                float scaleFactor = 0;
                float totalRotation;

                //roll = roll < 0 ? (roll + (2 * Math.PI)) : roll;
                //totalRotation = (float)(((bp.Orientation - roll) * scaleFactor) + roll);

                float cameraRotationOffset = MathHelper.ToDegrees(_viewManager.RotationAngle) % 360;
                cameraRotationOffset = cameraRotationOffset < 0 ? 360 + cameraRotationOffset : cameraRotationOffset;
                cameraRotationOffset = 360 - cameraRotationOffset;

                totalRotation = MathHelper.ToRadians(MathHelper.ToDegrees(bp.Orientation) - cameraRotationOffset);

                Quaternion finalOrientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1.0f), totalRotation);

                Segment s;
                s.P1 = _game.GraphicsDevice.Viewport.Unproject(new Vector3(bp.CenterX, bp.CenterY, 0f),
                    _viewManager.Projection, _viewManager.DefaultView, Matrix.Identity);
                s.P2 = _game.GraphicsDevice.Viewport.Unproject(new Vector3(bp.CenterX, bp.CenterY, 1f),
                    _viewManager.Projection, _viewManager.DefaultView, Matrix.Identity);

                Vector3 diff, point;
                Vector3.Subtract(ref s.P2, ref s.P1, out diff);
                Vector3.Multiply(ref diff, this.selectedBrick.Item3, out diff);         //TODO FIX NULL REFERENCE(selectedblock)
                Vector3.Add(ref s.P1, ref diff, out point);

                Vector3 offset = Vector3.Multiply(this.selectedBrick.Item4, scaleFactor);
                Vector3 position = Vector3.Add(point, offset);


                if (!(Vector3.Subtract(position, selectedBrick.Item1.Position).Length() > JengaConstants.MAX_TANGIBLE_DISTANCE))
                {
                    selectedBrick = new Tuple<SolidThing, Quaternion, float, Vector3>
                        (selectedBrick.Item1, finalOrientation, selectedBrick.Item3, selectedBrick.Item4);

                    selectedBrick.Item1.SetVelocity(Vector3.Zero, Vector3.Zero);
                    selectedBrick.Item1.SetWorld(position, selectedBrick.Item2);
                    selectedBrick.Item1.IsActive = true;
                }
            }
        }
        private void processCorkScrewTangible(BlobPair bp)
        {
            if (this.selectedBrick != null)
            {
                selectedBrick.Item1.LinearVelocity = Vector3.Zero;
                if (this.lastCorkScrewOrientation != -1)
                {
                    float deltaDegrees = MathHelper.ToDegrees(bp.Orientation - this.lastCorkScrewOrientation);
                    deltaDegrees = deltaDegrees > 180 ? (float)(deltaDegrees - 360) : deltaDegrees;
                    deltaDegrees = deltaDegrees < -180 ? (float)(deltaDegrees + 360) : deltaDegrees;

                    Vector3 newPosition = selectedBrick.Item1.Position;
                    Vector3 direction = _viewManager.Position - selectedBrick.Item1.Position;
                    direction.Normalize();
                    float deltaAdd = deltaDegrees * JengaConstants.TANGIBLE_ZOOM_SCALE_FACTOR;
                    newPosition = Vector3.Add(Vector3.Multiply(direction, deltaAdd), selectedBrick.Item1.Position);

                    selectedBrick.Item1.IsActive = true;
                    selectedBrick.Item1.SetWorld(newPosition, selectedBrick.Item2);
                }
            }

            this.lastCorkScrewOrientation = bp.Orientation;
        }
        private void processFineCamera(BlobPair bp)
        {
            bool leftRightDisabled = false;

            if (this.lastFineCameraInformation != null)
            {
                float deltaDegrees = MathHelper.ToDegrees(bp.Orientation - this.lastFineCameraInformation.Orientation);
                deltaDegrees = deltaDegrees > 180 ? (float)(deltaDegrees - 360) : deltaDegrees;
                deltaDegrees = deltaDegrees < -180 ? (float)(deltaDegrees + 360) : deltaDegrees;
                deltaDegrees *= -1;

                float deltaX = bp.CenterX - this.lastFineCameraInformation.CenterX;
                float deltaY = bp.CenterY - this.lastFineCameraInformation.CenterY;

                float newHeightAngle = MathHelper.ToRadians((MathHelper.ToDegrees(_viewManager.HeightAngle)
                        + (JengaConstants.HEIGHT_REVERSED * deltaY / JengaConstants.PAN_SPEED_DIVISOR)));

                float newRotationAngle;
                if (leftRightDisabled)
                {
                    newRotationAngle = (float)(Math.PI - bp.Orientation);
                }
                else
                {
                    newRotationAngle = MathHelper.ToRadians((MathHelper.ToDegrees(_viewManager.RotationAngle)
                        + (JengaConstants.ROTATE_REVERSED * deltaX) / JengaConstants.PAN_SPEED_DIVISOR) + deltaDegrees);
                }
                
                _viewManager.updateCameraPosition(newRotationAngle, newHeightAngle, _viewManager.CameraDistance);
            }
            this.lastFineCameraInformation = bp;
        }
        #endregion

        #region General Methods
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
        private bool wasFakeTap(TouchPoint t)
        {
            Thread.Sleep(75);
            if (this.blockTangibleLastInContact != null)
            {
                long timeDifference = Timestamp - this.blockTangibleLastInContact.Item2;
                if (timeDifference < JengaConstants.TIME_BETWEEN_FAKE_TOUCH_AND_TANGIBLE)
                {
                    int x = (int)(this.blockTangibleLastInContact.Item1.CenterX - (JengaConstants.HIT_BOX_SIZE * 0.5));
                    int y = (int)(this.blockTangibleLastInContact.Item1.CenterY - (JengaConstants.HIT_BOX_SIZE * 0.5));
                    Rectangle hitBox = new Rectangle(x, y, JengaConstants.HIT_BOX_SIZE, JengaConstants.HIT_BOX_SIZE);
                    if (hitBox.Contains(new Point((int)t.CenterX, (int)t.CenterY)))
                    {
                        return true;
                    }
                }
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
