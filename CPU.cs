using System.Collections.Generic;

namespace chip8_emu
{
	public class CPU
	{
		// 16 one byte data registers
		// first 15 are data, 16th is "carry flag"
		private byte[] registers;

		private ushort indexRegister;
		private ushort programCounter;

		private Stack<ushort> stack;

		/*
			0x000-0x1FF - Chip 8 interpreter (contains font set in emu)
			0x050-0x0A0 - Used for the built in 4x5 pixel font set (0-F)
			0x200-0xFFF - Program ROM and work RAM
		*/
		private byte[] memory;

		// counts down to 0, once per cycle.
		private byte delayTimer;
		// counts down to 0, once per cycle. if non-zero, play a beep.
		private byte soundTimer;

		// 16 keys, array tells you if it's on or not
		private bool[] keys;

		// one bit pixels. access [x,y]
		private bool[,] graphicsDisplay;

		public CPU()
		{
			memory = new byte[0x1000];
			registers = new byte[16];
			stack = new Stack<ushort>();
			indexRegister = 0;
			programCounter = 0;
			delayTimer = 0;
			soundTimer = 0;
			graphicsDisplay = new bool[64, 32];
			keys = new bool[16];
		}

		public void LoadGame(string name)
		{

		}

		public void AdvanceOneCycle()
		{

		}

		public bool[,] GetDisplay()
		{
			return graphicsDisplay;
		}
	}
}