using Mercury.Engine.Common;
using Mercury.Engine.Common.Builders;
using Mercury.Engine.Mips.Runtime.OS;
using Mercury.Engine.Mips.Runtime.Simple;

namespace Mercury.Engine.Mips.Runtime;

public class MipsMachineBuilder : MachineBuilder {
    private ICpuModule? cpu;
    private MipsSyscallModule? os;
    private OsType osType = OsType.NotSet;

    public MipsMachineBuilder(MachineBuilder builder) : base(builder) {
    }

    public MipsMachineBuilder WithMipsMonocycle() {
        cpu = new Monocycle();
        Modules.Add(new Monocycle());
        return this;
    }

    public MipsMachineBuilder WithCpu(ICpuModule cpu) {
        this.cpu = cpu;
        Modules.Add(cpu);
        return this;
    }

    public MipsMachineBuilder WithMarsOs() {
        if (osType != OsType.NotSet) {
            throw new NotSupportedException("OS type already set.");
        }

        osType = OsType.Named;
        os = new Mars();
        Modules.Add(os);
        return this;
    }

    public MipsMachineBuilder WithOs(MipsSyscallModule os) {
        if (osType != OsType.NotSet) {
            throw new NotSupportedException("OS type already set.");
        }

        osType = OsType.Named;
        this.os = os;
        Modules.Add(os);
        return this;
    }

    public MipsMachineBuilder WithBareMetal() {
        if (osType != OsType.NotSet) {
            throw new NotSupportedException("OS type already set.");
        }

        osType = OsType.BareMetal;
        return this;
    }
    
    public override MipsMachine Build() {
        if (DataMemory is null) {
            throw new InvalidOperationException("Data Memory must be set.");
        }

        if (InstructionMemory is null) {
            throw new InvalidOperationException("Instruction Memory must be set.");
        }

        if (cpu is null) {
            throw new InvalidOperationException("CPU must be set.");
        }

        if (osType == OsType.NotSet) {
            throw new InvalidOperationException("Operating System must be set or use bare metal");
        }

        foreach (IModule module in Modules) {
            module.SubscribeToEvents(EventBus);
        }

        MipsMachine mipsMachine = new() {
            EventBus = EventBus,
            Modules = Modules,
            Architecture = Architecture.Mips
        };

        return mipsMachine;
    }

    private enum OsType {
        NotSet,
        Named,
        Anonymous,
        BareMetal
    }
}