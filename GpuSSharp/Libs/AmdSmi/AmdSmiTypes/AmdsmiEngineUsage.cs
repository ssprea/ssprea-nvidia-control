using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential)]
public struct AmdsmiEngineUsage
{
    /// <summary>
    /// GPU core utilization %
    /// </summary>
    public UInt32 gfx_activity;
    
    /// <summary>
    /// GPU Memory Ctl utilization %
    /// </summary>
    public UInt32 umc_activity;
    
    
    public UInt32 mm_activity;
    
    public UInt32 Reserved1;
    public UInt32 Reserved2;
    public UInt32 Reserved3;
    public UInt32 Reserved4;
    public UInt32 Reserved5;
    public UInt32 Reserved6;
    public UInt32 Reserved7;
    public UInt32 Reserved8;
    public UInt32 Reserved9;
    public UInt32 Reserved10;
    public UInt32 Reserved11;
    public UInt32 Reserved12;
    public UInt32 Reserved13;
}