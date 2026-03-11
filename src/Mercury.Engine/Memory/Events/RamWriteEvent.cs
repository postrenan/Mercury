using Mercury.Engine.Common.Events;

namespace Mercury.Engine.Memory.Events;

public readonly struct RamWriteEvent : IMemoryWriteEvent {
    public ulong Address { get; init; }
    public ulong Size { get; init; }
    public ReadOnlyMemory<byte> Buffer { get; init; }
}