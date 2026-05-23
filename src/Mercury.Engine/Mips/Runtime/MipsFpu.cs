using System.Numerics;
using System.Runtime.CompilerServices;
using Mercury.Engine.Common;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Mips.Instructions;
using Mercury.Engine.Mips.Runtime.Events;

namespace Mercury.Engine.Mips.Runtime;

/// <summary>
/// MIPS Floating Point Unit (Coprocessor 1).
/// Owns FPU registers, control registers, and condition flags.
/// Shares the machine's RegisterCollection with the CPU to allow
/// MFC1/MTC1 data movement between GPR and FPU banks.
/// </summary>
public sealed class MipsFpu : IModule {
    private readonly RegisterCollection registers;
    private readonly Action<int> branchTo;
    private EventBus eventBus = null!;

    public MipsFpu(RegisterCollection registers, Action<int> branchTo) {
        this.registers = registers;
        this.branchTo = branchTo;
        registers.DefineGroup<MipsFpuRegisters, MipsRegisterHelper>();
        registers.DefineGroup<MipsFpuControlRegisters, MipsRegisterHelper>();
    }

    /// <summary>8 condition flag bits used by C.cond and BC1T/BC1F instructions.</summary>
    public bool[] Flags { get; } = new bool[8];

    public event Action? OnFlagUpdate;

    public void SubscribeToEvents(EventBus bus) => eventBus = bus;
    public void UnsubscribeFromEvents() { }

