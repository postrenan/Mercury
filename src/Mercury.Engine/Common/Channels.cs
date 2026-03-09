using System.Text;
using System.Threading.Channels;

namespace Mercury.Engine.Common;

public class EventStream {

    private readonly Func<ReadOnlySpan<char>, CancellationToken, ValueTask> writeFunc;
    private readonly Func<CancellationToken, ValueTask<char>> readFunc;
    
    public EventStream(Func<CancellationToken,ValueTask<char>>? readFunc, Func<ReadOnlySpan<char>, CancellationToken, ValueTask>? writeFunc) {
        this.writeFunc = writeFunc ?? ((_,_) => ValueTask.CompletedTask);
        this.readFunc = readFunc ?? (_ => ValueTask.FromResult('\0'));
    }

    public ValueTask WriteAsync(string str, CancellationToken cancellationToken = default) {
        return writeFunc(str.AsSpan(), cancellationToken);
    }
    
    public async ValueTask<string> ReadStringAsync(int count, char[] delimeters, CancellationToken cancellationToken = default) {
        StringBuilder sb = new();
        for (int i = 0; i < count || count == -1; i++) {
            char c = await readFunc(cancellationToken);
            if (delimeters.Contains(c)) {
                break;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }
    
    public async ValueTask<char> ReadCharAsync(CancellationToken cancellationToken = default) {
        return await readFunc(cancellationToken);
    }
    
    public ValueTask WriteAsync(char c, CancellationToken cancellationToken = default) {
        Span<char> buf = stackalloc char[1];
        buf[0] = c;
        return writeFunc(buf, cancellationToken);
    }
    
    public async ValueTask Read(Memory<char> buffer, CancellationToken cancellationToken = default) {
        for (int i = 0; i < buffer.Length; i++) {
            char c = await readFunc(cancellationToken);
            buffer.Span[i] = c;
        }
    }
    
    public ValueTask Write(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default) {
        return writeFunc(buffer.Span, cancellationToken);
    }
}

// a principio usado so no MARS
public class ChannelStream : Stream {

    private readonly EventStream stream;

    public ChannelStream(EventStream stream) {
        this.stream = stream;
    }

    public override void Flush() {
        // nothing
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        for (int i = 0; i < count && !cancellationToken.IsCancellationRequested; i++) {
            buffer[offset+i] = (byte)await stream.ReadCharAsync(cancellationToken);
        }
        return count;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new()) {
        int read = 0;
        for (int i = 0; i < buffer.Length && !cancellationToken.IsCancellationRequested; i++) {
            byte b = (byte)await stream.ReadCharAsync(cancellationToken);
            buffer.Span[i] = b;
            read++;
        }
        return read;
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        for (int i = 0; i < count && !cancellationToken.IsCancellationRequested; i++) {
            await stream.WriteAsync((char)buffer[offset + i], cancellationToken);
        }
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new()) {
        for (int i = 0; i < buffer.Length && !cancellationToken.IsCancellationRequested; i++) {
            await stream.WriteAsync((char)buffer.Span[i], cancellationToken);
        }
    }

    public override int Read(byte[] buffer, int offset, int count) {
        throw new NotSupportedException("Use Async Version");
    }

    public override long Seek(long offset, SeekOrigin origin) {
        throw new NotSupportedException();
    }

    public override void SetLength(long value) {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count) {
        throw  new NotSupportedException("Use Async Version");
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => -1;
    public override long Position { get; set; } = -1;
}

/// <summary>
/// A channel that uses a stream as the underlying device.
/// </summary>
public sealed class StreamChannel : Channel<char>, IDisposable {

    public StreamChannel(Stream stream) {
        if (stream.CanWrite) {
            Writer = new StreamCharWriter(stream);
        }
        if (stream.CanRead) {
            Reader = new StreamCharReader(stream);
        }
    }

    private sealed class StreamCharReader(Stream s) : ChannelReader<char>, IDisposable {

        private readonly StreamReader reader = new(s);

        public override bool TryRead(out char item) {
            int result = reader.Read();
            if (result == -1) {
                item = '\0';
                return false;
            }
            item = (char)result;
            return true;
        }

        public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = new CancellationToken()) {
            return new ValueTask<bool>(true);
        }

        public void Dispose() {
            reader.Dispose();
        }
    }

    private sealed class StreamCharWriter(Stream s) : ChannelWriter<char>, IDisposable {

        private readonly StreamWriter writer = new(s);

        public override bool TryWrite(char item) {
            writer.Write(item);
            return true;
        }

        public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = new CancellationToken()) {
            return new ValueTask<bool>(true);
        }
        
        public void Dispose() {
            s.Dispose();
        }
    }

    public void Dispose() {
        if(Reader is IDisposable d1) { d1.Dispose(); }
        if(Writer is IDisposable d2) { d2.Dispose(); }
    }
}