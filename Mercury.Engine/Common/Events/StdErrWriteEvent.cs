namespace Mercury.Engine.Common.Events;

public readonly struct StdErrWriteEvent {
    public ReadOnlyMemory<char> Data { get; init; }
}