namespace Emulator.Chip8
{
    public delegate void DisassembleLogger(ushort address, string mnemonic, params string[] parameters);

    public class Disassembler
    {
        public void Disassemble(byte[] program, DisassembleLogger log)
        {
            for (ushort pc = 0; pc + 1 < program.Length; pc += 2)
            {
                var raw = (ushort)(program[pc] << 8 | program[pc + 1]);
                var instr = new Instruction(raw);
                switch (instr.OpCode)
                {
                    case 0x0:
                        if (instr.NN == 0xE0)
                        {
                            log(pc, "CLS", "");
                        }
                        else if (instr.NN == 0xEE)
                        {
                            log(pc, "RET", "");
                        }
                        else
                        {
                            log(pc, "SYS", $"${instr.NNN:X3}");
                        }
                        break;
                    case 0x1:
                        log(pc, "JMP", $"${instr.NNN:X3}");
                        break;
                    case 0x2:
                        log(pc, "CALL", $"${instr.NNN:X3}");
                        break;
                    case 0x3:
                        log(pc, "SE", $"V{instr.X}", $"${instr.NN:X2}");
                        break;
                    case 0x4:
                        log(pc, "SNE", $"V{instr.X}", $"${instr.NN:X2}");
                        break;
                    case 0x5:
                        log(pc, "SE", $"V{instr.X}", $"V{instr.Y}");
                        break;
                    case 0x6:
                        log(pc, "LD", $"V{instr.X}", $"${instr.NN:X2}");
                        break;
                    case 0x7:
                        log(pc, "ADD", $"V{instr.X}", $"${instr.NN:X2}");
                        break;
                    case 0x8:
                        switch (instr.N)
                        {
                            case 0x0: log(pc, "LD", $"V{instr.X}", $"V{instr.Y}"); break;
                            case 0x1: log(pc, "OR", $"V{instr.X}", $"V{instr.Y}"); break;
                            case 0x2: log(pc, "AND", $"V{instr.X}", $"V{instr.Y}"); break;
                            case 0x3: log(pc, "XOR", $"V{instr.X}", $"V{instr.Y}"); break;
                            case 0x4: log(pc, "ADD", $"V{instr.X}", $"V{instr.Y}"); break;
                            case 0x5: log(pc, "SUB", $"V{instr.X}", $"V{instr.Y}"); break;
                            case 0x6: log(pc, "SHR", $"V{instr.X}"); break;
                            case 0x7: log(pc, "SUBN", $"V{instr.X}", $"V{instr.Y}"); break;
                            case 0xE: log(pc, "SHL", $"V{instr.X}"); break;
                        }
                        break;
                    case 0x9:
                        log(pc, "SNE", $"V{instr.X}", $"V{instr.Y}");
                        break;
                    case 0xA:
                        log(pc, "LD", $"I", $"${instr.NNN:X3}");
                        break;
                    case 0xB:
                        log(pc, "JMP", $"V0", $"${instr.NNN:X3}");
                        break;
                    case 0xC:
                        log(pc, "RND", $"V{instr.X}", $"${instr.NN:X2}");
                        break;
                    case 0xD:
                        log(pc, "DRAW", $"V{instr.X}", $"V{instr.Y}", $"{instr.N}");
                        break;
                    case 0xE:
                        if (instr.NN == 0x9E)
                        {
                            log(pc, "SKP", $"V{instr.X}");
                        }
                        else if (instr.NN == 0xA1)
                        {
                            log(pc, "SKNP", $"V{instr.X}");
                        }
                        else
                        {
                            log(pc, "UNK", $"${instr.Value:X4}");
                        }
                        break;
                    case 0xF:
                        switch (instr.NN)
                        {
                            case 0x07: log(pc, "LD", $"V{instr.X}", "DT"); break;
                            case 0x0A: log(pc, "LD", $"V{instr.X}", "K"); break;
                            case 0x15: log(pc, "LD", "DT", $"V{instr.X}"); break;
                            case 0x18: log(pc, "LD", "ST", $"V{instr.X}"); break;
                            case 0x1E: log(pc, "ADD", "I", $"V{instr.X}"); break;
                            case 0x29: log(pc, "LD", "F", $"V{instr.X}"); break;
                            case 0x33: log(pc, "LD", "B", $"V{instr.X}"); break;
                            case 0x55: log(pc, "LD", "[I]", $"V{instr.X}"); break;
                            case 0x65: log(pc, "LD", $"V{instr.X}", "[I]"); break;
                            default: log(pc, "UNK", $"${instr.Value:X4}"); break;
                        }
                        break;
                }
            }
        }
    }
}