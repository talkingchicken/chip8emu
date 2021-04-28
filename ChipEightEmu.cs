using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace chip8_emu
{
	public class ChipEightEmu : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private CPU _gameCPU;

		private Texture2D currentDisplay;

		public ChipEightEmu()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			_gameCPU = new CPU();
			_gameCPU.LoadGame("tetris.c8");
			
			_graphics.PreferredBackBufferWidth = 1280;
			_graphics.PreferredBackBufferHeight = 640;
			_graphics.ApplyChanges();

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			// this defaults to 60fps though we should probably find a way to force this
			_gameCPU.AdvanceOneCycle();

			// get and update input
			_gameCPU.UpdateInput(GetKeyboardState());

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			bool[,] screen = _gameCPU.GetDisplay();
			
			if (_gameCPU.shouldUpdateGraphics || currentDisplay == null)
			{
				currentDisplay = CreateTexture(_graphics.GraphicsDevice, screen);
			}

			float minScale = Math.Min((float)_graphics.PreferredBackBufferWidth / screen.GetLength(0), (float)_graphics.PreferredBackBufferHeight / screen.GetLength(1));
			_spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
			_spriteBatch.Draw(currentDisplay, Vector2.Zero, new Rectangle(0, 0, screen.GetLength(0), screen.GetLength(1)), Color.White, 0f, Vector2.Zero, minScale, SpriteEffects.None, 0f);
			_spriteBatch.End();

			base.Draw(gameTime);
		}

		private static Texture2D CreateTexture(GraphicsDevice device, bool[,] pixelData)
		{
			int width = pixelData.GetLength(0);
			int height = pixelData.GetLength(1);
			Texture2D texture  = new Texture2D(device, width, height);

			Color[] data = new Color[width * height];
			
			int counter = 0;

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					bool isPixelOn = pixelData[x,y];
					data[counter++] = isPixelOn ? new Color(255, 255, 255) : new Color(0, 0, 0);
				}
			}
			
			texture.SetData(data);

			return texture;
		}

		private bool[] GetKeyboardState()
		{
			KeyboardState state = Keyboard.GetState();

			bool[] returnValue = new bool[16];

			if (state.IsKeyDown(Keys.D1))
			{
				returnValue[0] = true;
			}
			
			if (state.IsKeyDown(Keys.D2))
			{
				returnValue[1] = true;
			}
			
			if (state.IsKeyDown(Keys.D3))
			{
				returnValue[2] = true;
			}
			
			if (state.IsKeyDown(Keys.D4))
			{
				returnValue[3] = true;
			}

			if (state.IsKeyDown(Keys.Q))
			{
				returnValue[4] = true;
			}

			if (state.IsKeyDown(Keys.W))
			{
				returnValue[5] = true;
			}
			
			if (state.IsKeyDown(Keys.E))
			{
				returnValue[6] = true;
			}
			
			if (state.IsKeyDown(Keys.R))
			{
				returnValue[7] = true;
			}
			
			if (state.IsKeyDown(Keys.A))
			{
				returnValue[8] = true;
			}
			
			if (state.IsKeyDown(Keys.S))
			{
				returnValue[9] = true;
			}
			
			if (state.IsKeyDown(Keys.D))
			{
				returnValue[10] = true;
			}
			
			if (state.IsKeyDown(Keys.F))
			{
				returnValue[11] = true;
			}
			
			if (state.IsKeyDown(Keys.Z))
			{
				returnValue[12] = true;
			}
			
			if (state.IsKeyDown(Keys.X))
			{
				returnValue[13] = true;
			}
			
			if (state.IsKeyDown(Keys.C))
			{
				returnValue[14] = true;
			}
			
			if (state.IsKeyDown(Keys.V))
			{
				returnValue[15] = true;
			}
			
			return returnValue;
		}
	}
}
