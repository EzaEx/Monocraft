using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Monocraft
{
    class Rect : IDrawable
    { 
        //dimensions
        protected Vector2 _position;
        protected Vector2 _dimensions; 
        //graphics managers
        private GraphicsDevice _graphics; 
        private SpriteBatch _spriteBatch; 
        //aesthetics
        private Texture2D _texture;
        private float _alpha;
        protected Color _color;

        public float Alpha { get => _alpha; set => _alpha = value; }

        public Rect(Vector2 position, Vector2 dimensions, Color color, GraphicsDevice graphics, SpriteBatch spriteBatch, Texture2D texture = null)
        { 
            //init all properties
            _position = position;
            _graphics = graphics; 
            _spriteBatch = spriteBatch;
            _color = color;
            _dimensions = dimensions; 
            //if texture isnt provided, use plain colour
            if (texture == null)
            {
                _texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _texture.SetData(new Color[] { Color.White });
            }
            else
            {
                _texture = texture;
            }
            _alpha = 1f;
        }
        //draw rect with spritebatch
        virtual public void Draw()
        { 
            //draw rect at position with dimensions
            _spriteBatch.Begin();
            _spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, (int)_dimensions.X, (int)_dimensions.Y), _color * _alpha);
            _spriteBatch.End();
        }
    }
}
