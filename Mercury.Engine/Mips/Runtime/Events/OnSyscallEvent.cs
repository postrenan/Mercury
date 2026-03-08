namespace Mercury.Engine.Mips.Runtime.Events;

public readonly struct OnSyscallEvent {
    public uint Instruction { get; init; }
    public uint V0 { get; init; }
    public uint A0 { get; init; }
    public uint A1 { get; init; }
    public uint A2 { get; init; }
    public uint A3 { get; init; }
    public uint F12 { get; init; }
    public uint F13 { get; init; }
    
    public Action<int> RespondV0 { get; init; }
    public Action<int> RespondF0 { get; init; }
    public Action<int> RespondF1 { get; init; }
    public Action<int> RespondA0 { get; init; }
    public Action<int> RespondA1 { get; init; }
}