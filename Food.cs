using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Final_Project
{
    public class Food
    {
        public Vector2 Position;
        public int Level; // 1..4
        public Texture2D Texture; // null for level 1 (we'll draw a square)
        public int Size; // pixel size used for untextured food
        public float Scale = 1f; // scale applied to Texture when drawing

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
                return new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale), (int)(Texture.Height * Scale));
            }
            return new Rectangle((int)Position.X, (int)Position.Y, Size, Size);
            }
        }

        public void Draw(SpriteBatch sb, Texture2D pixel)
        {
            if (Texture != null)
            {
                sb.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            }
            else
            {
                sb.Draw(pixel, Position, null, Color.LimeGreen, 0f, Vector2.Zero, new Vector2(Size, Size), SpriteEffects.None, 0f);
            }
        }
    }
}
