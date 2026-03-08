using System.Threading.Channels;
using ELFSharp.ELF;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Memory;
using Mercury.Engine.Mips.Runtime;
using Mercury.Engine.Mips.Runtime.Simple;

namespace Mercury.Engine.Common;

/// <summary>
/// A class that holds all the parts that the simulated computer needs
/// to function.
/// </summary>
public abstract class Machine : IAsyncClockable, IDisposable {

    public required EventBus EventBus { get; set; } = new();

    public List<IModule> Modules { get; set; } = [];
    
    /// <summary>
    /// A reference to the memory object that holds all data
    /// that the program operates on. It may be the same object
    /// as <see cref="Memory"/>.
    /// </summary>
    public IMemory DataMemory { get; init; } = null!;

    /// <summary>
    /// A reference to the memory object that contains instructions.
    /// It may be the same object as <see cref="DataMemory"/>.
    /// </summary>
    public IMemory Memory { get; init; } = null!;

    /// <summary>
    /// The object that executes code.
    /// </summary>
    public ICpuModule CpuModule {
        get {
            field ??= (ICpuModule?)Modules.Find(x => x is ICpuModule);
            return field ?? throw new NullReferenceException("No module found implementing ICpuModule");
        }
    }

    /// <summary>
    /// The Operating System that answers syscalls of this machine.
    /// </summary>
    public ISyscallModule? SyscallModule {
        get {
            field ??= (ISyscallModule?)Modules.Find(x => x is ISyscallModule);
            return field;
        }
    }

    public BufferedStdinModule StdIn {
        get {
            field ??= (BufferedStdinModule?)Modules.Find(x => x is BufferedStdinModule);
            return field ?? throw new NullReferenceException("No BufferedStdinModule found");
        }
    }
    

    // /// <summary>
    // /// The standard input that gives data to the program being run.
    // /// </summary>
    // public required Channel<char> StdIn { get; init; }
    //
    // /// <summary>
    // /// The standard output that programs can write to.
    // /// </summary>
    // public required Channel<char> StdOut { get; init; }
    //
    // /// <summary>
    // /// The standard channels where errors from the program and machine are written to.
    // /// </summary>
    // public required Channel<char> StdErr { get; init; }

    /// <summary>
    /// The current architecture of this machine.
    /// </summary>
    public required Architecture Architecture { get; init; }

    public bool IsDisposed { get; private set; }
    
    public async ValueTask ClockAsync() {
        await EventBus.PublishAsync(new ClockEvent());
        ValueTuple<Type, int>[] dirty = CpuModule.Registers.GetDirty(out int count);
        OnRegisterChanged?.Invoke(dirty,count);
    }

    public bool IsClockingFinished() => ((Monocycle)Modules.Find(x => x is Monocycle)!).IsClockingFinished();
    
    /// <summary>
    /// Raised every cycle with a list of the changed registers. Contains
    /// the enum type of the register and the actual register as a base <see cref="Enum"/>.
    /// </summary>
    public event Action<ValueTuple<Type,int>[], int>? OnRegisterChanged;
    
    /// <summary>
    /// Event fired when any access to the memory is made.
    /// </summary>
    public event EventHandler<MemoryAccessEventArgs>? MemoryAccessed;

    /// <summary>
    /// Loads a ELF executable into the <see cref="DataMemory"/>.
    /// </summary>
    /// <param name="elf">The ELF file to be loaded</param>
    public abstract void LoadElf(ELF<uint> elf);
    
    protected uint Load(Span<byte> data, uint address) {
        for(ulong i=0;i<(ulong)data.Length; i++) {
            DataMemory.WriteByte(address + i, data[(int)i]);
        }
        return address + (uint)data.Length;
    }
    
    protected uint Load(Span<int> bytes, uint address) {
        for (ulong i = 0; i < (ulong)bytes.Length; i++) {
            DataMemory.WriteWord(address + i*4, bytes[(int)i]);
        }
        return address + (uint)bytes.Length * 4;
    }

    /// <summary>
    /// Loads a program based on a list of words. Separates between instructions and data. 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="data"></param>
    public abstract void LoadProgram(Span<int> text, Span<int> data);
    
    /// <summary>
    /// Disposes all resources used by this machine.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }
        IsDisposed = true;
        
        // remove links
        //Cpu.Machine = null!;
        //Os.Machine = null!;
        
        // dispose objects
        if(DataMemory is IDisposable dispDMem) dispDMem.Dispose();
        if(Memory is IDisposable dispIMem) dispIMem.Dispose();
        SyscallModule?.Dispose();
    }

    /// <summary>
    /// Raises the <see cref="MemoryAccessed"/> event.
    /// </summary>
    /// <param name="e">The arguments of the event</param>
    public void OnMemoryAccess(MemoryAccessEventArgs e)
    {
        MemoryAccessed?.Invoke(this, e);
    }
}