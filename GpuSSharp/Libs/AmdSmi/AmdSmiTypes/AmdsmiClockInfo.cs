using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential)]
public struct AmdsmiClockInfo
{
    public UInt32 clk;


    public UInt32 min_clk;


    public UInt32 max_clk;


    public byte clk_locked;


    public byte clk_deep_sleep;

    public UInt32 Reserved1;
    public UInt32 Reserved2;
    public UInt32 Reserved3;
    public UInt32 Reserved4;
}