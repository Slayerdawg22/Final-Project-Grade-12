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
        int currentLevel = 1;
        int maxLevel = 4;
        List<Texture2D[]> levelSpriteSets = new List<Texture2D[]>();
        float baseCellScale = 0.25f;
        Texture2D heartSprite;
        int maxLives = 3;
        int currentLives = 3;

        // XP / evolution bar
        Texture2D xpBarEmptySprite; // optional decorative empty bar (set asset name below)

        int xpCurrent = 0;
        int xpToNext = 800; // total XP required to fill the evolution bar
        int xpBarWidth = 250; // default width in pixels
        int xpBarHeight = 10;  // default height in pixels
        int xpBarInset = 4; // inset for the green fill inside the decorative sprite
        // precise inner offsets inside the decorative sprite where the green fill should draw
        // adjust these to match the transparent inner area of your sprite
        int xpBarInnerOffsetX = 6;
        int xpBarInnerOffsetY = 6;

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
            xpBarEmptySprite = Content.Load<Texture2D>("Other Textures/XpBar");
            // scale xp bar sprite to a reasonable width
            {
                int maxDesiredWidth = 300;
                int desiredWidth = Math.Min(xpBarEmptySprite.Width, maxDesiredWidth);
                float scale = (float)desiredWidth / xpBarEmptySprite.Width;
                xpBarWidth = desiredWidth;
                xpBarHeight = Math.Max(1, (int)(xpBarEmptySprite.Height * scale));
                xpBarInset = Math.Max(2, xpBarHeight / 8);
            }

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

            
            
            // Load level sprites using pattern: "Level {lvl} Cell Textures/Lvl{lvl}Cell({i})"
            for (int lvl = 1; lvl <= maxLevel; lvl++)
            {
                Texture2D[] set = new Texture2D[3];
                for (int i = 1; i <= 3; i++)
                {
                    // Update this pattern if your Content paths differ. Use the exact pipeline asset name.
                    string assetKey = $"Level {lvl} Cell Textures/Lvl{lvl}Cell({i})";
                    set[i - 1] = Content.Load<Texture2D>(assetKey);
                }
                levelSpriteSets.Add(set);
            }
            cell.SetSprites(levelSpriteSets[currentLevel - 1], baseCellScale);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var ks = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || ks.IsKeyDown(Keys.Escape))
                Exit();

            // Boost input: space consumes XP and triggers a short directional boost
            if (ks.IsKeyDown(Keys.Space) && !prevKeyboardState.IsKeyDown(Keys.Space))
            {
                int boostCost = 350;
                if (xpCurrent >= boostCost)
                {
                    // determine boost direction from arrow keys; fallback to current velocity or up
                    Vector2 dir = Vector2.Zero;
                    if (ks.IsKeyDown(Keys.Up)) dir.Y = -1;
                    if (ks.IsKeyDown(Keys.Down)) dir.Y = 1;
                    if (ks.IsKeyDown(Keys.Left)) dir.X = -1;
                    if (ks.IsKeyDown(Keys.Right)) dir.X = 1;
                    if (dir.LengthSquared() <= 0.0001f)
                    {
                        dir = cell.Velocity;
                        if (dir.LengthSquared() <= 0.0001f) dir = new Vector2(0, -1);
                    }

                    xpCurrent = Math.Max(0, xpCurrent - boostCost);
                    float magnitude = cell.Speed * 6f;
                    cell.Boost(dir, magnitude, 0.25f);
                }
            }

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
                    // Account for camera scale (zoom) when mapping world -> screen.
                    float cameraScale = (float)Math.Pow(0.75, currentLevel - 1);
                    Vector2 vScreen = (vCenter - cell.Position) * cameraScale + new Vector2(vp.Width / 2f, vp.Height / 2f);
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

                    // grant XP based on food level (simple ratio: 50 XP per food level)
                    int xpGain = 25 * Math.Max(1, f.Level);
                    xpCurrent = Math.Min(xpToNext, xpCurrent + xpGain);

                    // If XP bar is full, attempt to evolve
                    if (xpCurrent >= xpToNext)
                    {
                        xpCurrent = 0; // reset XP
                        if (currentLevel < maxLevel)
                        {
                            currentLevel++;
                            // swap to new sprite set and slightly increase scale per level
                            float newScale = baseCellScale + (currentLevel - 1) * 0.06f;
                            cell.SetSprites(levelSpriteSets[currentLevel - 1], newScale);
                            // make next evolution harder
                            float multiplier = 2f;
                            xpToNext = Math.Max(xpToNext + 1, (int)(xpToNext * multiplier));
                        }
                        else
                        {
                            // at max level, keep XP reset
                        }
                    }

                    chunkManager.RemoveFood(f);
                }
            }

            prevKeyboardState = ks;


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(new Color(2, 13, 58));


            var vp = GraphicsDevice.Viewport;

            
            float cameraScale = (float)Math.Pow(0.75, currentLevel - 1);
            
            var translateToOrigin = Matrix.CreateTranslation(new Vector3(-cell.Position, 0f));
            var scaleMat = Matrix.CreateScale(cameraScale, cameraScale, 1f);
            var translateToScreen = Matrix.CreateTranslation(new Vector3(vp.Width / 2f, vp.Height / 2f, 0f));
            var camera = translateToOrigin * scaleMat * translateToScreen;

            _spriteBatch.Begin(transformMatrix: camera);
            foreach (var r in chunkManager.GetRocks()) r.Draw(_spriteBatch);
            cell.Draw(_spriteBatch);

            
            virus.Draw(_spriteBatch);

            foreach (var f in chunkManager.GetFoods()) f.Draw(_spriteBatch, pixel);
            particleManager.Draw(_spriteBatch, pixel);
            _spriteBatch.End();
            // Draw UI (no camera transform)
            _spriteBatch.Begin();
            
            for (int i = 0; i < currentLives; i++)
            {
                _spriteBatch.Draw(heartSprite, new Vector2(10 + i * 40, 1), null, Color.White, 0f, Vector2.Zero, 0.3f, SpriteEffects.None, 0f);
            }

            
            int barX = (vp.Width - xpBarWidth) / 2;
            int barY = vp.Height - xpBarHeight - 10; 

            
            if (xpBarEmptySprite != null)
            {
               
                int innerX = barX + xpBarInnerOffsetX;
                int innerY = barY + xpBarInnerOffsetY;
                int innerW = Math.Max(0, xpBarWidth - xpBarInnerOffsetX * 2);
                int innerH = Math.Max(0, xpBarHeight - xpBarInnerOffsetY * 2);

                float pct = xpToNext > 0 ? (float)xpCurrent / xpToNext : 0f;
                int fillW = (int)(innerW * MathHelper.Clamp(pct, 0f, 1f));
                if (fillW > 0 && innerH > 0)
                {
                    _spriteBatch.Draw(pixel, new Rectangle(innerX, innerY, fillW, innerH), Color.LimeGreen);
                }

                
                _spriteBatch.Draw(xpBarEmptySprite, new Rectangle(barX, barY, xpBarWidth, xpBarHeight), Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}