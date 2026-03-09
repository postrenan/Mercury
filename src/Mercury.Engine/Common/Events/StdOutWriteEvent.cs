namespace Mercury.Engine.Common.Events;

public readonly struct StdOutWriteEvent {
    public ReadOnlyMemory<char> Data { get; init; }
}