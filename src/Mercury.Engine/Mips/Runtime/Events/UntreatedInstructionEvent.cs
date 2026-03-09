namespace Mercury.Engine.Mips.Runtime.Events;

public readonly struct UntreatedInstructionEvent {
    public string Description { get; init; }
    public uint Word { get; init; }
    public ulong Address { get; init; }
}