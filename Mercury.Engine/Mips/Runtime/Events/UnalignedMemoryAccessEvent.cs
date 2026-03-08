namespace Mercury.Engine.Mips.Runtime.Events;

public readonly struct UnalignedMemoryAccessEvent {
    public ulong InstructionAddress { get; init; }
    public ulong InstructionWord { get; init; }
    public ulong MemoryAddress { get; init; }
    public ulong AccessSize { get; init; }
}