using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JengaSimulator.Source.UI;
using JengaSimulator.Source;

namespace JengaSimulator
{
    public sealed class ViewManager : DrawableGameComponent, IViewManager, SliderListener
	{
        private float cameraDistance = 13;
        public float CameraDistance { get { return cameraDistance; } }

        private float rotationAngle;
        public float RotationAngle { get { return rotationAngle; } }

        private float heightAngle;
        public float HeightAngle { get { return heightAngle; } }

		const float DefaultMaxPitch = float.PositiveInfinity;
		const float DefaultMinPitch = float.NegativeInfinity;
		static readonly Vector3 DefaultUpAxis = Vector3.UnitY;
		static readonly Vector3 DefaultForwardAxis = -Vector3.UnitZ;
		static readonly Vector3 DefaultSideAxis = Vector3.UnitX;

		private GraphicsDevice _device;		
		private Vector3 _position;
		private Vector3 _upAxis = DefaultUpAxis;
		private Vector3 _forwardAxis = DefaultForwardAxis;
		private Vector3 _sideAxis = DefaultSideAxis;
		private float _pitch = 0f;
		private float _yaw = 0f;
		private float _maxPitch = DefaultMaxPitch;
		private float _minPitch = DefaultMinPitch;

        private Matrix _viewMatrix;
        private Matrix _projectionMatrix;

        private Viewport _miniMapViewPort;

        private Color backgroundColor;

		public ViewManager(Game game)
			: base(game)
		{
			this.Game.Services.AddService(typeof(IViewManager), this);
			this.Game.Components.Add(this);
            this.backgroundColor = Color.Black;
		}

		public GraphicsDevice Device { get { return this.Game.GraphicsDevice; } }

        public Color BackgroundColor {
            get { return backgroundColor; }
            set
            {
                this.backgroundColor = value;
            }
        }
		public Vector3 UpAxis
		{
			get { return _upAxis; }
			set
			{
				_upAxis = value;
				Vector3.Cross(ref _forwardAxis, ref _upAxis, out _sideAxis);
			}
		}

		public Vector3 ForwardAxis
		{
			get { return _forwardAxis; }
			set
			{
				_forwardAxis = value;
				Vector3.Cross(ref _forwardAxis, ref _upAxis, out _sideAxis);
			}
		}

		public Vector3 Position { get { return _position; } set { _position = value; } }

		public Vector3 Direction
		{
			get
			{
				return Vector3.TransformNormal(
					_forwardAxis,
					Matrix.CreateFromAxisAngle(_sideAxis, _pitch) * Matrix.CreateFromAxisAngle(_upAxis, _yaw));
			}
			set
			{
				this.Pitch = (float)Math.Asin(Vector3.Dot(_upAxis, value));
				this.Yaw = (float)Math.Atan2(
					Vector3.Dot(-_forwardAxis, value), Vector3.Dot(-_sideAxis, value));
			}
		}

		public Matrix Projection { get { return _projectionMatrix; } set { _projectionMatrix = value; } }

		public Matrix View { get { return _viewMatrix; } }

		public float MaxPitch { get { return _maxPitch; } set { _maxPitch = value; } }

		public float MinPitch { get { return _minPitch; } set { _minPitch = value; } }

		public float Pitch
		{
			get { return _pitch; }
			set
			{
				_pitch = MathHelper.Clamp(value, _minPitch, _maxPitch);
			}
		}

		public float Yaw
		{
			get { return _yaw; }
			set
			{
				_yaw = (value >= MathHelper.TwoPi) ? value % MathHelper.TwoPi : value;
			}
		}

        

		public void SetProjection(float viewPlaneNear, float viewPlaneFar, float fieldOfView)
		{
			Matrix.CreatePerspectiveFieldOfView(
				fieldOfView,
				this.Game.GraphicsDevice.Viewport.AspectRatio,
				viewPlaneNear,
				viewPlaneFar,
				out _projectionMatrix);
		}

		public void SetProjection(float viewDistance)
		{
			Matrix.CreateOrthographicOffCenter(
				0f,
				this.Game.GraphicsDevice.Viewport.Width,
				0f,
				this.Game.GraphicsDevice.Viewport.Height,
				0f,
				viewDistance,
				out _projectionMatrix);
		}

		public void Move(Vector3 delta)
		{
			if (delta != Vector3.Zero)
			{
				Vector3 sideAxis;
				Vector3.Cross(ref _forwardAxis, ref _upAxis, out sideAxis);
				_position += Vector3.Transform(
					delta,
					Matrix.CreateFromAxisAngle(sideAxis, _pitch) * Matrix.CreateFromAxisAngle(_upAxis, _yaw));
			}
		}

		public override void Initialize()
		{
			base.Initialize();

			_device = this.Game.GraphicsDevice;
			_position = Vector3.Zero;
			_projectionMatrix = Matrix.Identity;

            _miniMapViewPort.X = 0;
            _miniMapViewPort.Y = 0;
            _miniMapViewPort.Width = 400;
            _miniMapViewPort.Height = 225;
            _miniMapViewPort.MaxDepth = 0.9f;
            _miniMapViewPort.MinDepth = 0.1f;
        }

