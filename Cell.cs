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
    public class Cell
    {
        private Texture2D[] sprites;
        private int spriteIndex = 0;
        private float animationTimer = 0f;
        private float animationSpeed = 0.15f;

        public Vector2 Position;
        public Vector2 Velocity;
        public float Speed = 3f;

        public Rectangle Bounds =>
        new Rectangle((int)Position.X, (int)Position.Y, sprites[spriteIndex].Width, sprites[spriteIndex].Height);

        private Nucleus nucleus;

        public Cell(Vector2 startPos, Texture2D[] cellSprites, Texture2D nucleusSprite)
        {
            Position = startPos;
            sprites = cellSprites;
            nucleus = new Nucleus(this, nucleusSprite);
        }

        public void HandleInput()
        {
            KeyboardState k = Keyboard.GetState();
            Velocity = Vector2.Zero;

            if (k.IsKeyDown(Keys.Up)) Velocity.Y = -Speed;
            if (k.IsKeyDown(Keys.Down)) Velocity.Y = Speed;
            if (k.IsKeyDown(Keys.Left)) Velocity.X = -Speed;
            if (k.IsKeyDown(Keys.Right)) Velocity.X = Speed;
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                spriteIndex = (spriteIndex + 1) % sprites.Length;
            }
        }

        public void Update(GameTime gameTime)
        {
            HandleInput();
            UpdateAnimation(gameTime);

            Position += Velocity;

            nucleus.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprites[spriteIndex], Position, Color.White);
            nucleus.Draw(spriteBatch);
        }
    }
}
