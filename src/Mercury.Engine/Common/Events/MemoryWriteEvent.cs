namespace Mercury.Engine.Common.Events;

public interface IMemoryWriteEvent {
    public ulong Address { get; init; }
    public ulong Size { get; init; }
    public ReadOnlyMemory<byte> Buffer { get; init; }
}

public readonly struct MemoryWriteEvent : IMemoryWriteEvent {
    public ulong Address { get; init; }
    public ulong Size { get; init; }
    public ReadOnlyMemory<byte> Buffer { get; init; }
}