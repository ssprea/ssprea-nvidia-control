using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential)]
public struct AmdsmiRange
{
    public UInt64 lower_bound;
    public UInt64 upper_bound;
    
    private UInt64 Reserved1;
    private UInt64 Reserved2;
}