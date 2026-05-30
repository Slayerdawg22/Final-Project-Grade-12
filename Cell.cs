using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Final_Project
{
    public class Cell : INucleated
    {
        private Texture2D[] sprites;
        private int spriteIndex = 0;
        private float animationTimer = 0f;
        private float animationSpeed = 0.15f;

        // Implemented as properties to satisfy INucleated
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Speed = 3f;
        public float Scale { get; set; } = 0.25f;

        public Rectangle Bounds =>
            new Rectangle((int)Position.X, (int)Position.Y, (int)(sprites[spriteIndex].Width * Scale), (int)(sprites[spriteIndex].Height * Scale));

        private Nucleus nucleus;

        public Cell(Vector2 startPos, Texture2D[] cellSprites, Texture2D nucleusSprite)
        {
            Position = startPos;
            sprites = cellSprites;
            nucleus = new Nucleus(this, nucleusSprite, 0.5f);
        }

        public void HandleInput()
        {
            KeyboardState k = Keyboard.GetState();
            // Use a local Vector2 to modify components, then assign back to the Velocity property.
            Vector2 v = Vector2.Zero;
            if (k.IsKeyDown(Keys.Up)) v.Y = -Speed;
            if (k.IsKeyDown(Keys.Down)) v.Y = Speed;
            if (k.IsKeyDown(Keys.Left)) v.X = -Speed;
            if (k.IsKeyDown(Keys.Right)) v.X = Speed;
            Velocity = v;
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

        public void ResolveCollisions(IEnumerable<RockManager.Rock> obstacles)
        {
            var tex = sprites[spriteIndex];
            float w = tex.Width * Scale;
            float h = tex.Height * Scale;
            Vector2 center = Position + new Vector2(w / 2f, h / 2f);
            float radius = Math.Max(w, h) * 0.5f;

            foreach (var obs in obstacles)
            {
                var rect = obs.Bounds;

                float closestX = MathHelper.Clamp(center.X, rect.Left, rect.Right);
                float closestY = MathHelper.Clamp(center.Y, rect.Top, rect.Bottom);
                Vector2 closest = new Vector2(closestX, closestY);

                Vector2 diff = center - closest;
                float distSq = diff.LengthSquared();

                if (distSq < radius * radius)
                {
                    if (distSq > 0.0001f)
                    {
                        float dist = (float)Math.Sqrt(distSq);
                        float overlap = radius - dist;
                        Vector2 push = diff / dist * overlap;
                        Position += push;
                        center += push;
                        if (Vector2.Dot(Velocity, push) > 0)
                        {
                            Velocity = Vector2.Zero;
                        }
                    }
                    else
                    {
                        float leftOverlap = center.X - rect.Left;
                        float rightOverlap = rect.Right - center.X;
                        float topOverlap = center.Y - rect.Top;
                        float bottomOverlap = rect.Bottom - center.Y;

                        float min = Math.Min(Math.Min(leftOverlap, rightOverlap), Math.Min(topOverlap, bottomOverlap));
                        Vector2 push = Vector2.Zero;
                        if (min == leftOverlap) push = new Vector2(radius - leftOverlap, 0);
                        else if (min == rightOverlap) push = new Vector2(-(radius - rightOverlap), 0);
                        else if (min == topOverlap) push = new Vector2(0, radius - topOverlap);
                        else push = new Vector2(0, -(radius - bottomOverlap));

                        Position += push;
                        center += push;
                        Velocity = Vector2.Zero;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprites[spriteIndex], Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            nucleus.Draw(spriteBatch);
        }
    }
}
