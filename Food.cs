
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
    public class Food
    {
        
        public const float GlobalScale = 0.25f;

        public Vector2 Position;
        public int Level;
        public Texture2D Texture;
        public int Size;
        public float Scale = 1f;

        public Food(Vector2 pos, int level, Texture2D texture, int size, float scale = 1f)
        {
            Position = pos;
            Level = level;
            Texture = texture;
            Size = size;
            Scale = scale;
        }

        public Rectangle Bounds
        {
            get
            {
                if (Texture != null)
                {
                    
                    int w = (int)(Texture.Width * Scale * GlobalScale);
                    int h = (int)(Texture.Height * Scale * GlobalScale);
                    return new Rectangle((int)Position.X, (int)Position.Y, w, h);
                }
                return new Rectangle((int)Position.X, (int)Position.Y, Size, Size);
            }
        }

        public void Draw(SpriteBatch sb, Texture2D pixel)
        {
            if (Texture != null)
            {
                
                sb.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, Scale * GlobalScale, SpriteEffects.None, 0f);
            }
            else
            {
                sb.Draw(pixel, Position, null, Color.LimeGreen, 0f, Vector2.Zero, new Vector2(Size, Size), SpriteEffects.None, 0f);
            }
        }
    }
}
