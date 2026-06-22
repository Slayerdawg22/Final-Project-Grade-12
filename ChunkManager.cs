using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Final_Project
{
    public class ChunkManager
    {
        private readonly RockManager rockManager;
        private readonly FoodManager foodManager;
        private readonly int chunkSize;
        private readonly int loadRadius; 
        private readonly Dictionary<Point, Chunk> loaded = new Dictionary<Point, Chunk>();

        public ChunkManager(RockManager rocks, FoodManager foods, int chunkSize = 1024, int loadRadius = 1)
        {
            rockManager = rocks;
            foodManager = foods;
            this.chunkSize = chunkSize;
            this.loadRadius = loadRadius;
        }

        public void Update(Vector2 playerWorldPos)
        {
            Point playerChunk = WorldToChunk(playerWorldPos);

           
            var required = new HashSet<Point>();
            for (int dx = -loadRadius; dx <= loadRadius; dx++)
            for (int dy = -loadRadius; dy <= loadRadius; dy++)
            {
                required.Add(new Point(playerChunk.X + dx, playerChunk.Y + dy));
            }

            var toUnload = new List<Point>();
            foreach (var kv in loaded)
            {
                if (!required.Contains(kv.Key)) toUnload.Add(kv.Key);
            }
            foreach (var p in toUnload) loaded.Remove(p);

            // load required chunks
            foreach (var p in required)
            {
                if (!loaded.ContainsKey(p))
                {
                    var chunkRect = new Rectangle(p.X * chunkSize, p.Y * chunkSize, chunkSize, chunkSize);
                    var rocks = rockManager.GenerateRocksIn(chunkRect);
                    var foods = foodManager.GenerateIn(chunkRect, rocks);
                    loaded[p] = new Chunk { Rect = chunkRect, Rocks = rocks, Foods = foods };
                }
            }
        }

        public IEnumerable<RockManager.Rock> GetRocks() {
            foreach (var c in loaded.Values) foreach (var r in c.Rocks) yield return r;
        }

        public IEnumerable<Food> GetFoods() {
            foreach (var c in loaded.Values) foreach (var f in c.Foods) yield return f;
        }

        // Remove a food item from whatever chunk contains it. Returns true if removed.
        public bool RemoveFood(Food food)
        {
            foreach (var kv in loaded)
            {
                var chunk = kv.Value;
                if (chunk.Rect.Contains((int)food.Position.X, (int)food.Position.Y))
                {
                    return chunk.Foods.Remove(food);
                }
                // also allow removal by bounds intersection (safety)
                for (int i = 0; i < chunk.Foods.Count; i++)
                {
                    if (chunk.Foods[i] == food)
                    {
                        chunk.Foods.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        private Point WorldToChunk(Vector2 worldPos)
        {
            int x = (int)Math.Floor(worldPos.X / chunkSize);
            int y = (int)Math.Floor(worldPos.Y / chunkSize);
            return new Point(x, y);
        }

        private class Chunk
        {
            public Rectangle Rect;
            public List<RockManager.Rock> Rocks = new List<RockManager.Rock>();
            public List<Food> Foods = new List<Food>();
        }
    }
}
