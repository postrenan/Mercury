namespace Mercury.Engine.Common.Events;

public readonly struct ReadMemoryEvent {
    public ulong Address { get; init; }
    public ulong Size { get; init; }
    public Memory<byte> Buffer { get; init; }
}