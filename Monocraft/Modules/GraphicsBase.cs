using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Monocraft
{
    [Serializable]
    public class GraphicsBase
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly Camera _camera;
        private VertexBuffer _vertexBuffer;
        private Texture2D _texture;
        private BasicEffect _effect;

        public GraphicsDeviceManager Graphics => _graphics;
        public VertexBuffer VertexBuffer { get => _vertexBuffer; set => _vertexBuffer = value; }
        public Texture2D Texture { get => _texture; set => _texture = value; }
        public BasicEffect Effect { get => _effect; set => _effect = value; }

        public GraphicsBase(GraphicsDeviceManager graphics, Camera camera)
        {
            _camera = camera;
            _graphics = graphics;
            _effect = new BasicEffect(_graphics.GraphicsDevice);
        }

        public GraphicsBase() { }
        //creates copy of graphics object with new properties (so not ref)
        public GraphicsBase Clone()
        {
            GraphicsBase clone = new GraphicsBase(_graphics, _camera);
            clone.VertexBuffer = _vertexBuffer;
            clone.Texture = _texture;
            clone.Effect = _effect;
            return clone;
        }

        public void LoadContent(Texture2D texture)
        {
            _texture = texture;
        }

        public void LoadVertices(VertexPositionNormalTexture[] vertices)
        { //take array of verticies and convert to vertex buffer
            VertexBuffer = new VertexBuffer(Graphics.GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData<VertexPositionNormalTexture>(vertices);
        }

        public void Destroy()
        { 
            //properly destroy graphics item
            if (_vertexBuffer != null)
            {
                _vertexBuffer.Dispose();
            }
        }

        public void RenderMesh()
        {
            //get matricies
            Matrix view = _camera.GetViewMatrix();
            Matrix projection = _camera.GetProjectionMatrix();

            _effect.World = Matrix.Identity;
            _effect.View = view;
            _effect.Projection = projection;

            //set up lighting
            _effect.LightingEnabled = true; // turn on the lighting subsystem.
            _effect.DirectionalLight0.DiffuseColor = new Vector3(0.6f, 0.6f, 0.6f);
            _effect.DirectionalLight0.Direction = new Vector3(1, -0.5f, 0.3f);
            //_effect.DirectionalLight0.SpecularColor = new Vector3(0.3f, 0.3f, 0.3f); 
            _effect.AmbientLightColor = new Vector3(0.6f, 0.6f, 0.6f); 

            //load texture
            _effect.TextureEnabled = true;
            _effect.Texture = _texture;

            _graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            _graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //load in vertex buffer
            _graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            _graphics.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            _effect.CurrentTechnique.Passes[0].Apply();

                _graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexBuffer.VertexCount / 3);
        }
    }
}
