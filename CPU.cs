using System;
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

		private readonly Random _random = new Random();
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

		public bool shouldUpdateGraphics{get; private set;}

		public CPU()
		{
			Reset();
		}

		public void Reset()
		{
			memory = new byte[0x1000];
			registers = new byte[16];
			stack = new Stack<ushort>();
			indexRegister = 0;
			programCounter = 0x200;
			delayTimer = 0;
			soundTimer = 0;
			graphicsDisplay = new bool[64, 32];
			keys = new bool[16];
			shouldUpdateGraphics = false;
		}

		public void LoadGame(string name)
		{

		}

		public void AdvanceOneCycle()
		{
			// fetch opcode
			var opcode = (ushort)ReadOpCode();
			
			// decode opcode
			// execute opcode

			// update timers
			if (delayTimer > 0)
				delayTimer--;

			if (soundTimer > 0)
				soundTimer--;
		}

		public bool[,] GetDisplay()
		{
			return graphicsDisplay;
		}

		// return true if a pixel changed.
		public bool DrawLineOnDisplay(int x, int y, byte value)
		{
			bool changed = false;
			for (int i = 0; i < 8; i++)
			{
				bool currentPixel = (value & 0x01) != 0;
				if (currentPixel != graphicsDisplay[x, y + 7 - i])
				{
					changed = true;
					graphicsDisplay[x, y + 7 - i] = currentPixel;
				}

				value = (byte)(value >> 1);
			}

			return changed;
		}

		public void UpdateInput(bool[] inputValues)
		{
			keys = inputValues;
		}

		private uint ReadOpCode()
		{
			return (uint)memory[programCounter] << 8 | (uint)memory[programCounter + 1];
		}

		private int GetHexDigitFromUnsignedShort(ushort input, int digit)
		{
			if (digit > 0 && digit <= 4)
			{
				//1  2 3 4
				//12 8 4 0
				return (input >> (4 - input)) & 0x000F;
			}
			else
			{
				throw new Exception("bad digit!");
			}
		}
		private void RunOpcode(ushort opcode)
		{
			switch(opcode & 0xF000)
			{
				case 0x0000:
				{
					if ((opcode & 0x0F00) == 0)
					{
						if (opcode == 0x00E0)
						{
							// 00E0: clear the screen
							for (int y = 0; y < graphicsDisplay.GetLength(0); y++)
							{
								for (int x = 0; x < graphicsDisplay.GetLength(1); x++)
								{
									graphicsDisplay[x,y] = false;
								}
							}

							programCounter += 2;
						}
						else if (opcode == 0x00EE)
						{
							// 00EE: return from a subroutine
							programCounter = stack.Pop();
							programCounter += 2;
						}

					}
					else
					{
						// 0NNN: Calls machine code routine (RCA 1802 for COSMAC VIP) at address NNN. Not necessary for most ROMs.
					}
					
					break;
				}

				case 0x1000:
				{
					// 1NNN: Jump to address NNN. (no stack)
					programCounter = (ushort)(opcode & 0x0FFF);
					break;
				}

				case 0x2000:
				{
					// 2NNN: call subroutine at NNN (puts it on the stack)
					stack.Push(programCounter);
					programCounter = (ushort)(opcode & 0x0FFF);
					break;
				}

				case 0x3000:
				{
					// 3XNN: skip the next instruction if register X equals NN.
					int registerValue = GetHexDigitFromUnsignedShort(opcode, 2);
					if (registers[registerValue] == (opcode & 0x00FF))
					{
						programCounter += 4;
					}
					else
					{
						programCounter += 2;
					}

					break;
				}

				case 0x4000:
				{
					// 4XNN: skip the next instruction if register X does NOT equal NN.
					int registerValue = GetHexDigitFromUnsignedShort(opcode, 2);
					if (registers[registerValue] != (opcode & 0x00FF))
					{
						programCounter += 4;
					}
					else
					{
						programCounter += 2;
					}
					
					break;
				}

				case 0x5000:
				{
					// 5XY0: skip the next instruction if register X equals register Y.
					
					int registerOneValue = GetHexDigitFromUnsignedShort(opcode, 2);
					int registerTwoValue = GetHexDigitFromUnsignedShort(opcode, 3);

					if (registers[registerOneValue] == registers[registerTwoValue])
					{
						programCounter += 4;
					}
					else
					{
						programCounter += 2;
					}
					break;
				}

				case 0x6000:
				{
					// 6XNN: sets register X to NN.
					int registerValue = GetHexDigitFromUnsignedShort(opcode, 2);
					registers[registerValue] = (byte)(opcode & 0x00FF);
					programCounter += 2;
					break;
				}

				case 0x7000:
				{
					// 7XNN: Add NN to register X.
					int registerValue = GetHexDigitFromUnsignedShort(opcode, 2);
					registers[registerValue] += (byte)(opcode & 0x00FF);
					programCounter += 2;
					break;
				}

				case 0x8000:
				{
					int registerX = GetHexDigitFromUnsignedShort(opcode, 2);
					int registerY = GetHexDigitFromUnsignedShort(opcode, 3);

					// 0x8XYZ: does operation based on Z between registers X and Y.
					switch (opcode & 0x000F)
					{
						case 0:
						registers[registerX] = registers[registerY];
						break;

						case 1:
						registers[registerX] |= registers[registerY];
						break;

						case 2:
						registers[registerX] &= registers[registerY];
						break;

						case 3:
						registers[registerX] ^= registers[registerY];
						break;

						case 4:
						{
							// VX + VY, if overflow, then set VF to 1
							byte prevValue = registers[registerX];
							registers[registerX] += registers[registerY];
							registers[0xF] = (prevValue > registers[registerX]) ? (byte)1 : (byte)0;
							break;
						}

						case 5:
						{
							// VX - VY. if borrow, set VF to 0.
							byte prevValue = registers[registerX];
							registers[registerX] -= registers[registerY];
							registers[0xF] = (prevValue < registers[registerX]) ? (byte)0 : (byte)1;
							break;
						}

						case 6:
						{
							// VX >> 1. Store shifted bit in VF.

							registers[0xF] = (byte)(opcode & (ushort)1);
							registers[registerX] = (byte)(registers[registerX] >> 1);
							break;
						}

						case 7:
						{
							// VX = VY - VX
							byte prevValue = registers[registerY];
							registers[registerX] = (byte)(registers[registerY] - registers[registerX]);
							registers[0xF] = (prevValue < registers[registerY]) ? (byte)0 : (byte)1;
							break;
						}
						case 0xE:
						{
							registers[0xF] = (byte)((opcode & 0x8000) >> 15);
							registers[registerX] = (byte)(registers[registerX] << 1);
							break;
						}

						default:
							throw new Exception("invalid register arithmetic operation");
					}

					programCounter += 2;
					break;
				}

				case 0x9000:
				{
					// 9XY0: Skips the next instruction if VX != VY
					
					int registerX = GetHexDigitFromUnsignedShort(opcode, 2);
					int registerY = GetHexDigitFromUnsignedShort(opcode, 3);

					if (registers[registerX] != registers[registerY])
					{
						programCounter += 4;
					}
					else
					{
						programCounter += 2;
					}

					break;
				}

				case 0xA000:
				{
					//ANNN: Sets index to address NNN
					indexRegister = (ushort)(opcode & 0xFFF);
					programCounter += 2;
					break;
				}

				case 0xB000:
				{
					//BNNN: jump to address NNN + V0
					programCounter = (ushort)(opcode & 0xFFF + registers[0]);
					break;
				}

				case 0xC000:
				{
					// CXNN: VX = bitwise AND between a random number between 0 and 255 and N.
					// TODO: look up actual spec for random numbers

					int registerX = GetHexDigitFromUnsignedShort(opcode, 2);

					registers[registerX] = (byte)(_random.Next(0, 255) & (opcode & 0x00FF));
					programCounter += 2;
					break;
				}

				case 0xD000:
				{
					// DXYN: draw a sprite at coordinate (VX, VY) with a width 0f 8 and a height of N+1 pixels. Start at index I for reading bits.
					// Set VF to 1 if any pixels changed value.

					bool changed = false;
					int registerX = GetHexDigitFromUnsignedShort(opcode, 2);
					int registerY = GetHexDigitFromUnsignedShort(opcode, 3);

					int startX = registers[registerX];
					int startY = registers[registerY];

					int height = opcode & 0x000F;

					for (int i = 0; i < height; i++)
					{
						changed = changed || DrawLineOnDisplay(startX, startY, memory[indexRegister + height]);
					}

					if (changed)
					{
						registers[0xF] = 1;
					}

					programCounter += 2;
					break;
				}

				case 0xE000:
				{
					int registerX = GetHexDigitFromUnsignedShort(opcode, 2);

					switch (opcode & 0x00FF)
					{
						case 0x009E:
						{
							// EX9E: skip the next instruction if key[VX] is pressed
							if (keys[registers[registerX]])
							{
								programCounter += 4;
							}
							else
							{
								programCounter += 2;
							}
							break;
						}
						
						case 0x00A1:
						{
							// EXA1: skip the next instruction if key[vX] is NOT pressed
							if (!keys[registers[registerX]])
							{
								programCounter += 4;
							}
							else
							{
								programCounter += 2;
							}
							break;
						}

						default:
							throw new Exception("invalid key opcode");
					}
					break;
				}

				case 0xF000:
				{
					int registerX = GetHexDigitFromUnsignedShort(opcode, 2);
					
					switch(opcode & 0x00FF)
					{
						case 0x0007:
						{
							// FX07: set Vx to the delay timer
							registers[registerX] = delayTimer;
							break;
						}

						case 0x000A:
						{
							// FX0A: block input until a key is pressed, then store the key in Vx

							break;
						}

						case 0x0015:
						{
							// FX15: set the delay timer to Vx
							delayTimer = registers[registerX];
							break;
						}

						case 0x0018:
						{
							// FX18: set the sound timer to Vx
							soundTimer = registers[registerX];
							break;
						}

						case 0x001E:
						{
							// FX1E: add Vx to I (no carry)
							indexRegister += registers[registerX];
							break;
						}

						case 0x0029:
						{
							// FX29: set set I to the location of the sprite for the character in VX
							break;
						}

						case 0x0033:
						{
							// FX33: store the decimal representation of VX in the next three bytes of memory, starting at I
							byte currentValue = registers[registerX];
							
							memory[indexRegister + 2] = (byte)(currentValue % 10);

							currentValue /= 10;

							memory[indexRegister + 1] = (byte)(currentValue % 10);

							currentValue /= 10;

							memory[indexRegister] = currentValue;
							break;
						}

						case 0x0055:
						{
							// FX55: store the 16 registers in memory starting at I (don't change I)
							for (int i = 0; i < registers.Length; i++)
							{
								memory[indexRegister + i] = registers[i];
							}
							break;
						}

						case 0x0065:
						{
							// FX65: recall the 16 values starting at I into the registers (don't change I)
							for (int i = 0; i < registers.Length; i++)
							{
								registers[i] = [indexRegister + i];
							}
							break;
						}
					}

					programCounter += 2;
					break;
				}
			}
		}
	}
}