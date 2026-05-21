using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
namespace Final_Project
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D[] cellSprites;
        Texture2D nucleusSprite;
        FoodManager foodManager;
        Texture2D pixel;

        Cell cell;


        RockManager rockManager;
        ChunkManager chunkManager;
        ParticleManager particleManager;
        private KeyboardState prevKeyboardState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {

            prevKeyboardState = Keyboard.GetState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            cellSprites = new Texture2D[]
            {
                        Content.Load<Texture2D>("Cell Textures/Base(1)"),
                        Content.Load<Texture2D>("Cell Textures/Base(2)"),
                        Content.Load<Texture2D>("Cell Textures/Base(3)")
            };

            nucleusSprite = Content.Load<Texture2D>("Cell Textures/nucleus");

            cell = new Cell(new Vector2(300, 300), cellSprites, nucleusSprite);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });


            foodManager = new FoodManager(GraphicsDevice);

            string[] foodKeys = new string[5];

            foodKeys[2] = "Foods/XFood2";
            foodKeys[3] = "Foods/YFood3";
            foodKeys[4] = "Foods/YFood4";

            foodManager.LoadTextures((level) => foodKeys[level], Content);
            foodManager.TotalCount = 25;

            rockManager = new RockManager(GraphicsDevice);

            string[] mediumKeys = new string[] { "Rocks/MedRock(1)", "Rocks/MedRock(2)", "Rocks/MedRock(3)" };
            string[] verticalKeys = new string[] { "Rocks/VertRock(1)", "Rocks/VertRock(2)" };
            rockManager.LoadTextures(mediumKeys, verticalKeys, Content);

            rockManager.DrawCount = 6;
            rockManager.MediumRatio = 0.8f;
            
            chunkManager = new ChunkManager(rockManager, foodManager, 1024, 1);
            
            chunkManager.Update(cell.Position);

            particleManager = new ParticleManager();
        }





        protected override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || ks.IsKeyDown(Keys.Escape))
                Exit();

            cell.Update(gameTime);

            foodManager.Update(gameTime);

            // rock testing
            if (rockManager != null && ks.IsKeyDown(Keys.R) && !prevKeyboardState.IsKeyDown(Keys.R))
            {
                rockManager.GenerateRandom();
                
                foodManager.GenerateInitial(rockManager.Rocks);
            }


            chunkManager.Update(cell.Position);

            cell.ResolveCollisions(chunkManager.GetRocks());

            particleManager.Update(gameTime);

            var foodsList = new List<Food>(chunkManager.GetFoods());
            float eatThreshold = 4f; 

            var cb = cell.Bounds;
            Vector2 cCenter = cell.Position + new Vector2(cb.Width / 2f, cb.Height / 2f);
            float cRadius = Math.Max(cb.Width, cb.Height) * 0.5f;

            foreach (var f in foodsList)
            {
                var fb = f.Bounds;
                float closestX = MathHelper.Clamp(cCenter.X, fb.Left, fb.Right);
                float closestY = MathHelper.Clamp(cCenter.Y, fb.Top, fb.Bottom);
                Vector2 closest = new Vector2(closestX, closestY);
                float dist = Vector2.Distance(cCenter, closest);
                float penetration = cRadius - dist;
                if (penetration >= eatThreshold)
                {
                    Vector2 foodCenter = new Vector2(fb.X + fb.Width / 2f, fb.Y + fb.Height / 2f);
                    int count = 6 + f.Level * 4;
                    Color col = f.Level == 1 ? Color.LimeGreen : Color.Gold;
                    particleManager.SpawnAt(foodCenter, count, col, 3f);

                    
                    chunkManager.RemoveFood(f);
                }
            }

            prevKeyboardState = ks;

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
           
            GraphicsDevice.Clear(new Color(2, 13, 58));

            
            var vp = GraphicsDevice.Viewport;
            var camera = Matrix.CreateTranslation(new Vector3(-cell.Position + new Vector2(vp.Width / 2f, vp.Height / 2f), 0f));

            _spriteBatch.Begin(transformMatrix: camera);
            foreach (var r in chunkManager.GetRocks()) r.Draw(_spriteBatch);
            cell.Draw(_spriteBatch);
            foreach (var f in chunkManager.GetFoods()) f.Draw(_spriteBatch, pixel);
            particleManager.Draw(_spriteBatch, pixel);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
