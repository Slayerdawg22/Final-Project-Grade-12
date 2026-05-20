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
        private float rotationSmoothing = 0.2f; 
        private float returnSmoothing = 0.05f; 

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
                float target = (float)Math.Atan2(v.Y, v.X);
                float diff = MathHelper.WrapAngle(target - angle);
                angle += diff * rotationSmoothing;
            }
            else
            {
                
                float diff = MathHelper.WrapAngle(0f - angle);
                angle += diff * returnSmoothing;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 center = parent.Position + new Vector2(
            parent.Bounds.Width / 2,
            parent.Bounds.Height / 2
            );

            Vector2 origin = new Vector2(sprite.Width / 2, sprite.Height / 2);

            float nucleusScale = parent.Scale * 0.5f;
            spriteBatch.Draw(
            sprite,
            center,
            null,
            Color.White,
            angle,
            origin,
            nucleusScale,
            SpriteEffects.None,
            0f
            );
        }
    }


}
