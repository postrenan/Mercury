namespace Mercury.Engine.Mips.Runtime.Events;

public readonly struct InvalidOperationEvent {
    public ulong Address { get; init; }
    public uint Word { get; init; }
}