using System;

namespace chip8_emu
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new ChipEightEmu())
                game.Run();
        }
    }
}
