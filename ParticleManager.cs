using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Final_Project
{
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Life;
        public float MaxLife;
        public Color Color;
        public float Size;
    }

    public class ParticleManager
    {
        private readonly List<Particle> particles = new List<Particle>();
        private readonly Random rng = new Random();

        public void Update(GameTime gt)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Position += p.Velocity * dt;
                p.Life += dt;
                p.Velocity *= 0.98f;
                if (p.Life >= p.MaxLife) particles.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch sb, Texture2D pixel)
        {
            foreach (var p in particles)
            {
                float t = 1f - (p.Life / p.MaxLife);
                var col = p.Color * t;
                sb.Draw(pixel, p.Position, null, col, 0f, Vector2.One * 0.5f, p.Size, SpriteEffects.None, 0f);
            }
        }

        
        public void SpawnAt(Vector2 pos, int count, Color color, float size = 4f, float lifeMin = -1f, float lifeMax = -1f)
        {
            for (int i = 0; i < count; i++)
            {
                var angle = (float)(rng.NextDouble() * Math.PI * 2);
                var speed = (float)(rng.NextDouble() * 80 + 40);
                float maxLife;
                if (lifeMin >= 0f && lifeMax >= lifeMin)
                {
                    maxLife = (float)(rng.NextDouble() * (lifeMax - lifeMin) + lifeMin);
                }
                else
                {
                    maxLife = (float)(rng.NextDouble() * 0.6 + 0.4);
                }

                particles.Add(new Particle
                {
                    Position = pos,
                    Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed,
                    Life = 0f,
                    MaxLife = maxLife,
                    Color = color,
                    Size = size * (float)(rng.NextDouble() * 0.8 + 0.6f)
                });
            }
        }
    }
}
