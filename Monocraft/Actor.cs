using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Monocraft
{
    class Actor : IDrawable, IUpdateable
    { 
        //world matrix
        private Matrix _world; 
        //camera
        private Camera _camera; 
        //graphics
        private Model _model;
        private Texture2D _texture;
        private GraphicsDeviceManager _graphics;
        private PhysicsBase _physicsBase; 
        //time till next rotation
        private int _movetime = 0; 
        //rotation about y
        private Matrix _rotation; 
        //heading angle
        private float _angle;
        private Random _rgen; 
        //size of model scale
        private float _sizeMultiplyer;

        public Actor(PhysicsBase physicsBase, Camera camera, Model model, Texture2D texture, GraphicsDeviceManager graphics)
        {
            _physicsBase = physicsBase;
            _camera = camera;
            _model = model;
            _texture = texture;
            _graphics = graphics;
            _rgen = new Random(); 
            //gen no 0.25 -> approx 0.9
            _sizeMultiplyer = ((float)_rgen.NextDouble() / 1.2f) + 0.2f;
        }

        public PhysicsBase PhysicsBase { get => _physicsBase; set => _physicsBase = value; }

        public void Draw()
        {
            //draw
            DrawModel(_model);
        }

        public void Update(GameTime gt)
        { 
            //time to change direction
            if (_movetime == 0)
            { 
                //set random heading direction
                _angle = MathF.PI * 2 * ((float)new Random().NextDouble() - 0.5f); 
                //create roation matrix
                _rotation = Matrix.CreateRotationY(_angle); 
                //set random time till next move
                _movetime = _rgen.Next(150, 600);
            }
            _movetime--; 
            //move forward + jump
            Vector3 move = new Vector3(1, 1, 0);
            move.Normalize();  
            //update physics
            _physicsBase.Update(Vector3.Transform(new Vector3(1, 0, 0), _rotation), move, 1, gt); 
            //make new world matrix with rotation + movement
            _world = Matrix.CreateScale(0.2f * _sizeMultiplyer) * Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), _angle + (MathF.PI / 2));
            _world.Translation = _physicsBase.position + new Vector3(0, 1.5f, -1.75f); 
        }

        //get chunk index of actor
        public Vector2 GetCurrentChunk(int chunkSize)
        {  
            //divide coords by chunksize
            Vector2 pos = new Vector2((int)Math.Floor((int)_physicsBase.position.X / (double)chunkSize), (int)Math.Floor((int)_physicsBase.position.Z / (double)chunkSize));
            return pos;
        }

        private void DrawModel(Model model)
        { 
            //loop through meshes & effects
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //effect.CurrentTechnique.Passes[0].Apply(); 

                    //set up lighting
                    effect.LightingEnabled = true; // turn on the lighting subsystem. 
                    effect.DirectionalLight0.DiffuseColor = new Vector3(0.6f, 0.6f, 0.6f);
                    effect.DirectionalLight0.Direction = new Vector3(1, -0.9f, 0.3f);
                    effect.DirectionalLight0.SpecularColor = new Vector3(0.3f, 0.3f, 0.3f); 
                    effect.AmbientLightColor = new Vector3(1f, 1f, 1f);

                    //enable + load texture
                    effect.TextureEnabled = true;
                    effect.Texture = _texture; 
                    
                    //set Matrices
                    effect.World = _world;
                    effect.View = _camera.GetViewMatrix();
                    effect.Projection = _camera.GetProjectionMatrix();

                    //Set up texture resolution settings 

                    _graphics.GraphicsDevice.BlendState = BlendState.Opaque;
                    _graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    _graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                }

                mesh.Draw();
            }
        }
    }
}
