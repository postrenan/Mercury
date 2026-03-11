using Mercury.Engine.Common.Events;

namespace Mercury.Engine.Memory.Events;

public readonly struct RamReadEvent : IMemoryReadEvent{
    public ulong Address { get; init; }
    public ulong Size { get; init; }
    public Memory<byte> Buffer { get; init; }
}