using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Final_Project
{
    public class RockManager
    {
        private readonly GraphicsDevice graphics;
        private readonly Random rng;

        public float TextureScale { get; set; } = 0.5f;

        public List<Texture2D> MediumTextures { get; } = new List<Texture2D>();
        public List<Texture2D> VerticalTextures { get; } = new List<Texture2D>();

        public List<Rock> Rocks { get; } = new List<Rock>();
        public int DrawCount { get; set; } = 4;
        public float MediumRatio { get; set; } = 0.6f;

        public RockManager(GraphicsDevice gd, int? seed = null)
        {
            graphics = gd ?? throw new ArgumentNullException(nameof(gd));
            rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public void LoadTextures(string[] mediumKeys, string[] verticalKeys, ContentManager content)
        {
            MediumTextures.Clear();
            VerticalTextures.Clear();

            if (mediumKeys != null)
            {
                foreach (var k in mediumKeys)
                {
                    if (!string.IsNullOrEmpty(k))
                    {
                        MediumTextures.Add(content.Load<Texture2D>(k));
                    }
                }
            }

            if (verticalKeys != null)
            {
                foreach (var k in verticalKeys)
                {
                    if (!string.IsNullOrEmpty(k))
                    {
                        VerticalTextures.Add(content.Load<Texture2D>(k));
                    }
                }
            }
        }

        public void GenerateRandom()
        {
            var vp = graphics.Viewport;
            var generated = GenerateRocksIn(new Rectangle(0, 0, vp.Width, vp.Height));
            Rocks.Clear();
            Rocks.AddRange(generated);
        }

        // Generate rocks inside an arbitrary area (world coordinates). Returns a list and does not modify internal Rocks.
        public List<Rock> GenerateRocksIn(Rectangle area)
        {
            var result = new List<Rock>();
            int attemptsLimit = 200;
            int padding = 4; // small spacing so textures don't touch

            for (int i = 0; i < DrawCount; i++)
            {
                bool pickMedium = rng.NextDouble() <= MediumRatio;

                List<Texture2D> source = pickMedium ? MediumTextures : VerticalTextures;
                if (source.Count == 0)
                {
                    source = pickMedium ? VerticalTextures : MediumTextures;
                }

                if (source.Count == 0)
                {
                    break;
                }

                var tex = source[rng.Next(source.Count)];

                // scaled size used for placement / bounds
                int scaledW = Math.Max(1, (int)(tex.Width * TextureScale));
                int scaledH = Math.Max(1, (int)(tex.Height * TextureScale));

                Rectangle placedBounds = Rectangle.Empty;
                Vector2 pos = Vector2.Zero;
                bool placed = false;

                for (int attempt = 0; attempt < attemptsLimit; attempt++)
                {
                    int maxX = Math.Max(1, area.Width - scaledW);
                    int maxY = Math.Max(1, area.Height - scaledH);
                    pos = new Vector2(area.X + rng.Next(0, maxX), area.Y + rng.Next(0, maxY));
                    placedBounds = new Rectangle((int)pos.X - padding, (int)pos.Y - padding, scaledW + padding * 2, scaledH + padding * 2);

                    bool overlap = false;
                    foreach (var r in result)
                    {
                        if (placedBounds.Intersects(r.Bounds))
                        {
                            overlap = true;
                            break;
                        }
                    }

                    if (!overlap)
                    {
                        placed = true;
                        break;
                    }
                }

                if (placed)
                {
                    result.Add(new Rock(pos, tex, pickMedium, TextureScale));
                }
            }

            return result;
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (var r in Rocks)
            {
                r.Draw(sb);
            }
        }

        public class Rock
        {
            public Vector2 Position;
            public Texture2D Texture;
            public bool IsMedium;
            public float Scale;

            public Rock(Vector2 pos, Texture2D tex, bool isMedium, float scale)
            {
                Position = pos;
                Texture = tex;
                IsMedium = isMedium;
                Scale = scale;
            }

            
            public Rectangle Bounds => new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                Texture != null ? (int)(Texture.Width * Scale) : 0,
                Texture != null ? (int)(Texture.Height * Scale) : 0
            );

            public void Draw(SpriteBatch sb)
            {
                if (Texture != null)
                {
                    sb.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                }
            }
        }
    }
}