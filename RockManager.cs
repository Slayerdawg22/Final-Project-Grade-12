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

        // textures grouped by type
        public List<Texture2D> MediumTextures { get; } = new List<Texture2D>();
        public List<Texture2D> VerticalTextures { get; } = new List<Texture2D>();

        // generated rocks to draw
        public List<Rock> Rocks { get; } = new List<Rock>();

        // how many rocks to draw
        public int DrawCount { get; set; } = 4;

        // probability to pick a medium rock (0..1). vertical probability = 1 - MediumRatio
        public float MediumRatio { get; set; } = 0.6f;

        public RockManager(GraphicsDevice gd, int? seed = null)
        {
            graphics = gd ?? throw new ArgumentNullException(nameof(gd));
            rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        // Load texture keys into the manager. Null/empty keys are ignored.
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

        // Generate random rocks according to DrawCount and MediumRatio.
        // Positions are random within the viewport (texture-sized clamped).
        public void GenerateRandom()
        {
            Rocks.Clear();
            var vp = graphics.Viewport;

            for (int i = 0; i < DrawCount; i++)
            {
                bool pickMedium = rng.NextDouble() <= MediumRatio;

                // if chosen list is empty, fallback to the other
                List<Texture2D> source = pickMedium ? MediumTextures : VerticalTextures;
                if (source.Count == 0)
                {
                    source = pickMedium ? VerticalTextures : MediumTextures;
                }

                if (source.Count == 0)
                {
                    // nothing to pick
                    break;
                }

                var tex = source[rng.Next(source.Count)];

                // place within viewport so entire texture is visible
                int maxX = Math.Max(1, vp.Width - tex.Width);
                int maxY = Math.Max(1, vp.Height - tex.Height);
                var pos = new Vector2(rng.Next(0, maxX), rng.Next(0, maxY));

                Rocks.Add(new Rock(pos, tex, pickMedium == true));
            }
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

            public Rock(Vector2 pos, Texture2D tex, bool isMedium)
            {
                Position = pos;
                Texture = tex;
                IsMedium = isMedium;
            }

            public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Texture?.Width ?? 0, Texture?.Height ?? 0);

            public void Draw(SpriteBatch sb)
            {
                if (Texture != null)
                {
                    sb.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }
    }
}