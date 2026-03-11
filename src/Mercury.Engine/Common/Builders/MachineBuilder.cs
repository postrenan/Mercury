using System.Threading.Channels;
using Mercury.Engine.Memory;
using Mercury.Engine.Mips.Runtime;

namespace Mercury.Engine.Common.Builders;

/// <summary>
/// The standard builder for creating a machine instance. Must be extended for specific machine types because
/// the <see cref="Build"/> method will throw an exception if not overridden.
/// </summary>
public class MachineBuilder : IBuilder<Machine> {
    protected EventBus EventBus { get; init; }
    protected BufferedStdinModule StdinModule { get; init; }
    protected List<IModule> Modules { get; init; } = [];
    protected IMemory? Memory { get; private set; }
    protected AddressDecoderModule AddressDecoderModule { get; init; }


    public MachineBuilder() {
        // Default constructor for user
        EventBus = new EventBus();
        StdinModule = new BufferedStdinModule();
        Modules.Add(StdinModule);
        AddressDecoderModule = new AddressDecoderModule();
        Modules.Add(AddressDecoderModule);
    }

    protected MachineBuilder(MachineBuilder m) {
        Memory = m.Memory;
        EventBus = m.EventBus;
        Modules = m.Modules;
        StdinModule = m.StdinModule;
        AddressDecoderModule = m.AddressDecoderModule;
    }

    public MachineBuilder WithMemory(IMemory memory) {
        Memory = memory;
        Modules.Add((IModule)memory);
        return this;
    }

    public MipsMachineBuilder WithMips() {
        return new MipsMachineBuilder(this);
    }

    public virtual Machine Build() {
        throw new InvalidOperationException("Build method must be overridden in derived class.");
    }
}