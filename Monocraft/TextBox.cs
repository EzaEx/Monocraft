using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Monocraft
{
    class TextBox : IDrawable
    { 
        //aestetics
        protected Vector2 _position;
        protected String _text;
        protected int _scale;
        protected SpriteFont _font; 
        //graphics devices
        protected GraphicsDevice _graphics; 
        protected SpriteBatch _spriteBatch;
        protected SpriteEffects _spriteEffects;
        protected Color _color;  
        //time to live (optional)
        private int _ttl;

        public Color Color { get => _color; set => _color = value; }
        protected string Text { get => _text; set => _text = value; }

        public TextBox(String text, Vector2 position, int scale, SpriteFont font, GraphicsDevice graphics, SpriteBatch spriteBatch, int ttl = -1)
        { 
            //set properties
            _position = position;
            _text = text;
            _font = font;
            _graphics = graphics;
            _spriteBatch = spriteBatch;
            _scale = scale;
            _ttl = ttl;
            
            _color = Color.White;
            _spriteEffects = new SpriteEffects();
        }


        public void Draw()
        { 
            //draw text using spritebatch
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, _text, _position, _color, 0, new Vector2(0,0), _scale, _spriteEffects, 0);
            _spriteBatch.End(); 
            //if time-to-live is enabled, decrease by 1
            if (_ttl > 0)
            {
                _ttl--;
            } 
            //if time-to-live hits 0, remove text
            if (_ttl == 0)
            {
                _text = "";
            }
        }
    }
}
