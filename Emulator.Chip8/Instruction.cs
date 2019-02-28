namespace Emulator.Chip8
{
    struct Instruction
    {
        /// <summary>
        /// The original 16-bit instruction
        /// </summary>
        public readonly ushort Value;
        /// <summary>
        /// First 4 bits of the instruction
        /// </summary>
        public readonly byte OpCode;
        /// <summary>
        /// 12-bit address value used by the 0x0, 0x1, 0x2, 0xA and 0xB OpCodes
        /// </summary>
        public readonly ushort NNN;
        /// <summary>
        /// 8-bit value, used in conjunction with the V register at the index of the X field
        /// </summary>
        public readonly byte NN;
        /// <summary>
        /// 4-bit value, used to indicate a specific operation for the 0x8 OpCode or to determine the height of a sprite for the 0xD OpCode
        /// </summary>
        public readonly byte N;
        /// <summary>
        /// 4-bit V register index
        /// </summary>
        public readonly byte X;
        /// <summary>
        /// 4-bit V register index, used in the 0x5, 0x8 and 0xD OpCodes
        /// </summary>
        public readonly byte Y;

        public Instruction(ushort data)
        {
            Value = data;
            OpCode = (byte)(data >> 12);
            NNN = (ushort)(data & 0x0FFF);
            NN = (byte)(data & 0x00FF);
            N = (byte)(data & 0x000F);
            X = (byte)((data & 0x0F00) >> 8);
            Y = (byte)((data & 0x00F0) >> 4);
        }
    }
}
