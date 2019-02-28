using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// http://www.multigesture.net/articles/how-to-write-an-emulator-chip-8-interpreter/
// http://devernay.free.fr/hacks/chip8/C8TECH10.HTM

namespace Emulator.Chip8
{
    /// <summary>
    /// The Chip-8 virtual machine
    /// </summary>
    public class Chip8
    {
        public const int ScreenWidth = 64,
                         ScreenHeight = 32;

        private const int ProgramStart = 0x200, // RAM location of first instruction for Chip-8 programs
                          FontOffset = 0x50;    // Offset where fonts are loaded in interpreter area of RAM

        private byte[] _ram = new byte[0x1000], // 4K RAM
                       _v = new byte[16];       // V registers
        private ushort _i = 0,                  // I register
                       _pc = 0;                 // Program counter
        private byte _delayTimer = 0,           // Delay timer value
                     _soundTimer = 0;           // Sound timer value
        private Stack<ushort> _stack = new Stack<ushort>(16);                  // subroutine stack
        private HashSet<byte> _keys = new HashSet<byte>();                     // keyboard state
        private Dictionary<byte, Action<Instruction>> _codeTable, _miscTable;  // look-up of OpCode to method
        private Random _rng = new Random();                                    // for CXNN instruction
        private TimeSpan _sixtyHz = TimeSpan.FromSeconds(1f / 60f);            // 60Hz frequency as a TimeSpan
        private Timer _sixtyHzTimer;                                           // decrements timers every 60Hz

        /// <summary>
        /// Creates a new instance of the Chip-8 VM
        /// </summary>
        public Chip8()
        {
            InitializeOpCodeTables();
            LoadFont();

            _sixtyHzTimer = new Timer(DecrementTimers);
            DisplayBuffer = new bool[ScreenWidth, ScreenHeight];
        }

        /// <summary>
        /// Loads a Chip-8 program in to RAM and starts the internal 60Hz timer
        /// </summary>
        /// <param name="program"></param>
        public void LoadProgram(byte[] program)
        {
            _pc = ProgramStart;
            Array.Clear(_ram, ProgramStart, _ram.Length - ProgramStart);
            Array.Clear(DisplayBuffer, 0, DisplayBuffer.Length);
            Array.Copy(program, 0, _ram, ProgramStart, Math.Min(program.Length, 0xE00));
            StartTimer();
        }

        /// <summary>
        /// Performs a CPU cycle, reading and executing a single instruction.
        /// </summary>
        public void Cycle()
        {
            var code = (ushort)(_ram[_pc++] << 8 | _ram[_pc++]);
            var instr = new Instruction(code);

            ShouldRedraw = false;
            _codeTable[instr.OpCode](instr);
        }

        /// <summary>
        /// Sets a key as not pressed
        /// </summary>
        /// <param name="key">0x0 - 0xF</param>
        public void KeyDown(byte key)
        {
            _keys.Add(key);
        }

        /// <summary>
        /// Sets a key as being pressed
        /// </summary>
        /// <param name="key">0x0 - 0xF</param>
        public void KeyUp(byte key)
        {
            _keys.Remove(key);
        }

        /// <summary>
        /// Stops the internal 60Hz timer
        /// </summary>
        public void StopTimer()
        {
            _sixtyHzTimer.Change(TimeSpan.FromMilliseconds(-1), _sixtyHz);
        }

        /// <summary>
        /// Starts the internal 60Hz timer. This method is called when a program is loaded.
        /// </summary>
        public void StartTimer()
        {
            _sixtyHzTimer.Change(TimeSpan.FromSeconds(0), _sixtyHz);
        }

        private void DecrementTimers(object state)
        {
            if (_delayTimer > 0)
            {
                _delayTimer--;
            }

            if (_soundTimer > 0)
            {
                _soundTimer--;
            }
        }

        private void ClearDisplayOrReturn(Instruction instr)
        {
            switch (instr.NN)
            {
                case 0xE0: // Clear Display
                    Array.Clear(DisplayBuffer, 0, DisplayBuffer.Length);
                    break;
                case 0xEE: // Return from subroutine
                    _pc = _stack.Pop();
                    break;
            }
        }

        private void Jump(Instruction instr)
        {
            _pc = instr.NNN;
        }

        private void Call(Instruction instr)
        {
            _stack.Push(_pc);
            _pc = instr.NNN;
        }

