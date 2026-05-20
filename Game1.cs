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

            // pixel lvl 1 food
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });


            foodManager = new FoodManager(GraphicsDevice);

            string[] foodKeys = new string[5];

            foodKeys[2] = "Foods/XFood2";
            foodKeys[3] = "Foods/YFood3";
            foodKeys[4] = "Foods/YFood4";

            foodManager.LoadTextures((level) => foodKeys[level], Content);
            foodManager.TotalCount = 25; // change overall amount here
            foodManager.GenerateInitial();

            rockManager = new RockManager(GraphicsDevice);

            string[] mediumKeys = new string[] { "Rocks/MedRock(1)", "Rocks/MedRock(2)", "Rocks/MedRock(3)" };
            string[] verticalKeys = new string[] { "Rocks/VertRock(1)", "Rocks/VertRock(2)" };
            rockManager.LoadTextures(mediumKeys, verticalKeys, Content);

            rockManager.DrawCount = 4;
            rockManager.MediumRatio = 0.8f; // ~80% medium, 20% vertical
            rockManager.GenerateRandom();

            // now generate food, passing current rocks so food won't spawn on them
            foodManager.GenerateInitial(rockManager.Rocks);
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
                // regenerate food so it avoids newly placed rocks
                foodManager.GenerateInitial(rockManager.Rocks);
            }

            // clampinf of cell
            var vp = GraphicsDevice.Viewport;
            cell.Position = new Vector2(
                Math.Clamp(cell.Position.X, 0, vp.Width - cell.Bounds.Width),
                Math.Clamp(cell.Position.Y, 0, vp.Height - cell.Bounds.Height)
            );

            prevKeyboardState = ks;

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
           
            GraphicsDevice.Clear(new Color(2, 13, 58));

            _spriteBatch.Begin();


            rockManager?.Draw(_spriteBatch);

            cell.Draw(_spriteBatch);
            foodManager.Draw(_spriteBatch, pixel);
            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