    /// <summary>
    /// Executes a FPU instruction. Returns false if the instruction is not a FPU instruction.
    /// </summary>
    /// <param name="instruction">Decoded instruction.</param>
    /// <param name="currentWord">Raw 32-bit instruction word, used for error events.</param>
    public bool Execute(IInstruction instruction, uint currentWord) {
        switch (instruction) {
            case Abs abs: {
                switch (abs.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecUnary<float>(abs.Fs, abs.Fd, MathF.Abs);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecUnary<double>(abs.Fs, abs.Fd, Math.Abs);
                        break;
                    case TypeFInstruction.LongFixedPrecisionFormat:
                    case TypeFInstruction.WordFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case AddFloat add: {
                switch (add.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecBinary<float>(add.Fs, add.Ft, add.Fd, static (a, b) => a + b);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecBinary<double>(add.Fs, add.Ft, add.Fd, static (a, b) => a + b);
                        break;
                    case TypeFInstruction.LongFixedPrecisionFormat:
                    case TypeFInstruction.WordFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case Bc1F bc1F: {
                if (!Flags[bc1F.Cc]) {
                    branchTo(bc1F.Offset);
                }
                break;
            }
            case Bc1T bc1T: {
                if (Flags[bc1T.Cc]) {
                    branchTo(bc1T.Offset);
                }
                break;
            }
            case C c: {
                switch (c.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        Flags[c.Cc] = Compare(Read<float>(c.Fs), Read<float>(c.Ft), c.Cond);
                        OnFlagUpdate?.Invoke();
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        Flags[c.Cc] = Compare(Read<double>(c.Fs), Read<double>(c.Ft), c.Cond);
                        OnFlagUpdate?.Invoke();
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case Cfc1 cfc1: {
                registers.Set<MipsGprRegisters>(cfc1.Rt, registers.Get<MipsFpuControlRegisters>(cfc1.Fs));
                break;
            }
            case Ctc1 ctc1: {
                registers.Set<MipsFpuControlRegisters>(ctc1.Fs, registers.Get<MipsGprRegisters>(ctc1.Rt));
                break;
            }
            case CvtD cvtd: {
                switch (cvtd.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        Write<double>(cvtd.Fd, Read<float>(cvtd.Fs));
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        InvalidOp();
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case CvtS cvts: {
                switch (cvts.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        InvalidOp();
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        Write(cvts.Fd, (float)Read<double>(cvts.Fs));
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case DivFloat div: {
                switch (div.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecBinary<float>(div.Fs, div.Ft, div.Fd, static (a, b) => a / b);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecBinary<double>(div.Fs, div.Ft, div.Fd, static (a, b) => a / b);
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case Mfc1 mfc1: {
                registers.Set<MipsGprRegisters>(mfc1.Rt, registers.Get<MipsFpuRegisters>(mfc1.Fs));
                break;
            }
            case Mov mov: {
                switch (mov.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        registers.Set<MipsFpuRegisters>(mov.Fd, registers.Get<MipsFpuRegisters>(mov.Fs));
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        Write(mov.Fd, Read<double>(mov.Fs));
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case Mtc1 mtc1: {
                registers.Set<MipsFpuRegisters>(mtc1.Fs, registers.Get<MipsGprRegisters>(mtc1.Rt));
                break;
            }
            case MulFloat mul: {
                switch (mul.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecBinary<float>(mul.Fs, mul.Ft, mul.Fd, static (a, b) => a * b);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecBinary<double>(mul.Fs, mul.Ft, mul.Fd, static (a, b) => a * b);
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case Neg neg: {
                // manipula o bit do sinal diretamente para evitar conversão float
                // funciona para double pois o bit de sinal está no primeiro registrador (par)
                uint val = (uint)registers.Get<MipsFpuRegisters>(neg.Fs);
                val = val >> 31 > 0
                    ? val & 0x7FFF_FFFF          // negativo → positivo
                    : (val & 0x7FFF_FFFF) | 0x8000_0000; // positivo → negativo
                registers.Set<MipsFpuRegisters>(neg.Fd, (int)val);
                break;
            }
            case Sqrt sqrt: {
                switch (sqrt.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecUnary<float>(sqrt.Fs, sqrt.Fd, MathF.Sqrt);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecUnary<double>(sqrt.Fs, sqrt.Fd, Math.Sqrt);
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case SubFloat sub: {
                switch (sub.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecBinary<float>(sub.Fs, sub.Ft, sub.Fd, static (a, b) => a - b);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecBinary<double>(sub.Fs, sub.Ft, sub.Fd, static (a, b) => a - b);
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            default:
                return false;
        }

        return true;

        // --- local helpers ---

        void InvalidOp() {
            eventBus.Publish(new InvalidOperationEvent {
                Address = (ulong)registers.Get(MipsGprRegisters.Pc),
                Word = currentWord
            });
        }

        T Read<T>(int reg) where T : unmanaged {
            if (typeof(T) == typeof(float)) {
                int v = registers.Get<MipsFpuRegisters>(reg);
                return Unsafe.As<int, T>(ref v);
            }
            if (typeof(T) == typeof(double)) {
                long l =
                    ((long)registers.Get<MipsFpuRegisters>(reg) << 32) |
                    (uint)registers.Get<MipsFpuRegisters>(reg + 1);
                return Unsafe.As<long, T>(ref l);
            }
            throw new NotSupportedException(
                $"Mercury::Engine::Mips::Runtime::MipsFpu::Read<{typeof(T).Name}> called with unsupported type");
        }

        void Write<T>(int reg, T value) where T : unmanaged {
            if (typeof(T) == typeof(float)) {
                registers.Set<MipsFpuRegisters>(reg, Unsafe.As<T, int>(ref value));
                return;
            }
            if (typeof(T) == typeof(double)) {
                long bits = Unsafe.As<T, long>(ref value);
                registers.Set<MipsFpuRegisters>(reg,     (int)((bits >> 32) & 0xFFFF_FFFF));
                registers.Set<MipsFpuRegisters>(reg + 1, (int)(bits & 0xFFFF_FFFF));
                return;
            }
            throw new NotSupportedException(
                $"Mercury::Engine::Mips::Runtime::MipsFpu::Write<{typeof(T).Name}> called with unsupported type");
        }

        void ExecUnary<T>(int fs, int fd, Func<T, T> op) where T : unmanaged {
            Write(fd, op(Read<T>(fs)));
        }

        void ExecBinary<T>(int fs, int ft, int fd, Func<T, T, T> op) where T : unmanaged {
            Write(fd, op(Read<T>(fs), Read<T>(ft)));
        }

        bool Compare<T>(T a, T b, byte cond) where T : IEquatable<T>, IFloatingPoint<T> {
            return cond switch {
                0  => false,
                1  => T.IsNaN(a) || T.IsNaN(b),
                2  => a == b,
                3  => T.IsNaN(a) || T.IsNaN(b) || a == b,
                4  => !T.IsNaN(a) && !T.IsNaN(b) && a < b,
                5  => T.IsNaN(a) || T.IsNaN(b) || a < b,
                6  => !T.IsNaN(a) && !T.IsNaN(b) && a <= b,
                7  => T.IsNaN(a) || T.IsNaN(b) || a <= b,
                8  => SignalingCompare(false),
                9  => T.IsNaN(a) || T.IsNaN(b) || !(a > b || a < b || a == b),
                10 => SignalingCompare(!T.IsNaN(a) && !T.IsNaN(b) && a == b),
                11 => SignalingCompare(T.IsNaN(a) || T.IsNaN(b) || !(a > b || a < b)),
                12 => SignalingCompare(!T.IsNaN(a) && !T.IsNaN(b) && a < b),
                13 => SignalingCompare(T.IsNaN(a) || T.IsNaN(b) || !(a >= b)),
                14 => SignalingCompare(!T.IsNaN(a) && !T.IsNaN(b) && a <= b),
                15 => SignalingCompare(T.IsNaN(a) || T.IsNaN(b) || !(a > b)),
                _  => FallbackCompare()
            };

            bool SignalingCompare(bool result) {
                if (T.IsNaN(a) || T.IsNaN(b)) {
                    InvalidOp();
                }
                return result;
            }

            bool FallbackCompare() {
                eventBus.Publish(new UnsupportedFormatEvent());
                return false;
            }
        }
    }
}
