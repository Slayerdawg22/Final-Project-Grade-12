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

        // evolution UI
        bool xpFull = false;
        bool evolutionMenuOpen = false;
        Texture2D evoButtonSprite;
        Texture2D evoMenuBgSprite;
        Texture2D evoCloseSprite;
        // Flagella (three-frame animation)
        Texture2D[] flagellaSprites;
        bool hasFlagella = false;
        float flagellaAnimTimer = 0f;
        float flagellaAnimSpeed = 0.12f;
        int flagellaIndex = 0;
        // flagella rotation to follow cell direction (smoothed like nucleus)
        float flagellaAngle = 0f;
        float flagellaRotationSmoothing = 0.2f;
        float flagellaReturnSmoothing = 0.05f;
        MouseState prevMouseState;
        Rectangle evoBtnRect;
        int prevBackWidth = 0;
        int prevBackHeight = 0;
        bool resizedForMenu = false;

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
            // initialize mouse state to avoid missing the first click edge
            prevMouseState = Mouse.GetState();

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
                levelSpriteSets.Add(set);            }
            cell.SetSprites(levelSpriteSets[currentLevel - 1], baseCellScale);

            // Evo UI assets - update these asset keys to your actual names
            evoButtonSprite = Content.Load<Texture2D>("Other Textures/EvolveBtn");
            evoMenuBgSprite = Content.Load<Texture2D>("BackGrounds/Evolution Background");
            evoCloseSprite = Content.Load<Texture2D>("Other Textures/ExitBtn");
            
            flagellaSprites = new Texture2D[] {
                Content.Load<Texture2D>("Perm Evolutions/Flagella(1v)"),
                Content.Load<Texture2D>("Perm Evolutions/Flagella(2v)"),
                Content.Load<Texture2D>("Perm Evolutions/Flagella(3v)")
            };
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var ks = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || ks.IsKeyDown(Keys.Escape))
                Exit();

            // If evolution menu is open, pause world updates and only handle menu input (close button + selections)
            var msEarly = Mouse.GetState();
            if (evolutionMenuOpen)
            {
                var vpEarly = GraphicsDevice.Viewport;
                if (evoMenuBgSprite != null)
                {
                    int menuW = evoMenuBgSprite.Width;
                    int menuH = evoMenuBgSprite.Height;
                    int scaledW = menuW;
                    int scaledH = menuH;
                    int menuX = (vpEarly.Width - scaledW) / 2;
                    int menuY = (vpEarly.Height - scaledH) / 2;
                    int closeW = evoCloseSprite != null ? Math.Max(8, (int)(evoCloseSprite.Width * 0.25f)) : 0;
                    int closeH = evoCloseSprite != null ? Math.Max(8, (int)(evoCloseSprite.Height * 0.25f)) : 0;
                    int closePad = 12; // a little further from the corner
                    Rectangle closeRect = new Rectangle(menuX + scaledW - closeW - closePad, menuY + closePad, closeW, closeH);

                    // columns
                    int colCount = 4;
                    int colW = scaledW / colCount;
                    Rectangle firstColRect = new Rectangle(menuX + 0 * colW, menuY, colW, scaledH);
                    Rectangle secondColRect = new Rectangle(menuX + 1 * colW, menuY, colW, scaledH);

                    if (msEarly.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                    {
                        // close
                        if (closeRect.Contains(msEarly.Position))
                        {
                            evolutionMenuOpen = false;
                            if (resizedForMenu)
                            {
                                _graphics.PreferredBackBufferWidth = Math.Max(1, prevBackWidth);
                                _graphics.PreferredBackBufferHeight = Math.Max(1, prevBackHeight);
                                _graphics.ApplyChanges();
                                resizedForMenu = false;
                            }
                        }
                        // first column: size increase
                        else if (firstColRect.Contains(msEarly.Position))
                        {
                            if (currentLevel < maxLevel)
                            {
                                currentLevel++;
                                float newScale = baseCellScale + (currentLevel - 1) * 0.06f;
                                cell.SetSprites(levelSpriteSets[currentLevel - 1], newScale);
                                float multiplier = 2f;
                                xpToNext = Math.Max(xpToNext + 1, (int)(xpToNext * multiplier));
                            }
                            xpCurrent = 0;
                            xpFull = false;
                            evolutionMenuOpen = false;
                            if (resizedForMenu)
                            {
                                _graphics.PreferredBackBufferWidth = Math.Max(1, prevBackWidth);
                                _graphics.PreferredBackBufferHeight = Math.Max(1, prevBackHeight);
                                _graphics.ApplyChanges();
                                resizedForMenu = false;
                            }
                        }
                        // second column: attach flagella
                        else if (secondColRect.Contains(msEarly.Position))
                        {
                            hasFlagella = true;
                            xpCurrent = 0;
                            xpFull = false;
                            evolutionMenuOpen = false;
                            if (resizedForMenu)
                            {
                                _graphics.PreferredBackBufferWidth = Math.Max(1, prevBackWidth);
                                _graphics.PreferredBackBufferHeight = Math.Max(1, prevBackHeight);
                                _graphics.ApplyChanges();
                                resizedForMenu = false;
                            }
                        }
                    }
                }

                prevMouseState = msEarly;
                prevKeyboardState = ks;
                base.Update(gameTime);
                return;
            }

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

            // animate flagella if attached
            if (hasFlagella && flagellaSprites != null && flagellaSprites.Length > 0)
            {
                flagellaAnimTimer += dt;
                if (flagellaAnimTimer >= flagellaAnimSpeed)
                {
                    flagellaAnimTimer = 0f;
                    flagellaIndex = (flagellaIndex + 1) % flagellaSprites.Length;
                }
            }

            // smooth rotation to face opposite of movement (so flagella sits at the back)
            if (hasFlagella)
            {
                Vector2 v = cell.Velocity;
                if (v.LengthSquared() > 0.01f)
                {
                    float target = (float)Math.Atan2(v.Y, v.X) + MathHelper.Pi; // back of cell
                    float diff = MathHelper.WrapAngle(target - flagellaAngle);
                    flagellaAngle += diff * flagellaRotationSmoothing;
                }
                else
                {
                    float diff = MathHelper.WrapAngle(0f - flagellaAngle);
                    flagellaAngle += diff * flagellaReturnSmoothing;
                }
            }

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

                    // If XP bar is full, set flag to allow manual evolution via UI
                    if (xpCurrent >= xpToNext)
                    {
                        xpCurrent = xpToNext;
                        xpFull = true;
                    }

                    chunkManager.RemoveFood(f);
                }
            }

            prevKeyboardState = ks;
            // mouse handling for evo UI
            var ms = Mouse.GetState();
            // if XP bar full show evo button; clicking opens menu
            if (!evolutionMenuOpen && xpFull)
            {
                // use the exact rectangle provided by the user for the evo button
                evoBtnRect = new Rectangle(570, 420, 40, 40);
                if (ms.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                {
                    if (evoBtnRect.Contains(ms.Position))
                    {
                        evolutionMenuOpen = true;
                    }
                }
            }

            if (evolutionMenuOpen)
            {
                // menu centered and scaled to fit viewport if too large
                var vp = GraphicsDevice.Viewport;
                if (evoMenuBgSprite != null)
                {
                    int menuW = evoMenuBgSprite.Width;
                    int menuH = evoMenuBgSprite.Height;

                    // If we haven't resized the window to match the menu, do so now.
                    if (!resizedForMenu)
                    {
                        prevBackWidth = _graphics.PreferredBackBufferWidth;
                        prevBackHeight = _graphics.PreferredBackBufferHeight;
                        _graphics.PreferredBackBufferWidth = menuW;
                        _graphics.PreferredBackBufferHeight = menuH;
                        _graphics.ApplyChanges();
                        resizedForMenu = true;
                    }

                    // Since window may have been resized, refresh viewport
                    vp = GraphicsDevice.Viewport;

                    int scaledW = menuW;
                    int scaledH = menuH;
                    int menuX = (vp.Width - scaledW) / 2;
                    int menuY = (vp.Height - scaledH) / 2;

                    int closeW = evoCloseSprite != null ? Math.Max(8, evoCloseSprite.Width / 2) : 0; // make exit smaller
                    int closeH = evoCloseSprite != null ? Math.Max(8, evoCloseSprite.Height / 2) : 0;
                    int closePad = 8;
                    Rectangle closeRect = new Rectangle(menuX + scaledW - closeW - closePad, menuY + closePad, closeW, closeH);

                    if (ms.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                    {
                        if (closeRect.Contains(ms.Position))
                        {
                            evolutionMenuOpen = false;
                            // restore original window size
                            if (resizedForMenu)
                            {
                                _graphics.PreferredBackBufferWidth = Math.Max(1, prevBackWidth);
                                _graphics.PreferredBackBufferHeight = Math.Max(1, prevBackHeight);
                                _graphics.ApplyChanges();
                                resizedForMenu = false;
                            }
                        }
                    }
                }
            }

            prevMouseState = Mouse.GetState();


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(new Color(2, 13, 58));
            MouseState ms = Mouse.GetState();

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

            // draw flagella attached to cell (world space) close to the center (textures pre-rotated at ~5 o'clock)
            if (hasFlagella && flagellaSprites != null && flagellaSprites.Length > 0)
            {
                var fb = flagellaSprites[flagellaIndex % flagellaSprites.Length];
                // compute cell center and place flagella so its inner edge sits on the cell circumference
                var cb = cell.Bounds;
                Vector2 cCenter = cell.Position + new Vector2(cb.Width / 2f, cb.Height / 2f);
                // diagonal vector (bottom-right) normalized
                // cell radius in pixels
                float radius = Math.Max(cb.Width, cb.Height) * 0.5f;
                // draw position: center + offset computed using flagellaAngle so sprite sits at back edge
                Vector2 dir = new Vector2((float)Math.Cos(flagellaAngle), (float)Math.Sin(flagellaAngle));
                float spriteHalfHeight = (fb.Height * cell.Scale) * 0.5f;
                // bring flagella 10 pixels closer to the cell
                float placeDist = Math.Max(0f, radius + spriteHalfHeight - 2f - 10f);
                Vector2 place = cCenter + dir * placeDist;
                Vector2 origin = new Vector2(fb.Width / 2f, fb.Height / 2f);
                // textures are oriented pointing downwards; rotate by flagellaAngle minus 90deg to align
                float drawRotation = flagellaAngle - MathHelper.PiOver2;
                _spriteBatch.Draw(fb, place, null, Color.White, drawRotation, origin, cell.Scale, SpriteEffects.None, 0f);
            }

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

            // Draw evo button when XP is full using the exact rectangle requested
            if (xpFull)
            {
                evoBtnRect = new Rectangle(570, 420, 40, 40);
                if (evoButtonSprite != null)
                {
                    _spriteBatch.Draw(evoButtonSprite, evoBtnRect, Color.White);
                }
            }
            

            // Draw evolution menu if open
            if (evolutionMenuOpen)
            {
                if (evoMenuBgSprite != null)
                {
                    int menuW = evoMenuBgSprite.Width;
                    int menuH = evoMenuBgSprite.Height;
                    float scaleX = (vp.Width * 0.9f) / menuW;
                    float scaleY = (vp.Height * 0.9f) / menuH;
                    float menuScale = Math.Min(1f, Math.Min(scaleX, scaleY));

                    int scaledW = Math.Max(1, (int)(menuW * menuScale));
                    int scaledH = Math.Max(1, (int)(menuH * menuScale));
                    int menuX = (vp.Width - scaledW) / 2;
                    int menuY = (vp.Height - scaledH) / 2;

                    _spriteBatch.Draw(evoMenuBgSprite, new Rectangle(menuX, menuY, scaledW, scaledH), Color.White);

                    // draw preview in first column: show the sprite for the next level
                    int colCount = 4;
                    int colW = scaledW / colCount;
                    Rectangle firstColRect = new Rectangle(menuX + 0 * colW, menuY, colW, scaledH);
                    if (currentLevel < maxLevel)
                    {
                        var previewSet = levelSpriteSets[currentLevel]; // next level set
                        if (previewSet != null && previewSet.Length > 0)
                        {
                            var tex = previewSet[0];
                            // compute target area centered inside first column
                            float previewScale = baseCellScale + (currentLevel) * 0.06f; // scale for next level
                            int drawW = (int)(tex.Width * previewScale);
                            int drawH = (int)(tex.Height * previewScale);
                            int offset = Math.Max(4, firstColRect.Width / 10); // small right shift
                            int drawX = firstColRect.X + (firstColRect.Width - drawW) / 2 + offset;
                            int drawY = firstColRect.Y + (firstColRect.Height - drawH) / 2;
                            _spriteBatch.Draw(tex, new Rectangle(drawX, drawY, drawW, drawH), Color.White);
                        }
                    }

                    // draw second column preview: flagella (use first flagella texture)
                    Rectangle secondColRect = new Rectangle(menuX + 1 * colW, menuY, colW, scaledH);
                    if (flagellaSprites != null && flagellaSprites.Length > 0)
                    {
                        var ftex = flagellaSprites[0];
                        int fw = Math.Min(colW - 16, ftex.Width);
                        int fh = Math.Min(scaledH - 16, ftex.Height);
                        int fx = secondColRect.X + (secondColRect.Width - fw) / 2;
                        int fy = secondColRect.Y + (secondColRect.Height - fh) / 2;
                        _spriteBatch.Draw(ftex, new Rectangle(fx, fy, fw, fh), Color.White);
                    }

                    if (evoCloseSprite != null)
                    {
                        int closeW = Math.Max(8, (int)(evoCloseSprite.Width * 0.25f));
                        int closeH = Math.Max(8, (int)(evoCloseSprite.Height * 0.25f));
                        int closePad = 12;
                        _spriteBatch.Draw(evoCloseSprite, new Rectangle(menuX + scaledW - closeW - closePad, menuY + closePad, closeW, closeH), Color.White);
                    }
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}