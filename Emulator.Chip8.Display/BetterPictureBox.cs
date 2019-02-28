using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Emulator.Chip8.Display
{
    class BetterPictureBox : PictureBox
    {
        public InterpolationMode InterpolationMode { get; set; }

        public BetterPictureBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer | 
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            pe.Graphics.InterpolationMode = InterpolationMode;
            base.OnPaint(pe);
        }
    }
}