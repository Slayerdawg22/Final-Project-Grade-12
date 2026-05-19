using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Final_Project
{
    public class FoodManager
    {
        private GraphicsDevice graphics;
        private Random rng = new Random();
        public List<Food> Foods { get; private set; } = new List<Food>();

        // textures for levels 2..4 (level1 uses a drawn square)
        public Texture2D[] LevelTextures = new Texture2D[5];

        // weighting for spawn chances (index 1..4)
        public float[] Weights = new float[] { 0, 0.8f, 0.25f, 0.1f, 0.05f };

        // per-level scale multipliers for textured foods (index 1..4)
        public float[] LevelScales = new float[] { 0, 0.6f, 0.75f, 0.9f, 1f };

        // per-level absolute sizes for untextured (level1) or min size
        public int BaseSize = 8;

        public int TotalCount = 50;

        public FoodManager(GraphicsDevice gd)
        {
            graphics = gd;
        }

        public void LoadTextures(Func<int, string> resolver, Microsoft.Xna.Framework.Content.ContentManager content)
        {
            // resolver should return the asset key for level n (n >=2)
            for (int i = 2; i <= 4; i++)
            {
                string key = resolver(i);
                if (!string.IsNullOrEmpty(key))
                {
                    LevelTextures[i] = content.Load<Texture2D>(key);
                }
                else
                {
                    LevelTextures[i] = null;
                }
            }
        }

        public void GenerateInitial()
        {
            Foods.Clear();
            var vp = graphics.Viewport;

            // normalize weights
            float sum = 0f;
            for (int i = 1; i <= 4; i++) sum += Weights[i];

            int attemptsLimit = 200;
            for (int i = 0; i < TotalCount; i++)
            {
                float pick = (float)(rng.NextDouble() * sum);
                int level = 1;
                float accum = 0f;
                for (int l = 1; l <= 4; l++)
                {
                    accum += Weights[l];
                    if (pick <= accum)
                    {
                        level = l; break;
                    }
                }

                Texture2D tex = null;
                float scale = 1f;
                int size = BaseSize;
                if (level >= 2)
                {
                    tex = LevelTextures[level];
                    scale = LevelScales[level];
                    if (tex != null)
                    {
                        size = (int)(Math.Min(tex.Width, tex.Height) * scale);
                    }
                }
                else
                {
                    // level 1 small square slightly smaller than cell
                    size = Math.Max(4, (int)(BaseSize * 0.6f));
                }

                // pick a non-overlapping position
                Rectangle placedBounds;
                Vector2 pos = Vector2.Zero;
                bool placed = false;
                for (int attempt = 0; attempt < attemptsLimit; attempt++)
                {
                    pos = new Vector2(rng.Next(0, Math.Max(1, vp.Width - size)), rng.Next(0, Math.Max(1, vp.Height - size)));
                    placedBounds = new Rectangle((int)pos.X, (int)pos.Y, tex != null ? (int)(tex.Width * scale) : size, tex != null ? (int)(tex.Height * scale) : size);
                    bool overlap = false;
                    foreach (var f in Foods)
                    {
                        if (placedBounds.Intersects(f.Bounds))
                        {
                            overlap = true; break;
                        }
                    }
                    if (!overlap)
                    {
                        placed = true; break;
                    }
                }

                // even if cannot place perfectly non-overlapping after many attempts, just add it
                Foods.Add(new Food(pos, level, tex, size, scale));
            }
        }

        public void Update(GameTime gameTime)
        {
            // for now food is static, but this is where you'd animate or respawn
        }

        public void Draw(SpriteBatch sb, Texture2D pixel)
        {
            foreach (var f in Foods)
            {
                f.Draw(sb, pixel);
            }
        }
    }
}
