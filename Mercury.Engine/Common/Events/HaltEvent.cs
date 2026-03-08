namespace Mercury.Engine.Common.Events;

public struct HaltEvent {
    public int ExitCode { get; init; }
    public ulong Address { get; init; }
}