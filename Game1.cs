using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Final_Project
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D[] cellSprites;
        Texture2D nucleusSprite;
        // UI hearts
        Texture2D heartSprite;
        int maxLives = 3;
        int currentLives = 3;

        FoodManager foodManager;
        Texture2D pixel;

        Cell cell;

        // virus fields
        Texture2D[] virusSprites;
        Texture2D virusNucleusSprite;
        Virus virus;
        Random rng = new Random();
        float virusRespawnTimer = 0f;
        float virusRespawnDelay = 5f;

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

            
            virusSprites = new Texture2D[]
            {
                Content.Load<Texture2D>("Virus Textures/VirusBase(1)"),
                Content.Load<Texture2D>("Virus Textures/VirusBase(2)"),
                Content.Load<Texture2D>("Virus Textures/VirusBase(3)")
            };
            virusNucleusSprite = Content.Load<Texture2D>("Virus Textures/VirusNucleus");


            virus = new Virus(new Vector2(500, 300), virusSprites, virusNucleusSprite);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            
            heartSprite = Content.Load<Texture2D>("Other Textures/life");


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
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var ks = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || ks.IsKeyDown(Keys.Escape))
                Exit();

            cell.Update(gameTime);

            // compute cell center for virus targeting and collision checks
            var cb = cell.Bounds;
            Vector2 cCenter = cell.Position + new Vector2(cb.Width / 2f, cb.Height / 2f);
            float cRadius = Math.Max(cb.Width, cb.Height) * 0.5f;

            // virus spawn logic: respawn after delay at a random position around the cell
            if (!virus.Active)
            {
                virusRespawnTimer -= dt;
                if (virusRespawnTimer <= 0f)
                {
                    float angle = (float)(rng.NextDouble() * Math.PI * 2.0);
                    float dist = (float)(rng.NextDouble() * 400.0 + 300.0); // spawn 300-700 units away
                    Vector2 spawnPos = cell.Position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                    // set speed slightly less than cell speed
                    virus.Activate(spawnPos, cell.Speed * 0.9f);
                }
            }
            else
            {
                // active: steer towards the cell center
                virus.Update(gameTime, cCenter);
            }

            foodManager.Update(gameTime);

            // rock testing
            if (rockManager != null && ks.IsKeyDown(Keys.R) && !prevKeyboardState.IsKeyDown(Keys.R))
            {
                rockManager.GenerateRandom();

                foodManager.GenerateInitial(rockManager.Rocks);
            }

            chunkManager.Update(cell.Position);

            // resolve collisions for both cell and virus
            cell.ResolveCollisions(chunkManager.GetRocks());
            virus.ResolveCollisions(chunkManager.GetRocks());

            // if virus is active, check for collision using circle collision and for leaving the camera view
            if (virus.Active)
            {
                var vb = virus.Bounds;
                Vector2 vCenter = virus.Position + new Vector2(vb.Width / 2f, vb.Height / 2f);
                float vRadius = Math.Max(vb.Width, vb.Height) * 0.5f;

                // check circle collision between cell and virus
                float distSq = Vector2.DistanceSquared(cCenter, vCenter);
                float sumRadius = cRadius + vRadius;
                if (distSq <= sumRadius * sumRadius)
                {
                    // spawn particles at the virus center only (longer lived particles for death)
                    particleManager.SpawnAt(vCenter, 48, Color.Red, 4f, 1.0f, 1.8f);

                    // decrement life and deactivate virus
                    currentLives = Math.Max(0, currentLives - 1);
                    virus.Deactivate();
                    virusRespawnTimer = virusRespawnDelay;
                }
                else
                {
                    // if the virus goes off the camera view (you outran it), make it disappear
                    var vp = GraphicsDevice.Viewport;
                    Vector2 vScreen = vCenter - cell.Position + new Vector2(vp.Width / 2f, vp.Height / 2f);
                    int margin = 8;
                    if (vScreen.X < -margin || vScreen.X > vp.Width + margin || vScreen.Y < -margin || vScreen.Y > vp.Height + margin)
                    {
                        virus.Deactivate();
                        virusRespawnTimer = virusRespawnDelay;
                    }
                }
            }

            particleManager.Update(gameTime);

            var foodsList = new List<Food>(chunkManager.GetFoods());
            float eatThreshold = 4f;

            // cb, cCenter, cRadius already computed above

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

            // draw the virus (uses same camera centered on the cell; move cell to change camera)
            virus.Draw(_spriteBatch);

            foreach (var f in chunkManager.GetFoods()) f.Draw(_spriteBatch, pixel);
            particleManager.Draw(_spriteBatch, pixel);
            _spriteBatch.End();

            // Draw UI (no camera transform)
            _spriteBatch.Begin();
            for (int i = 0; i < maxLives; i++)
            {
                _spriteBatch.Draw(heartSprite, new Vector2(10 + i * 40, 1), null, Color.White, 0f, Vector2.Zero, 0.3f, SpriteEffects.None, 0f);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}