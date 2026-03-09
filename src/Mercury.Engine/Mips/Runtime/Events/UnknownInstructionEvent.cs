namespace Mercury.Engine.Mips.Runtime.Events;

public readonly struct UnknownInstructionEvent {
    public uint InstructionWord { get; init; }
    public ulong Address { get; init; }
}