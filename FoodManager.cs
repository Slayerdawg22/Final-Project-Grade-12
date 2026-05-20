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

        // now accepts an optional list of obstacle rocks to avoid when placing food
        public void GenerateInitial(IEnumerable<RockManager.Rock>? obstacles = null)
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

                Texture2D? tex = null;
                float scale = 1f;
                int size = BaseSize;
                if (level >= 2)
                {
                    tex = LevelTextures[level];
                    scale = LevelScales[level];
                    if (tex != null)
                    {
                        // keep original size value (used for untextured fallback), drawing uses Scale
                        size = (int)(Math.Min(tex.Width, tex.Height) * scale);
                    }
                }
                else
                {
                    // level 1 small square slightly smaller than cell
                    size = Math.Max(4, (int)(BaseSize * 0.6f));
                }

                // compute final placed width/height using Food.GlobalScale so placement matches drawing
                int placeW = tex != null ? Math.Max(1, (int)(tex.Width * scale * Food.GlobalScale)) : Math.Max(1, (int)(size * Food.GlobalScale));
                int placeH = tex != null ? Math.Max(1, (int)(tex.Height * scale * Food.GlobalScale)) : Math.Max(1, (int)(size * Food.GlobalScale));

                // pick a non-overlapping position
                Rectangle placedBounds;
                Vector2 pos = Vector2.Zero;
                bool placed = false;
                for (int attempt = 0; attempt < attemptsLimit; attempt++)
                {
                    pos = new Vector2(rng.Next(0, Math.Max(1, vp.Width - placeW)), rng.Next(0, Math.Max(1, vp.Height - placeH)));
                    placedBounds = new Rectangle((int)pos.X, (int)pos.Y, placeW, placeH);
                    bool overlap = false;

                    // check against already placed food
                    foreach (var f in Foods)
                    {
                        if (placedBounds.Intersects(f.Bounds))
                        {
                            overlap = true; break;
                        }
                    }

                    // check against obstacle rocks if provided
                    if (!overlap && obstacles != null)
                    {
                        foreach (var obs in obstacles)
                        {
                            if (placedBounds.Intersects(obs.Bounds))
                            {
                                overlap = true; break;
                            }
                        }
                    }

                    if (!overlap)
                    {
                        placed = true; break;
                    }
                }

                // Only add the food if we successfully found a non-overlapping place
                if (placed)
                {
                    Foods.Add(new Food(pos, level, tex, size, scale));
                }
                // if not placed, skip this food (keeps counts possibly lower)
            }
        }

        public void Update(GameTime gameTime)
        {
            // for now food is static
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
