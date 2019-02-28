using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Emulator.Chip8.Display
{
    public partial class Form1 : Form
    {
        private const int CpuHz = 500;

        private readonly Chip8 _vm;
        private readonly Bitmap _display;
        private readonly Rectangle _bitmapRect;
        private readonly Dictionary<Keys, byte> _inputMap;

        private Color _pixelColor = Color.FromArgb(0xFF, 0xCA, 0xE6, 0x82);
        private Color _backgroundColor = Color.FromArgb(0xFF, 0x40, 0x40, 0x40);
        private byte[] _pixelArgb, _backgroundArgb;

        private Disassembler _da = new Disassembler();
        private string _gamePath = @"..\..\..\Programs\BREAKOUT";

        public Form1()
        {
            InitializeComponent();

            _pixelArgb = BitConverter.GetBytes(_pixelColor.ToArgb());
            _backgroundArgb = BitConverter.GetBytes(_backgroundColor.ToArgb());

            _display = new Bitmap(Chip8.ScreenWidth, Chip8.ScreenHeight);
            _bitmapRect = new Rectangle(0, 0, _display.Width, _display.Height);
            output.Image = _display;

            _inputMap = new Dictionary<Keys, byte>()
            {
                {Keys.Up, 0x2},
                {Keys.Left, 0x4},
                {Keys.Right, 0x6},
                {Keys.Down, 0x8}
            };

            KeyDown += SetKeyDown;
            KeyUp += SetKeyUp;

            _vm = new Chip8();
            ResetGame(null, EventArgs.Empty);
        }

        protected override void OnLoad(EventArgs e) => Task.Run(Loop);

        private Task Loop()
        {
            var stopwatch = Stopwatch.StartNew();
            var target = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / CpuHz);
            var last = new TimeSpan();

            while (true)
            {
                var elapsed = stopwatch.Elapsed - last;

                if (elapsed >= target)
                {
                    _vm.Cycle();
                    UpdateDebugUI();
                    Draw();
                    last += target;
                }
            }
        }

        private void UpdateDebugUI()
        {
            try
            {
                Invoke(HighlightAddress);
                Invoke(UpdateCpuInfo);
            }
            catch (ObjectDisposedException)
            {
                //whatever
            }
        }

        private void Draw()
        {
            if (_vm.ShouldRedraw)
            {
                var data = _display.LockBits(_bitmapRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                var ptr = data.Scan0;

                for (int y = 0; y < _display.Height; y++)
                {
                    for (int x = 0; x < _display.Width; x++)
                    {
                        var argb = _vm.DisplayBuffer[x, y] ? _pixelArgb : _backgroundArgb;
                        Marshal.Copy(argb, 0, ptr, 4);
                        ptr += 4; // ptr = IntPtr.Add(ptr, 4);
                    }
                }

                _display.UnlockBits(data);
                output.Invoke(RefreshOutput);
            }
        }

        private void SetKeyUp(object sender, KeyEventArgs e)
        {
            if (_inputMap.ContainsKey(e.KeyCode))
            {
                _vm.KeyUp(_inputMap[e.KeyCode]);
            }
        }

        private void SetKeyDown(object sender, KeyEventArgs e)
        {
            if (_inputMap.ContainsKey(e.KeyCode))
            {
                _vm.KeyDown(_inputMap[e.KeyCode]);
            }
        }

        private void AppendToDisassemblerList(ushort address, string mnemonic, params string[] parameters)
        {
            addressList.Items.Add($"{address + 0x200:X4} - {mnemonic,-5} {string.Join(", ", parameters)}");
        }

        private MethodInvoker RefreshOutput => () => output.Refresh();

        private MethodInvoker HighlightAddress => () => addressList.SelectedIndex = (_vm.ProgramCounter - 0x200) / 2;

        private void ResetGame(object sender, EventArgs e)
        {
            var program = File.ReadAllBytes(_gamePath);
            _vm.LoadProgram(program);
            addressList.Items.Clear();
            _da.Disassemble(program, AppendToDisassemblerList);
        }

        private MethodInvoker UpdateCpuInfo => () =>
        {
            var strings = _vm.V.Select((v, i) => $"V{i:X1} = ${v:X2}");
            var lineOne = $"{string.Join("  ", strings.Take(8))}";
            var lineTwo = $"{string.Join("  ", strings.Skip(8).Take(8))}";
            var lineThree = $"I = ${_vm.I:X3}  DT = {_vm.DelayTimer,-3}  ST = {_vm.SoundTimer}";
            vRegisters.Text = $"{lineOne}\n{lineTwo}\n{lineThree}";
        };
    }
}