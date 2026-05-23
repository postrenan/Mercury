using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Mips.Runtime.Simple;

public partial class Monocycle {
    private ValueTask<bool> ExecuteTypeF(IInstruction instruction) {
        uint word = (uint)BytesToInt32(instructionBuffer.Span);
        return ValueTask.FromResult(Fpu.Execute(instruction, word));
    }
}
