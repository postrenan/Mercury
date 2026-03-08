namespace Mercury.Engine.Common.Events;

public readonly struct WriteMemoryEvent {
    public ulong Address { get; init; }
    public ulong Size { get; init; }
    public ReadOnlyMemory<byte> Buffer { get; init; }
}