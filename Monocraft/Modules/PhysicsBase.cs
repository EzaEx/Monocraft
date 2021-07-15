using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Runtime;

namespace Monocraft
{
    public class PhysicsBase
    {
        private Vector3 _position;
        private Vector3 _velocity;
        private float _moveSpeed;
        private float _height;
        private Func<Vector3, bool> Collider;
        private bool _grounded = false;
        private float _gravityForce;
        private bool _gravToggle = true;
        private bool _gravSwitch = false;
        private bool _canFly;
        private readonly Vector3 _upVector = Vector3.UnitY;

        public PhysicsBase(Vector3 position, float moveSpeed, float height, bool canFly = false)
        { 
            //set attributes
            _position = position;
            _moveSpeed = moveSpeed;
            _height = height;
            _canFly = canFly;
            Collider = Utility.PlaceholderCollider;
        }


        public Vector3 position { get => _position; set => _position = value; }

        public Vector3 upVector => _upVector;

        public Vector3 velocity { get => _velocity; set => _velocity = value; }
        public Func<Vector3, bool> Collider1 { get => Collider; set => Collider = value; }

        public void LoadCollider(Func<Vector3, bool> collider)
        {
            Collider = collider;
        } 

        public void Update(Vector3 directionPointing, Vector3 inputDirection, float buoyancy, GameTime gameTime)
        {
            Move(directionPointing, inputDirection, buoyancy, gameTime);
        }

        private void Move(Vector3 directionPointing, Vector3 inputDirection, float buoyancy, GameTime gameTime)
        {
            //get time ellapsed since frame
            float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 16.667f;

            Vector3 headingDir = directionPointing;
            headingDir.Y = 0;
            headingDir.Normalize();

            Vector3 crossDir = Vector3.Cross(directionPointing, upVector);
            crossDir.Y = 0;
            crossDir.Normalize();

            //enable / disable flight 
            if (_canFly)
            {
                if (!Keyboard.GetState().IsKeyDown(Keys.X) && _gravSwitch)
                {
                    _gravToggle = !_gravToggle;
                    _gravityForce = 0;
                }
            }
            _gravSwitch = Keyboard.GetState().IsKeyDown(Keys.X);
            //gravity calc
            if (_gravToggle)
            {
                _moveSpeed = 0.1f; 
                //check colliding with floor
                if (!Collider(Utility.Floor(position - new Vector3(0, 0.2f, 0))))
                {
                    _grounded = false;
                }

                if (!_grounded)
                { 
                    //increase gravity force
                    _gravityForce -= buoyancy * 0.01f * delta;
                }
                else
                {
                    _gravityForce = 0;
                }

                if (inputDirection.Y > 0 && _grounded)
                {
                    _gravityForce = 0.2f;
                }

                velocity = _moveSpeed * (headingDir * inputDirection.X + crossDir * inputDirection.Z) * delta + Vector3.UnitY * _gravityForce * delta;
            }
            else
            {
                _moveSpeed = 0.5f;
                velocity = _moveSpeed * (headingDir * inputDirection.X + crossDir * inputDirection.Z + Vector3.UnitY * inputDirection.Y) * delta;
            }

            //check x collision
            if (!Collider(Utility.Floor(position + new Vector3(velocity.X, 0, 0))) && !Collider(Utility.Floor(position + new Vector3(velocity.X + 0.1f * Math.Sign(velocity.X), 0, 0))) &&
                !Collider(Utility.Floor(position + new Vector3(velocity.X, _height, 0))) && !Collider(Utility.Floor(position + new Vector3(velocity.X + 0.1f * Math.Sign(velocity.X), _height, 0))))
            {
                position += new Vector3(velocity.X, 0, 0);
            }
            else
            {
                position = new Vector3((float)Math.Round(position.X) + 0.1f * Math.Sign(position.X - Math.Round(position.X)), position.Y, position.Z);
            }

            //check z collision
            if (!Collider(Utility.Floor(position + new Vector3(0, 0, velocity.Z))) && !Collider(Utility.Floor(position + new Vector3(0, 0, velocity.Z + 0.1f * Math.Sign(velocity.Z)))) &&
                !Collider(Utility.Floor(position + new Vector3(0, _height, velocity.Z))) && !Collider(Utility.Floor(position + new Vector3(0, _height, velocity.Z + 0.1f * Math.Sign(velocity.Z)))))
            {
                position += new Vector3(0, 0, velocity.Z);
            }
            else
            {
                position = new Vector3(position.X, position.Y, (float)Math.Round(position.Z) + 0.1f * Math.Sign(position.Z - Math.Round(position.Z)));
            }
            //check y collision
            if (velocity.Y != 0)
            {
                if (!Collider(Utility.Floor(position + new Vector3(0, velocity.Y, 0))) && !Collider(Utility.Floor(position + new Vector3(0, velocity.Y + 0.1f * Math.Sign(velocity.Y), 0))) &&
                    !Collider(Utility.Floor(position + new Vector3(0, velocity.Y + _height, 0))) && !Collider(Utility.Floor(position + new Vector3(0, velocity.Y + 0.1f * Math.Sign(velocity.Y) + _height, 0))))
                {
                    position += new Vector3(0, velocity.Y, 0); 
                    _grounded = false;
                }
                else
                {
                    if (Math.Sign(velocity.Y) == -1)
                    {
                        position = new Vector3(position.X, (float)Math.Round(position.Y) + 0.05f * Math.Sign(position.Y - Math.Round(position.Y)), position.Z);
                        _grounded = true;
                    } 
                        _gravityForce = 0;
                }
            }
        }
    }
}
