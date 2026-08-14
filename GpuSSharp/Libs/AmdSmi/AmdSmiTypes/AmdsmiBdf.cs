using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Explicit)]
public struct AmdsmiBdf
{
    [FieldOffset(0)]
    public ulong AsUInt;

    public readonly byte FunctionNumber =>
        (byte)(AsUInt & 0b111);

    public readonly byte DeviceNumber =>
        (byte)((AsUInt >> 3) & 0b1_1111);

    public readonly byte BusNumber =>
        (byte)((AsUInt >> 8) & 0xFF);

    public readonly ulong DomainNumber =>
        AsUInt >> 16;
    
    public readonly override string ToString()
    {
        return $"{DomainNumber:X4}:{BusNumber:X2}:{DeviceNumber:X2}.{FunctionNumber}";
    }
}