        private void SkipIfVXEqual(Instruction instr)
        {
            if (_v[instr.X] == instr.NN)
            {
                SkipInstruction();
            }
        }

        private void SkipIfVXNotEqual(Instruction instr)
        {
            if (_v[instr.X] != instr.NN)
            {
                SkipInstruction();
            }
        }

        private void SkipIfVXEqualVY(Instruction instr)
        {
            if (_v[instr.X] == _v[instr.Y])
            {
                SkipInstruction();
            }
        }

        private void SetVX(Instruction instr)
        {
            _v[instr.X] = instr.NN;
        }

        private void Add(Instruction instr)
        {
            _v[instr.X] += instr.NN;
        }

        private void Operation(Instruction instr)
        {
            switch (instr.N)
            {
                case 0x0: // Assign Vx to Vy
                    _v[instr.X] = _v[instr.Y];
                    break;
                case 0x1: // Assign Vx to Vx OR Vy
                    _v[instr.X] |= _v[instr.Y];
                    break;
                case 0x2: // Assign Vx to Vx AND Vy
                    _v[instr.X] &= _v[instr.Y];
                    break;
                case 0x3: // Assign Vx to Vx XOR Vy
                    _v[instr.X] ^= _v[instr.Y];
                    break;
                case 0x4: // Assign Vx to Vx + Vy, setting the VF flag on an overflow
                    SetFlag(_v[instr.X] + _v[instr.Y] > 0xFF);
                    _v[instr.X] += _v[instr.Y];
                    break;
                case 0x5: // Assign Vx to Vx - Vy, setting the VF flag when there's a borrow
                    SetFlag(_v[instr.X] > _v[instr.Y]);
                    _v[instr.X] -= _v[instr.Y];
                    break;
                case 0x6: // Assign Vx to Vx >> 1, setting the VF flag to the bit getting shifted off
                    SetFlag((_v[instr.X] & 0x1) == 1);
                    _v[instr.X] >>= 1;
                    break;
                case 0x7: // Assign Vx to Vy - Vx, setting the VF flag when there's a borrow
                    SetFlag(_v[instr.Y] > _v[instr.X]);
                    _v[instr.X] = (byte)(_v[instr.Y] - _v[instr.X]);
                    break;
                case 0xE: // Assign Vx to Vx << 1, setting the VF flag to the bit getting shifted off
                    SetFlag((_v[instr.X] & 0xF) == 0xF);
                    _v[instr.X] <<= 1;
                    break;
            }
        }

        private void SkipIfVXNotEqualVY(Instruction instr)
        {
            if (_v[instr.X] != _v[instr.Y])
            {
                SkipInstruction();
            }
        }

        private void SetI(Instruction instr)
        {
            _i = instr.NNN;
        }

        private void JumpWithOffset(Instruction instr)
        {
            _pc = (ushort)(instr.NNN + _v[0]);
        }

        private void Randomize(Instruction instr)
        {
            _v[instr.X] = (byte)(_rng.Next(0, 256) & instr.NN);
        }

        private void Draw(Instruction instr)
        {
            var startX = _v[instr.X];
            var startY = _v[instr.Y];

            SetFlag(false);

            for (int i = 0; i < instr.N; i++)
            {
                var line = _ram[_i + i];

                for (var bit = 0; bit < 8; bit++)
                {
                    var x = (startX + bit) % ScreenWidth;
                    var y = (startY + i) % ScreenHeight;

                    var spriteBit = ((line >> (7 - bit)) & 1);
                    var oldBit = DisplayBuffer[x, y] ? 1 : 0;

                    if (oldBit != spriteBit)
                    {
                        ShouldRedraw = true;
                    }

                    var newBit = oldBit ^ spriteBit;

                    DisplayBuffer[x, y] = newBit != 0;

                    if (oldBit != 0 && newBit == 0)
                    {
                        SetFlag(true);
                    }
                }
            }
        }

        private void SkipOnKey(Instruction instr)
        {
            var shouldSkip = (instr.NN == 0x9E && _keys.Contains(_v[instr.X])) || // Key Pressed
                             (instr.NN == 0xA1 && !_keys.Contains(_v[instr.X]));  // Key Not Pressed

            if (shouldSkip)
            {
                SkipInstruction();
            }
        }

        private void SetVXToDelay(Instruction instr)
        {
            _v[instr.X] = _delayTimer;
        }