        private Viewport defaultViewport;
        private Boolean isDefaultViewPort = true;
        private Vector3 positionSave;
        private Color colorSave;

        public void toggleViewPort() {
            Vector3 origin = Vector3.Zero;
            if (isDefaultViewPort)
            {
                _device.Clear(backgroundColor); 
                defaultViewport = _device.Viewport;
                _miniMapViewPort.X = defaultViewport.Width - _miniMapViewPort.Width;
                _device.Viewport = _miniMapViewPort;
                positionSave = _position;

                float x = (float)(13f * Math.Sin(89-heightAngle) * Math.Cos(360-rotationAngle));
                float y = (float)(13f * Math.Sin(89-heightAngle) * Math.Sin(360-rotationAngle));
                float z = (float)(13f * Math.Cos(89-heightAngle));

                _position = new Vector3(x, y, z);

                Matrix.CreateLookAt(
                ref _position,
                ref origin,
                ref _upAxis,
                out _viewMatrix);
                
                isDefaultViewPort = false;
                
            }
            else
            {
                _position = positionSave;
                _device.Viewport = defaultViewport;
                Matrix.CreateLookAt(
                ref _position,
                ref origin,
                ref _upAxis,
                out _viewMatrix);                
                isDefaultViewPort = true;
            }
            
        }
        
		public override void Draw(GameTime gameTime)
		{            
			_device.Clear(backgroundColor);           

			Vector3 look = this.Direction;
            Vector3 origin = Vector3.Zero;
			Vector3.Add(ref _position, ref look, out look);

			Matrix.CreateLookAt(
				ref _position,
                ref origin,
				ref _upAxis,
				out _viewMatrix);
            
			base.Draw(gameTime);
		}

        
        //Theta is angle in radians, radius is radius of sphere
        public void updateCameraPosition(float rotationAngle, float heightAngle, float radius)
        {
            heightAngle = heightAngle < JengaConstants.HEIGHT_ANGLE_MIN ? JengaConstants.HEIGHT_ANGLE_MIN : heightAngle;
            heightAngle = heightAngle > JengaConstants.HEIGHT_ANGLE_MAX ? JengaConstants.HEIGHT_ANGLE_MAX : heightAngle;

            float x = (float)(radius * Math.Sin(heightAngle) * Math.Cos(rotationAngle));
            float y = (float)(radius * Math.Sin(heightAngle) * Math.Sin(rotationAngle));
            float z = (float)(radius * Math.Cos(heightAngle));
            
            this.rotationAngle = rotationAngle;
            this.heightAngle = heightAngle;
            Vector3 cameraPosition = new Vector3(x, y, z);
            this.Position = cameraPosition;
        }

        private static double AngleDifference(double angle1, double angle2)
        {
            double diff = (angle2 - angle1 + 180) % 360 - 180;
            diff = diff < -180 ? diff + 360 : diff;
            diff = diff > 180 ? diff - 360 : diff;
            return diff;
        }

        private void updateCameraPositionSmoothly(float targetRotation, float targetHeight, float radius)
        {
            float speedFactor = 10f;
            float rotationDifference = (float)(AngleDifference(MathHelper.ToDegrees(targetRotation), MathHelper.ToDegrees(this.rotationAngle)));
            float heightDifference = MathHelper.ToDegrees(targetHeight) - MathHelper.ToDegrees(this.heightAngle);
            float newRotation, newHeight;

            newRotation = MathHelper.ToRadians(MathHelper.ToDegrees(this.rotationAngle) - (rotationDifference * speedFactor) / 180);
            newHeight = MathHelper.ToRadians(MathHelper.ToDegrees(this.heightAngle) + (heightDifference * speedFactor) / 89);

            updateCameraPosition(newRotation, newHeight, radius);       
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
                this.updateCameraPosition(rotationAngle, heightAngle, cameraDistance);
            }
            else if (sliderName == "bottom_slider")
            {
                double radians = System.Convert.ToDouble(MathHelper.ToRadians((slideRatio * 360)));
                this.rotationAngle = (float)radians;
                this.updateCameraPosition(rotationAngle, heightAngle, cameraDistance);
            }
        }

        public void rotateToSide(int sidesToRotate)
        {
            float rotationAngle = 0;
            double heightAngle = MathHelper.ToRadians(89);
            switch (sidesToRotate)
            {
                case 4:
                    heightAngle = System.Convert.ToDouble(MathHelper.ToRadians(1));
                    rotationAngle = MathHelper.ToRadians(270.0f);
                    break;
                case 5:
                    rotationAngle = MathHelper.ToRadians(0f);
                    break;
                case 6:
                    rotationAngle = MathHelper.ToRadians(90.0f);
                    break;
                case 7:
                    rotationAngle = MathHelper.ToRadians(180.0f);
                    break;                    
                case 8:
                    rotationAngle = MathHelper.ToRadians(270.0f);
                    break;
            }                
            //updateCameraPosition(rotationAngle, (float)heightAngle, 13f);
            updateCameraPositionSmoothly(rotationAngle, (float)heightAngle, 13f);
        }
	}
}
