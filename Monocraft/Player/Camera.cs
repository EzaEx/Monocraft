using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Monocraft
{
    [Serializable]
    public class Camera
    {
        //camera settings
        private readonly float
        _FOV,
        _aspectRatio,
        _nearClippingPlane,
        _farClippingPlane,
        _targetDistance; 

        //properties
        private Vector2 _originalMousePos;
        private float _sensitivity;
        private Vector3 _directionXZFixed;       
        private Vector3 _directionPointing;       
        private Vector3 _upVector;
        private Vector3 _position; 
        //screen width and height
        private int bbw;
        private int bbh;

        public Vector3 Position { get => _position; set => _position = value; }
        public Vector3 DirectionPointing { get => _directionPointing; set => _directionPointing = value; }

        public Camera(GraphicsDeviceManager graphics, Vector3 position, Vector3 direction)
        { 
            //set default properties
            _FOV = MathHelper.ToRadians(90);
            //_graphics = graphics; 
            bbw = graphics.PreferredBackBufferWidth;
            bbh = graphics.PreferredBackBufferHeight;
            _aspectRatio = (graphics.PreferredBackBufferWidth) / (float)graphics.PreferredBackBufferHeight;
            _nearClippingPlane = 0.03f;
            _farClippingPlane = 400f;
            _position = position;
            _upVector = Vector3.UnitY;
            _directionPointing = direction;
            _targetDistance = 10;
            _sensitivity = 0.004f;

            //get mouse to centre
            Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2); 

            //store original mouse pos
            _originalMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            _directionXZFixed = direction;
        }

        //get matricies required for 3d rendering (settings matrix)
        public Matrix GetProjectionMatrix()
        {
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(_FOV, _aspectRatio, _nearClippingPlane, _farClippingPlane);
            return projection;
        }
        //get matricies required for 3d rendering (viewpoint matrix)
        public Matrix GetViewMatrix()
        {
            Vector3 target = _position + _directionPointing * _targetDistance;
            Matrix view = Matrix.CreateLookAt(_position, target, _upVector);
            return view;
        }

        //update cam position and point at mouse
        public void Update(Vector3 position)
        {
            Position = position;
            PointAtMouse();
        }

        //move camera based on mouse movement
        public Vector3 PointAtMouse()
        { 
            //get mouse info 
            MouseState mouse = Mouse.GetState();

            //calc mouse movement since last frame
            float xMovement = _originalMousePos.X - mouse.X;
            float yMovement = _originalMousePos.Y - mouse.Y;

            Mouse.SetPosition((int)_originalMousePos.X, (int)_originalMousePos.Y);

            //vector representing camera y rotation
            _directionXZFixed = Vector3.Transform(_directionXZFixed, Matrix.CreateRotationY(xMovement * _sensitivity));

            //rotate camera about its y axis
            Matrix yRotation = Matrix.CreateRotationY(xMovement * _sensitivity);
            _directionPointing = Vector3.Transform(_directionPointing, yRotation);

            //rotate camera about x and z axes
            Matrix zRotation = Matrix.CreateFromAxisAngle(Vector3.Cross(_upVector, _directionXZFixed), -yMovement * _sensitivity);
            if (Math.Acos(Vector3.Dot(Vector3.Transform(_directionPointing, zRotation), _directionXZFixed) / (Vector3.Transform(_directionPointing, zRotation).Length() * Vector3.Transform(_directionXZFixed, zRotation).Length())) <= 1.55f &&
                Math.Acos(Vector3.Dot(Vector3.Transform(_directionPointing, zRotation), _directionXZFixed) / (Vector3.Transform(_directionPointing, zRotation).Length() * Vector3.Transform(_directionXZFixed, zRotation).Length())) >= -1.55f)
            {
                _directionPointing = Vector3.Transform(_directionPointing, zRotation);
            }
            
            return _directionPointing;
        }

        
        public Vector3 GetDirectionPointing()
        {
            return _directionPointing;
        }


    }
}
