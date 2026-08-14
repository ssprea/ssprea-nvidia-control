using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential)]
public struct AmdsmiVramUsage
{
    /// <summary>
    /// Vram total in MegaBytes
    /// </summary>
    public UInt32 vram_total;
    
    /// <summary>
    /// Vram used in MegaBytes
    /// </summary>
    public UInt32 vram_used;
    
    public UInt32 Reserved1;
    public UInt32 Reserved2;
}