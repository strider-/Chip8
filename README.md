# Chip8 Emulator
An implementation of the Chip8 virtual machine in C#, written with guideance from articles at http://www.multigesture.net/articles/how-to-write-an-emulator-chip-8-interpreter/ & http://devernay.free.fr/hacks/chip8/C8TECH10.HTM

## Technology Stack
* C# 7 (Visual Studio 2017)
* WinForms (for a cheap GUI implementation)

Always wanted to try my hand at emulation, and the Chip8 VM seemed like a great place to start, given the minimal instruction set. The Emulator.Chip8 project is the core
implementation with a ghetto console UI, and the Emulator.Chip8.Display project is a mildly better GUI implementation using win forms and a custom picture box control.

I'm under the impression that the included Chip8 programs can be freely distributed, but I'll gladly rip them out of the repo if this is not the case.