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
			_gameCPU.LoadGame("testroms/test_opcode.ch8");
			
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

			// get and update input
			UpdateKeyboardState(_gameCPU.keys);

			int instructionsPerTick = 9;
			// this defaults to 60fps though we should probably find a way to force this
			_gameCPU.AdvanceOneCycle(instructionsPerTick);


			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			bool[,] screen = _gameCPU.GetDisplay();

			//if (_gameCPU.shouldUpdateGraphics || currentDisplay == null)
			{
				currentDisplay = CreateTexture(_graphics.GraphicsDevice, screen, gameTime.IsRunningSlowly);
			}

			float minScale = Math.Min((float)_graphics.PreferredBackBufferWidth / screen.GetLength(0), (float)_graphics.PreferredBackBufferHeight / screen.GetLength(1));
			_spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
			_spriteBatch.Draw(currentDisplay, Vector2.Zero, new Rectangle(0, 0, screen.GetLength(0), screen.GetLength(1)), Color.White, 0f, Vector2.Zero, minScale, SpriteEffects.None, 0f);
			_spriteBatch.End();

			base.Draw(gameTime);
		}

		private static Texture2D CreateTexture(GraphicsDevice device, bool[,] pixelData, bool isSlow)
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
					Color onColor = isSlow ? new Color(255, 0, 0) : new Color(255,255,255);
					data[counter++] = isPixelOn ? onColor : new Color(0, 0, 0);
				}
			}
			
			texture.SetData(data);

			return texture;
		}

		private void UpdateKeyboardState(bool[] keys)
		{
			KeyboardState state = Keyboard.GetState();

			keys[0x1] = state.IsKeyDown(Keys.D1);
			keys[0x2] = state.IsKeyDown(Keys.D2);
			keys[0x3] = state.IsKeyDown(Keys.D3);
			keys[0xC] = state.IsKeyDown(Keys.D4);
			keys[0x4] = state.IsKeyDown(Keys.Q);
			keys[0x5] = state.IsKeyDown(Keys.W);
			keys[0x6] = state.IsKeyDown(Keys.E);
			keys[0xD] = state.IsKeyDown(Keys.R);
			keys[0x7] = state.IsKeyDown(Keys.A);
			keys[0x8] = state.IsKeyDown(Keys.S);
			keys[0x9] = state.IsKeyDown(Keys.D);
			keys[0xE] = state.IsKeyDown(Keys.F);
			keys[0xA] = state.IsKeyDown(Keys.Z);
			keys[0x0] = state.IsKeyDown(Keys.X);
			keys[0xB] = state.IsKeyDown(Keys.C);
			keys[0xF] = state.IsKeyDown(Keys.V);
		}
	}
}
