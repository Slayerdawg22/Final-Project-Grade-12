using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace Final_Project
{

    public class Nucleus
    {
        private Cell parent;
        private Texture2D sprite;
        private float angle = 0f;

        public Nucleus(Cell parentCell, Texture2D nucleusSprite)
        {
            parent = parentCell;
            sprite = nucleusSprite;
        }

        public void Update(GameTime gameTime)
        {
            Vector2 v = parent.Velocity;

            if (v.LengthSquared() > 0.1f)
            {
                angle = (float)Math.Atan2(v.Y, v.X);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 center = parent.Position + new Vector2(
            parent.Bounds.Width / 2,
            parent.Bounds.Height / 2
            );

            Vector2 origin = new Vector2(sprite.Width / 2, sprite.Height / 2);

            spriteBatch.Draw(
            sprite,
            center,
            null,
            Color.White,
            angle,
            origin,
            1f,
            SpriteEffects.None,
            0f
            );
        }
    }


}
