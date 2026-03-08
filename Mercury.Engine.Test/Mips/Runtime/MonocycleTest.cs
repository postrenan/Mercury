using Mercury.Engine.Common;
using Mercury.Engine.Common.Builders;
using Mercury.Engine.Mips.Instructions;
using Mercury.Engine.Mips.Runtime;
using Mercury.Engine.Mips.Runtime.OS;
using Mercury.Engine.Mips.Runtime.Simple;

namespace Mercury.Engine.Test.Mips.Runtime;

[TestClass]
public class MonocycleTest {
    [TestMethod]
    public void TestBeq()
    {
        using MipsMachine mipsMachine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithVolatileStorage()
                .Build())
            .WithMips()
            .WithMipsMonocycle()
            .WithMarsOs()
            .Build();
        
        Monocycle cpu = (Monocycle)mipsMachine.Modules.First(x => x is Monocycle); 
        int[] code = [
            0x2008_000f,
            0x2009_0014,
            0x1109_0002,
            0x0000_000d,
            0x0810_0006,
            0x0000_004d
            ];
        mipsMachine.LoadProgram(code, Span<int>.Empty);
        bool hasBreaked = false;
        
        var pool = new InstructionPool();
        cpu.SignalException += (e) => {
            if(e.Signal != SignalExceptionEventArgs.SignalType.Breakpoint) {
                return Task.CompletedTask;
            }

            IInstruction? inst = Disassembler.Disassemble((uint)e.Instruction, pool);
            if(inst is not Break brk) {
                return Task.CompletedTask;
            }
            Console.WriteLine($"Reached BREAK {brk.Code}");
            Assert.AreEqual(0, brk.Code);
            hasBreaked = true;
            return Task.CompletedTask;
        };
        cpu.UseBranchDelaySlot = false;
        while (!cpu.IsClockingFinished() && !hasBreaked) {
            cpu.ClockAsync().GetAwaiter().GetResult();
        }
    }

    [TestMethod]
    public void TestBuilder() {
        using MipsMachine mipsMachine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithVolatileStorage()
                .Build())
            .WithMips()
            .WithMipsMonocycle()
            .WithMarsOs()
            .Build();
        
        Assert.IsNotNull(mipsMachine.DataMemory);
        Assert.IsNotNull(mipsMachine.Memory);
        Assert.IsNotNull(mipsMachine.CpuModule);
        Assert.IsNotNull(mipsMachine.SyscallModule);
        
        const ulong gb = 1024 * 1024 * 1024;
        Assert.AreEqual(4 * gb, (mipsMachine.DataMemory as Engine.Memory.Memory)!.Size);
        Assert.IsInstanceOfType<Monocycle>(mipsMachine.CpuModule);
        Assert.IsInstanceOfType<Mars>(mipsMachine.SyscallModule);
    }

    [TestMethod]
    public void TestFibonacci() {
        using MipsMachine mipsMachine = new MachineBuilder()
            .WithMemory(new MemoryBuilder()
                .With4Gb()
                .WithVolatileStorage()
                .Build())
            .WithMips()
            .WithMipsMonocycle()
            .WithMarsOs()
            .Build();
        
        int[] code = [
            0x2004_0000,
            0x0c10_0023,
            0x1440_001d,
            0x0000_0000,
            0x2004_0001,
            0x0c10_0023,
            0x2001_0001,
            0x1422_0018,
            0x0000_0000,
            0x2004_0002,
            0x0c10_0023,
            0x2001_0001,
            0x1422_0013,
            0x0000_0000,
            0x2004_0005,
            0x0c10_0023,
            0x2001_0005,
            0x1422_000e,
            0x0000_0000,
            0x2004_0007,
            0x0c10_0023,
            0x2001_000d,
            0x1422_0009,
            0x0000_0000,
            0x2004_000d,
            0x0c10_0023,
            0x2001_00e9,
            0x1422_0004,
            0x0000_0000,
            0x2002_0011,
            0x2004_0000,
            0x0000_000c,
            0x2002_0011,
            0x2004_0001,
            0x0000_000c,
            0x1c80_0003,
            0x0000_0000,
            0x2002_0000,
            0x0e30_0008,
            0x2001_0001,    
            0x1424_0003,
            0x0000_0000,
            0x2002_0001,
            0x03e0_0008,
            0x2008_0000,
            0x2009_0001,
            0x200b_0002,
            0x0000_000c,
            0x1420_0006,
            0x0000_0000,
            0x0109_5020,
            0x0009_4020,
            0x000a_4820,
            0x216b_0001,
            0x0810_002f,
            0x000a_1020,
            0x03e0_0008
        ];
        mipsMachine.LoadProgram(code, Span<int>.Empty);

        int i = 0;
        while (!mipsMachine.IsClockingFinished()) {
            mipsMachine.ClockAsync().AsTask().GetAwaiter().GetResult();
            i++;
        }
        
        Assert.AreEqual(0, ((Monocycle)mipsMachine.Modules.Find(x => x is Monocycle)).ExitCode);    
    }
}
