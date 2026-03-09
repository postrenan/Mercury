using System.Threading.Channels;
using Mercury.Engine.Common.Events;

namespace Mercury.Engine.Common;

public class BufferedStdinModule : IModule {

    private readonly Channel<char> channel;
    private readonly List<IDisposable> subscriptions = [];
    public ChannelWriter<char> Writer => channel.Writer;

    public BufferedStdinModule() {
        channel = Channel.CreateUnbounded<char>();
    }
    
    public void SubscribeToEvents(EventBus eventBus) {
        subscriptions.Add(eventBus.Subscribe<StdInReadEvent>(Handle));
    }

    public void UnsubscribeFromEvents() {
        foreach (IDisposable sub in subscriptions) {
            sub.Dispose();
        }
        subscriptions.Clear();
    }

    private async ValueTask Handle(StdInReadEvent evt) {
        ChannelReader<char> reader = channel.Reader;

        int i = 0;
        int max = evt.Buffer.Length;
        // read 
        while (await reader.WaitToReadAsync()) {
            _ = reader.TryRead(out char c);
            if (c == evt.Delimiter) {
                break;
            }
            evt.Buffer.Span[i] = c;
            i++;
            if (i >= max) {
                break;
            }
        }
        evt.OnReadComplete(i);
    }
}