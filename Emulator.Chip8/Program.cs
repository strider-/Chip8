using System;
using System.IO;

namespace Emulator.Chip8
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetWindowSize(Chip8.ScreenWidth + 1, Chip8.ScreenHeight + 1);
            Console.SetBufferSize(Chip8.ScreenWidth + 1, Chip8.ScreenHeight + 1);

            var cpu = new Chip8();
            var program = File.ReadAllBytes(@"..\..\..\..\Programs\BREAKOUT");

            cpu.LoadProgram(program);            
            
            while (true)
            {
                cpu.Cycle();
                Draw(cpu);
            }
        }

        static void Draw(Chip8 cpu)
        {
            if (cpu.ShouldRedraw)
            {
                for (int x = 0; x < Chip8.ScreenWidth; x++)
                {
                    for (int y = 0; y < Chip8.ScreenHeight; y++)
                    {
                        Console.SetCursorPosition(x, y);
                        var pixel = cpu.DisplayBuffer[x, y] ? "█" : " ";
                        Console.Write(pixel);
                    }
                }
            }
        }
    }
}
