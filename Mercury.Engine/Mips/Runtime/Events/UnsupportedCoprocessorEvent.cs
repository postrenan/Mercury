namespace Mercury.Engine.Mips.Runtime.Events;

public readonly struct UnsupportedCoprocessorEvent {
    public ulong Address { get; init; }
    public uint Instruction { get; init; }
    public byte Value { get; init; }
}