        private void WaitForKey(Instruction instr)
        {
            if (_keys.Any())
            {
                _v[instr.X] = _keys.First();
            }
            else
            {
                _pc -= 2; // Re-read the current instruction until there's a key press
            }
        }

        private void SetDelayTimer(Instruction instr)
        {
            _delayTimer = _v[instr.X];
        }

        private void SetSoundTimer(Instruction instr)
        {
            _soundTimer = _v[instr.X];
        }

        private void AddVXToI(Instruction instr)
        {
            _i += _v[instr.X];
        }

        private void SetIToSprite(Instruction instr)
        {
            _i = (ushort)((_v[instr.X] * 5) + FontOffset);
        }

        private void BinaryCodedDecimal(Instruction instr)
        {
            _ram[_i] = (byte)((_v[instr.X] / 100) % 10);
            _ram[_i + 1] = (byte)((_v[instr.X] / 10) % 10);
            _ram[_i + 2] = (byte)(_v[instr.X] % 10);
        }

        private void DumpVX(Instruction instr)
        {
            for (int i = 0; i <= instr.X; i++)
            {
                _ram[_i + i] = _v[i];
            }
        }

        private void LoadVX(Instruction instr)
        {
            for (int i = 0; i <= instr.X; i++)
            {
                _v[i] = _ram[_i + i];
            }
        }

        private void SetFlag(bool condition)
        {
            _v[0xF] = (byte)(condition ? 1 : 0);
        }

        private void SkipInstruction()
        {
            _pc += 2;
        }

        private void InitializeOpCodeTables()
        {
            _miscTable = new Dictionary<byte, Action<Instruction>>
            {
                {0x07, SetVXToDelay},
                {0x0A, WaitForKey},
                {0x15, SetDelayTimer},
                {0x18, SetSoundTimer},
                {0x1E, AddVXToI},
                {0x29, SetIToSprite},
                {0x33, BinaryCodedDecimal},
                {0x55, DumpVX},
                {0x65, LoadVX}
            };

            _codeTable = new Dictionary<byte, Action<Instruction>>
            {
                {0x0, ClearDisplayOrReturn},
                {0x1, Jump},
                {0x2, Call},
                {0x3, SkipIfVXEqual},
                {0x4, SkipIfVXNotEqual},
                {0x5, SkipIfVXEqualVY},
                {0x6, SetVX},
                {0x7, Add},
                {0x8, Operation},
                {0x9, SkipIfVXNotEqualVY},
                {0xA, SetI},
                {0xB, JumpWithOffset},
                {0xC, Randomize},
                {0xD, Draw},
                {0xE, SkipOnKey},
                {0xF, (code) => _miscTable[code.NN](code)},
            };
        }

        private void LoadFont()
        {
            var fontData = new byte[]
            {
                  0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
                  0x20, 0x60, 0x20, 0x20, 0x70, // 1
                  0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
                  0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
                  0x90, 0x90, 0xF0, 0x10, 0x10, // 4
                  0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
                  0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
                  0xF0, 0x10, 0x20, 0x40, 0x40, // 7
                  0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
                  0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
                  0xF0, 0x90, 0xF0, 0x90, 0x90, // A
                  0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
                  0xF0, 0x80, 0x80, 0x80, 0xF0, // C
                  0xE0, 0x90, 0x90, 0x90, 0xE0, // D
                  0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
                  0xF0, 0x80, 0xF0, 0x80, 0x80  // F
            };
            Array.Copy(fontData, 0, _ram, FontOffset, fontData.Length);
        }

        /// <summary>
        /// Gets whether or not the screen should be redrawn
        /// </summary>
        public bool ShouldRedraw { get; private set; }

        /// <summary>
        /// Gets whether or not a beep should be playing
        /// </summary>
        public bool ShouldBeep => _soundTimer > 0;

        /// <summary>
        /// Gets the current state of the display
        /// </summary>
        public bool[,] DisplayBuffer { get; private set; }

        /// <summary>
        /// Gets the current address of the program counter
        /// </summary>
        public ushort ProgramCounter => _pc;

        /// <summary>
        /// Gets the V registers
        /// </summary>
        public byte[] V => _v;

        /// <summary>
        /// Gets the I register
        /// </summary>
        public ushort I => _i;

        /// <summary>
        /// Gets the delay timer value
        /// </summary>
        public int DelayTimer => _delayTimer;

        /// <summary>
        /// Gets the sound timer value.
        /// </summary>
        public int SoundTimer => _soundTimer;
    }
}