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
    protected IMemory? DataMemory { get; private set; }
    protected IMemory? InstructionMemory { get; private set; }


    public MachineBuilder() {
        // Default constructor for user
        EventBus = new EventBus();
        StdinModule = new BufferedStdinModule();
        Modules.Add(StdinModule);
    }

    protected MachineBuilder(MachineBuilder m) {
        DataMemory = m.DataMemory;
        InstructionMemory = m.InstructionMemory;
        EventBus = m.EventBus;
        Modules = m.Modules;
        StdinModule = m.StdinModule;
    }

    public MachineBuilder WithMemory(IMemory memory) {
        DataMemory = memory;
        InstructionMemory = memory;
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