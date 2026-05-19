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

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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

            // create pixel for drawing level1 food
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            // setup food manager
            foodManager = new FoodManager(GraphicsDevice);
            // configure asset keys for level 2..4 here. Keep them organized so you can change easily.
            string[] foodKeys = new string[5];
            // example: foodKeys[2] = "Food Textures/food_level2";
            // set these to your actual content asset names
            foodKeys[2] = "Foods/XFood2"; // replace with actual asset name
            foodKeys[3] = "Foods/YFood3"; // replace with actual asset name
            foodKeys[4] = "Foods/YFood4"; // replace with actual asset name

            foodManager.LoadTextures((level) => foodKeys[level], Content);
            foodManager.TotalCount = 50; // change overall amount here
            foodManager.GenerateInitial();
        }    
           
            
            // TODO: use this.Content to load your game content here
  

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            cell.Update(gameTime);

            foodManager.Update(gameTime);

            // Prevent the cell from leaving the visible screen by clamping its position
            var vp = GraphicsDevice.Viewport;
            cell.Position = new Vector2(
                Math.Clamp(cell.Position.X, 0, vp.Width - cell.Bounds.Width),
                Math.Clamp(cell.Position.Y, 0, vp.Height - cell.Bounds.Height)
            );

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkBlue);

            _spriteBatch.Begin();
            cell.Draw(_spriteBatch);
            foodManager.Draw(_spriteBatch, pixel);
            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
