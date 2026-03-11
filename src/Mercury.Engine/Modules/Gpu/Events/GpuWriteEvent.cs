using Mercury.Engine.Common.Events;

namespace Mercury.Engine.Modules.Gpu.Events;

public readonly struct GpuWriteEvent : IMemoryWriteEvent{
    public ulong Address { get; init; }
    public ulong Size { get; init; }
    public ReadOnlyMemory<byte> Buffer { get; init; }
}