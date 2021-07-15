using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Monocraft
{ 
    //This is a button (and a test)
    class Button : Rect , IUpdateable
    { 
        //properties
        private TextBox _text;
        private bool _hovering;
        private ButtonState _clicked;
        private Action _action;

        //constructor
        public Button(String text, SpriteFont font, Vector2 position, Vector2 dimensions, GraphicsDevice graphics, SpriteBatch spriteBatch, Action action) : base(position, dimensions, Color.White, graphics, spriteBatch)
        { 
            _text = new TextBox(text, position + new Vector2(10, 10), 1, font, graphics, spriteBatch);
            _hovering = false;
            _action = action;
        }

        public override void Draw()
        { 
            base.Draw();
            _text.Draw();
        }  


        public void Update(GameTime gameTime = null)
        { 
            //if mouse is over button
            if (Mouse.GetState().X >= _position.X && 
                Mouse.GetState().X <= _position.X + _dimensions.X &&
                Mouse.GetState().Y >= _position.Y &&
                Mouse.GetState().Y <= _position.Y + _dimensions.Y)
            { 
                //light the button up
                _hovering = true;
                _color = Color.Black;
                _text.Color = Color.White;
            }
            else
            { 
                //keep button depressed
                _hovering = false;
                _color = Color.White;
                _text.Color = Color.Black;
            }

            //if clicked run action
            if (_hovering && Mouse.GetState().LeftButton == ButtonState.Released && _clicked == ButtonState.Pressed)
            {
                _action.Invoke();
            }

            _clicked = Mouse.GetState().LeftButton;
        }
    }
}
