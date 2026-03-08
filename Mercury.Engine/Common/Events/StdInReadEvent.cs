namespace Mercury.Engine.Common.Events;

public readonly struct StdInReadEvent {
    public Memory<char> Buffer { get; init; }
    public Action<int> OnReadComplete { get; init; }
    public char Delimiter { get; init; }
}