using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace JengaSimulator
{
	public sealed class InputManager : GameComponent, IInputManager
	{
		const int MouseCenterPositionX = 100;
		const int MouseCenterPositionY = 100;
		const float DefaultMouseSensitivity = 0.005f;
        const float DefaultTouchSensitivity = 0.000001f;

		private GamePadState _gamePadState, _lastGamePadState;
		private MouseState? _preCaptureMouseState;
		private MouseState _mouseState, _lastMouseState;
		private Vector2 _touchPosition, _lastTouchPosition;
		
		private KeyboardState _keyboardState, _lastKeyboardState;
		private float _mouseSensitivity = DefaultMouseSensitivity;
        private float _touchSensitivity = DefaultTouchSensitivity;
		private bool _captureMouse = true;

        private IViewManager _camera;
        private IInputManager _input;

        float movementSpeed = 10f;

		public InputManager(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(IInputManager), this);            
			game.Components.Add(this);

            _camera = (IViewManager)game.Services.GetService(typeof(IViewManager));
            _input = this;

            _input.CaptureMouse = true;
		}

		public GamePadState LastGamePadState { get { return _lastGamePadState; } }
		public GamePadState GamePadState { get { return _gamePadState; } }
		public MouseState LastMouseState { get { return LastMouseState; } }
		public MouseState MouseState { get { return _mouseState; } }
		public Vector2 LastTouchPosition { get { return _lastTouchPosition; } }
		public Vector2 TouchPosition { get { return _touchPosition; } }
		public KeyboardState KeyboardState { get { return _keyboardState; } }
		public float MouseSensitivity { get { return _mouseSensitivity; } set { _mouseSensitivity = value; } }
        public float TouchSensitivity { get { return _touchSensitivity; } set { _touchSensitivity = value; } }

		public Vector2 MouseDelta
		{
			get
			{
				return new Vector2(
					CaptureMouse ? _mouseState.X - MouseCenterPositionX : 0,
                    CaptureMouse ? MouseCenterPositionY - _mouseState.Y : 0);
			}
		}


		public bool CaptureMouse
		{
			get
			{
				return _captureMouse && this.Game.IsActive;
			}
			set
			{
				if (_captureMouse != value)
				{
					if (_captureMouse = value && this.Game.IsActive)
					{
						_preCaptureMouseState = _mouseState;
						Mouse.SetPosition(MouseCenterPositionX, MouseCenterPositionY);
						this.Game.IsMouseVisible = false;
					}
					else
					{
						if (_preCaptureMouseState != null)
						{
							Mouse.SetPosition(_preCaptureMouseState.Value.X, _preCaptureMouseState.Value.Y);
						}
						this.Game.IsMouseVisible = true;
					}
				}
			}
		}

		public IEnumerable<Keys> KeysPressed
		{
			get
			{
				foreach (Keys k in _keyboardState.GetPressedKeys())
				{
					if (!_lastKeyboardState.IsKeyDown(k))
						yield return k;
				}
			}
		}

		public bool WasPressed(Buttons button)
		{
			return _gamePadState.IsButtonDown(button) && !_lastGamePadState.IsButtonDown(button);
		}

		public bool WasPressed(MouseButton button)
		{
			switch (button)
			{
				case MouseButton.LeftButton:
					return _mouseState.LeftButton == ButtonState.Pressed && _lastMouseState.LeftButton != ButtonState.Pressed;
				case MouseButton.MiddleButton:
					return _mouseState.MiddleButton == ButtonState.Pressed && _lastMouseState.MiddleButton != ButtonState.Pressed;
				case MouseButton.RightButton:
					return _mouseState.RightButton == ButtonState.Pressed && _lastMouseState.RightButton != ButtonState.Pressed;
				case MouseButton.XButton1:
					return _mouseState.XButton1 == ButtonState.Pressed && _lastMouseState.XButton1 != ButtonState.Pressed;
				case MouseButton.XButton2:
					return _mouseState.XButton2 == ButtonState.Pressed && _lastMouseState.XButton2 != ButtonState.Pressed;
				default:
					return false;
			}
		}

		public override void Update(GameTime gameTime)
		{
			_lastKeyboardState = _keyboardState;
			_lastGamePadState = _gamePadState;
			_lastMouseState = _mouseState;
			_lastTouchPosition = _touchPosition;
			_gamePadState = GamePad.GetState(0);
			_mouseState = Mouse.GetState();
			_keyboardState = Keyboard.GetState();

			if (this.CaptureMouse)
			{
				Mouse.SetPosition(MouseCenterPositionX, MouseCenterPositionY);
			}

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 moveVector = Vector3.Zero;

            _camera.Pitch += _input.MouseDelta.Y * _input.MouseSensitivity;
            _camera.Yaw -= _input.MouseDelta.X * _input.MouseSensitivity;

            if (_input.KeyboardState.IsKeyDown(Keys.E) || _input.KeyboardState.IsKeyDown(Keys.W))
            {
                moveVector.X -= 1f;
            }
            if (_input.KeyboardState.IsKeyDown(Keys.A))
            {
                moveVector.Y -= 1f;
            }
            if (_input.KeyboardState.IsKeyDown(Keys.D))
            {
                moveVector.Y += 1f;
            }
            if (_input.KeyboardState.IsKeyDown(Keys.S))
            {
                moveVector.X += 1f;
            }

            if (moveVector != Vector3.Zero)
            {
                moveVector.Normalize();
                moveVector *= movementSpeed * delta;
                _camera.Move(moveVector);
            }

			base.Update(gameTime);
		}
	}
}
