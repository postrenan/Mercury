namespace Mercury.Engine.Common.Events;

public interface IMemoryReadEvent {
    public ulong Address { get; init; }
    public ulong Size { get; init; }
    public Memory<byte> Buffer { get; init; }
}

public readonly struct MemoryReadEvent : IMemoryReadEvent {
    public ulong Address { get; init; }
    public ulong Size { get; init; }
    public Memory<byte> Buffer { get; init; }
}