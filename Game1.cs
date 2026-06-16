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

        Texture2D xpBarEmptySprite;

        bool xpFull = false;
        bool evolutionMenuOpen = false;
        Texture2D evoButtonSprite;
        Texture2D evoMenuBgSprite;
        Texture2D evoCloseSprite;
        Texture2D[] flagellaSprites;
        bool hasFlagella = false;
        float flagellaBaseScale = 0.25f;
        Texture2D chlorophyllSprite;
        bool hasChlorophyll = false;
        float flagellaAnimTimer = 0f;
        float flagellaAnimSpeed = 0.12f;
        int flagellaIndex = 0;
        float flagellaAngle = 0f;
        float flagellaRotationSmoothing = 0.2f;
        float flagellaReturnSmoothing = 0.05f;
        MouseState prevMouseState;
        Rectangle evoBtnRect;
        int prevBackWidth = 0;
        int prevBackHeight = 0;
        bool resizedForMenu = false;

        int xpCurrent = 800;
        int xpToNext = 800;
        int xpBarWidth = 250;
        int xpBarHeight = 10;
        int xpBarInset = 4;
        int xpBarInnerOffsetX = 6;
        int xpBarInnerOffsetY = 6;

        FoodManager foodManager;
        Texture2D pixel;

        Cell cell;

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

            for (int lvl = 1; lvl <= maxLevel; lvl++)
            {
                Texture2D[] set = new Texture2D[3];
                for (int i = 1; i <= 3; i++)
                {
                    string assetKey = $"Level {lvl} Cell Textures/Lvl{lvl}Cell({i})";
                    set[i - 1] = Content.Load<Texture2D>(assetKey);
                }
                levelSpriteSets.Add(set);
            }
            cell.SetSprites(levelSpriteSets[currentLevel - 1], baseCellScale);

            evoButtonSprite = Content.Load<Texture2D>("Other Textures/EvolveBtn");
            evoMenuBgSprite = Content.Load<Texture2D>("BackGrounds/Evolution Background");
            evoCloseSprite = Content.Load<Texture2D>("Other Textures/ExitBtn");

            flagellaSprites = new Texture2D[] {
                Content.Load<Texture2D>("Perm Evolutions/Flagella(1v)"),
                Content.Load<Texture2D>("Perm Evolutions/Flagella(2v)"),
                Content.Load<Texture2D>("Perm Evolutions/Flagella(3v)")
            };
            chlorophyllSprite = Content.Load<Texture2D>("Perm Evolutions/Chlorophyll");
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var ks = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || ks.IsKeyDown(Keys.Escape))
                Exit();

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
                    int closePad = 12;
                    Rectangle closeRect = new Rectangle(menuX + scaledW - closeW - closePad, menuY + closePad, closeW, closeH);

                    int colCount = 4;
                    int colW = scaledW / colCount;
                    Rectangle firstColRect = new Rectangle(menuX + 0 * colW, menuY, colW, scaledH);
                    Rectangle secondColRect = new Rectangle(menuX + 1 * colW, menuY, colW, scaledH);
                    Rectangle thirdColRect = new Rectangle(menuX + 2 * colW, menuY, colW, scaledH);

                    if (msEarly.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                    {
                        if (closeRect.Contains(msEarly.Position))
                        {
                            evolutionMenuOpen = false;
                        }
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
                        }
                        else if (secondColRect.Contains(msEarly.Position))
                        {
                            hasFlagella = true;
                            cell.Speed *= 1.35f;
                            xpCurrent = 0;
                            xpFull = false;
                            evolutionMenuOpen = false;
                        }
                        else if (thirdColRect.Contains(msEarly.Position))
                        {
                            hasChlorophyll = true;
                            xpCurrent = 0;
                            xpFull = false;
                            evolutionMenuOpen = false;
                        }
                    }
                }

                prevMouseState = msEarly;
                prevKeyboardState = ks;
                base.Update(gameTime);
                return;
            }

            if (ks.IsKeyDown(Keys.Space) && !prevKeyboardState.IsKeyDown(Keys.Space))
            {
                int boostCost = 350;
                if (xpCurrent >= boostCost)
                {
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

            if (hasFlagella && flagellaSprites != null && flagellaSprites.Length > 0)
            {
                flagellaAnimTimer += dt;
                if (flagellaAnimTimer >= flagellaAnimSpeed)
                {
                    flagellaAnimTimer = 0f;
                    flagellaIndex = (flagellaIndex + 1) % flagellaSprites.Length;
                }
            }

            if (hasFlagella)
            {
                Vector2 v = cell.Velocity;
                if (v.LengthSquared() > 0.01f)
                {
                    float target = (float)Math.Atan2(v.Y, v.X) + MathHelper.Pi;
                    float diff = MathHelper.WrapAngle(target - flagellaAngle);
                    flagellaAngle += diff * flagellaRotationSmoothing;
                }
                else
                {
                    float diff = MathHelper.WrapAngle(0f - flagellaAngle);
                    flagellaAngle += diff * flagellaReturnSmoothing;
                }
            }

            var cb = cell.Bounds;
            Vector2 cCenter = cell.Position + new Vector2(cb.Width / 2f, cb.Height / 2f);
            float cRadius = Math.Max(cb.Width, cb.Height) * 0.5f;

            if (!virus.Active)
            {
                virusRespawnTimer -= dt;
                if (virusRespawnTimer <= 0f)
                {
                    float angle = (float)(rng.NextDouble() * Math.PI * 2.0);
                    float dist = (float)(rng.NextDouble() * 400.0 + 300.0);
                    Vector2 spawnPos = cell.Position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                    virus.Activate(spawnPos, cell.Speed * 0.9f);
                }
            }
            else
            {
                virus.Update(gameTime, cCenter);
            }

            foodManager.Update(gameTime);

            if (rockManager != null && ks.IsKeyDown(Keys.R) && !prevKeyboardState.IsKeyDown(Keys.R))
            {
                rockManager.GenerateRandom();

                foodManager.GenerateInitial(rockManager.Rocks);
            }

            chunkManager.Update(cell.Position);

            cell.ResolveCollisions(chunkManager.GetRocks());
            virus.ResolveCollisions(chunkManager.GetRocks());

            if (virus.Active)
            {
                var vb = virus.Bounds;
                Vector2 vCenter = virus.Position + new Vector2(vb.Width / 2f, vb.Height / 2f);
                float vRadius = Math.Max(vb.Width, vb.Height) * 0.5f;

                float distSq = Vector2.DistanceSquared(cCenter, vCenter);
                float sumRadius = cRadius + vRadius;
                if (distSq <= sumRadius * sumRadius)
                {
                    particleManager.SpawnAt(vCenter, 48, Color.Red, 4f, 1.0f, 1.8f);

                    currentLives = Math.Max(0, currentLives - 1);
                    virus.Deactivate();
                    virusRespawnTimer = virusRespawnDelay;
                }
                else
                {
                    var vp = GraphicsDevice.Viewport;
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

                    int xpGain = 25 * Math.Max(1, f.Level);
                    xpCurrent = Math.Min(xpToNext, xpCurrent + xpGain);

                    if (xpCurrent >= xpToNext)
                    {
                        xpCurrent = xpToNext;
                        xpFull = true;
                    }

                    chunkManager.RemoveFood(f);
                }
            }

            // Passive XP gain from chlorophyll
            if (hasChlorophyll)
            {
                float passiveXpPerSecond = 60f;
                int passiveXpGain = (int)(passiveXpPerSecond * dt);
                xpCurrent = Math.Min(xpToNext, xpCurrent + passiveXpGain);

                if (xpCurrent >= xpToNext)
                {
                    xpCurrent = xpToNext;
                    xpFull = true;
                }
            }

            prevKeyboardState = ks;
            var ms = Mouse.GetState();
            if (!evolutionMenuOpen && xpFull)
            {
                evoBtnRect = new Rectangle(560, 420, 125, 50);
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
                var vp = GraphicsDevice.Viewport;
                if (evoMenuBgSprite != null)
                {
                    int menuW = evoMenuBgSprite.Width;
                    int menuH = evoMenuBgSprite.Height;

                    vp = GraphicsDevice.Viewport;

                    int scaledW = menuW;
                    int scaledH = menuH;
                    int menuX = (vp.Width - scaledW) / 2;
                    int menuY = (vp.Height - scaledH) / 2;

                    int closeW = evoCloseSprite != null ? Math.Max(8, evoCloseSprite.Width / 2) : 0;
                    int closeH = evoCloseSprite != null ? Math.Max(8, evoCloseSprite.Height / 2) : 0;
                    int closePad = 8;
                    Rectangle closeRect = new Rectangle(menuX + scaledW - closeW - closePad, menuY + closePad, closeW, closeH);

                    if (ms.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                    {
                        if (closeRect.Contains(ms.Position))
                        {
                            evolutionMenuOpen = false;
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

            if (hasFlagella && flagellaSprites != null && flagellaSprites.Length > 0)
            {
                var fb = flagellaSprites[flagellaIndex % flagellaSprites.Length];
                var cb = cell.Bounds;
                Vector2 cCenter = cell.Position + new Vector2(cb.Width / 2f, cb.Height / 2f);
                float radius = Math.Max(cb.Width, cb.Height) * 0.5f;
                Vector2 dir = new Vector2((float)Math.Cos(flagellaAngle), (float)Math.Sin(flagellaAngle));
                // Calculate flagella scale: base scale * 1.25^(currentLevel-1) for 25% growth per level
                float flagellaScale = flagellaBaseScale * (float)Math.Pow(1.25f, currentLevel - 1);
                float spriteHalfHeight = (fb.Height * flagellaScale) * 0.5f;
                float placeDist = Math.Max(0f, radius + spriteHalfHeight - 2f - 10f);
                Vector2 place = cCenter + dir * placeDist;
                Vector2 origin = new Vector2(fb.Width / 2f, fb.Height / 2f);
                float drawRotation = flagellaAngle - MathHelper.PiOver2;
                _spriteBatch.Draw(fb, place, null, Color.White, drawRotation, origin, flagellaScale, SpriteEffects.None, 0f);
            }

            if (hasChlorophyll && chlorophyllSprite != null)
            {
                
                var cb2 = cell.Bounds;
                Vector2 cCenter2 = cell.Position + new Vector2(cb2.Width / 2f, cb2.Height / 2f);
                float radius2 = Math.Max(cb2.Width, cb2.Height) * 0.5f;
                float angle = MathHelper.ToRadians(400f); // Bottom-right corner (~45 degrees)
                Vector2 dir2 = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                int destW = Math.Max(1, chlorophyllSprite.Width / 8);
                int destH = Math.Max(1, chlorophyllSprite.Height / 8);
                float placeDist2 = radius2 - (destH * 0.5f) - 10f;
                Vector2 place2 = cCenter2 + dir2 * placeDist2;
                int drawX = (int)(place2.X - destW / 2f);
                int drawY = (int)(place2.Y - destH / 2f);
                _spriteBatch.Draw(chlorophyllSprite, new Rectangle(drawX, drawY, destW, destH), Color.White);
            }

            foreach (var f in chunkManager.GetFoods()) f.Draw(_spriteBatch, pixel);
            particleManager.Draw(_spriteBatch, pixel);
            _spriteBatch.End();
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

            if (xpFull)
            {
                evoBtnRect = new Rectangle(560, 420, 125, 50);
                if (evoButtonSprite != null)
                {
                    _spriteBatch.Draw(evoButtonSprite, evoBtnRect, Color.White);
                }
            }

            if (evolutionMenuOpen)
            {
                if (evoMenuBgSprite != null)
                {
                    int menuW = evoMenuBgSprite.Width;
                    int menuH = evoMenuBgSprite.Height;
                    float menuScale = (float)vp.Width / menuW;

                    int scaledW = Math.Max(1, (int)(menuW * menuScale));
                    int scaledH = Math.Max(1, (int)(menuH * menuScale));
                    int menuX = (vp.Width - scaledW) / 2;
                    int menuY = (vp.Height - scaledH) / 2;

                    _spriteBatch.Draw(evoMenuBgSprite, new Rectangle(menuX, menuY, scaledW, scaledH), Color.White);

                    int colCount = 4;
                    int colW = scaledW / colCount;
                    Rectangle firstColRect = new Rectangle(menuX + 0 * colW, menuY, colW, scaledH);
                    if (currentLevel < maxLevel)
                    {
                        var previewSet = levelSpriteSets[currentLevel];
                        if (previewSet != null && previewSet.Length > 0)
                        {
                            var tex = previewSet[0];
                            float previewScale = (baseCellScale + (currentLevel) * 0.06f) * 0.75f;
                            int drawW = (int)(tex.Width * previewScale);
                            int drawH = (int)(tex.Height * previewScale);
                            int offset = Math.Max(4, firstColRect.Width / 10);
                            int drawX = firstColRect.X + (firstColRect.Width - drawW) / 2 + offset;
                            int drawY = firstColRect.Y + (firstColRect.Height - drawH) / 2;
                            _spriteBatch.Draw(tex, new Rectangle(drawX, drawY, drawW, drawH), Color.White);
                        }
                    }

                    Rectangle secondColRect = new Rectangle(menuX + 1 * colW, menuY, colW, scaledH);
                    if (flagellaSprites != null && flagellaSprites.Length > 0)
                    {
                        var ftex = flagellaSprites[0];
                        int fw = Math.Min((int)(colW * 0.5f) - 16, ftex.Width);
                        int fh = Math.Min((int)(scaledH * 0.5f) - 16, ftex.Height);
                        int fx = secondColRect.X + (secondColRect.Width - fw) / 2;
                        int fy = secondColRect.Y + (secondColRect.Height - fh) / 2;
                        _spriteBatch.Draw(ftex, new Rectangle(fx, fy, fw, fh), Color.White);
                    }

                    // Third column: show chlorophyll preview if available
                    Rectangle thirdColRect = new Rectangle(menuX + 2 * colW, menuY, colW, scaledH);
                    if (chlorophyllSprite != null)
                    {
                        float pad = 16f;
                        float availableW = Math.Max(1, thirdColRect.Width - (int)pad);
                        float availableH = Math.Max(1, thirdColRect.Height - (int)pad);
                        float scaleC = Math.Min(0.375f, Math.Min(availableW / chlorophyllSprite.Width, availableH / chlorophyllSprite.Height));
                        int cw = Math.Max(1, (int)(chlorophyllSprite.Width * scaleC));
                        int ch = Math.Max(1, (int)(chlorophyllSprite.Height * scaleC));
                        int cx = thirdColRect.X + (thirdColRect.Width - cw) / 2;
                        int cy = thirdColRect.Y + (thirdColRect.Height - ch) / 2;
                        _spriteBatch.Draw(chlorophyllSprite, new Rectangle(cx, cy, cw, ch), Color.White);
                    }

                    if (evoCloseSprite != null)
                    {
                        int closeW = Math.Max(8, (int)(evoCloseSprite.Width * 0.0625f));
                        int closeH = Math.Max(8, (int)(evoCloseSprite.Height * 0.0625f));
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
