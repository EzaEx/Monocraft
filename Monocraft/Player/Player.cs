using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.MediaFoundation;

namespace Monocraft
{
    public class Player : IUpdateable, IDrawable
    {
        private readonly Camera _camera;   
        private PhysicsBase _physicsBase; 
        //coords of block looking at
        private Vector3 _blockLookingAt;
        private Vector3 _blockHover; 
        //water properties
        private Rect _liquidHaze;
        private float _buoyancy;
        private Func<Vector3, bool> _submersionCheck;

        public Camera camera => _camera;
        public PhysicsBase physicsBase { get => _physicsBase; set => _physicsBase = value; }
        public Vector3 blockLookingAt { get => _blockLookingAt; set => _blockLookingAt = value; }
        public Vector3 blockHover { get => _blockHover; set => _blockHover = value; }
        public Func<Vector3, bool> SubmersionCheck { get => _submersionCheck; set => _submersionCheck = value; }

        public Player(GraphicsDeviceManager graphics, PhysicsBase physicsBase)
        { 
            //init objects
            _physicsBase = physicsBase;
            _camera = new Camera(graphics, physicsBase.position + new Vector3(0, 1.5f, 0), Vector3.UnitX);
            _liquidHaze = new Rect(new Vector2(0, 0), new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.DeepSkyBlue, graphics.GraphicsDevice, new SpriteBatch(graphics.GraphicsDevice));
            
            //transparency of waterhaze to 100%
            _liquidHaze.Alpha = 0f; 
            //buoyancy (gravity modifier) to normal (onland)
            _buoyancy = 1f;
        }

        public void Update(GameTime gameTime) 
        {
            camera.Update(physicsBase.position + new Vector3(0, 1.5f, 0)); 
            //update block looking at 
            _blockLookingAt = GetBlockLookingAt(); 
            //update physics base (fall, collision, movement)
            physicsBase.Update(camera.GetDirectionPointing(), GetInputDirection(), _buoyancy, gameTime);
            //assume not in water
            _liquidHaze.Alpha = 0;
            _buoyancy = 1f; 
            //if in water, set water movement and graphics values
            if (_submersionCheck(Utility.Floor(camera.Position)))
            {
                _liquidHaze.Alpha = 0.7f;
                _buoyancy = 0.3f;
            }
        }       

        //simple raytrace to get target block in line of sight
        private Vector3 GetBlockLookingAt()
        {
            Vector3 tracePoint = camera.Position; 
            //get direction facing vector
            Vector3 traceDirection = camera.GetDirectionPointing();  
            //movement step through space
            float traceStep = 0.005f; 
            //do max 700 moves (blocks further than 0.005*700 blocks away are unreachable)
            for (int i = 0; i < 700; i++)
            { 
                //if trace is in a block, this is the block looking at
                if (physicsBase.Collider1(Utility.Floor(tracePoint)))
                { 
                    blockHover = Utility.Floor(tracePoint - traceDirection * traceStep);
                    return Utility.Floor(tracePoint);
                }
                else
                { 
                    //move 1 step in direction
                    tracePoint += traceDirection * traceStep;
                }
            }
            return new Vector3(GetCurrentChunk(16).X * 16, 0, GetCurrentChunk(16).Y * 16);
        }
         
        //add movement direction vectors based on keys pressed
        private Vector3 GetInputDirection()
        {
            KeyboardState state = Keyboard.GetState();
            Vector3 movement = Vector3.Zero;
            if (state.GetPressedKeyCount() > 0)
            {
                if (state.IsKeyDown(Keys.W))
                {
                    movement += new Vector3(1, 0, 0);
                }
                if (state.IsKeyDown(Keys.A))
                {
                    movement += new Vector3(0, 0, -1);
                }
                if (state.IsKeyDown(Keys.S))
                {
                    movement += new Vector3(-1, 0, 0);
                }
                if (state.IsKeyDown(Keys.D))
                {
                    movement += new Vector3(0, 0, 1);
                }
                if (state.IsKeyDown(Keys.Space))
                {
                    movement += new Vector3(0, 1, 0);
                }
                if (state.IsKeyDown(Keys.LeftShift))
                {
                    movement += new Vector3(0, -1, 0);
                }
                if (movement != Vector3.Zero)
                {
                    movement.Normalize();                  
                }
                if (state.IsKeyDown(Keys.Back))
                {
                    physicsBase.position = new Vector3(new Random().Next(-10000, 10000), 220, new Random().Next(-10000, 10000));
                }
            }
            return movement;
        }  

        //get chunkIndex the player is in
        public Vector2 GetCurrentChunk(int chunkSize)
        {
            Vector2 pos = new Vector2((int)Math.Floor((int)physicsBase.position.X / (double)chunkSize), (int)Math.Floor((int)physicsBase.position.Z / (double)chunkSize));
            return pos;
        } 

        //draw the liquid haze
        public void Draw()
        {
            _liquidHaze.Draw();
        }

    }
}
