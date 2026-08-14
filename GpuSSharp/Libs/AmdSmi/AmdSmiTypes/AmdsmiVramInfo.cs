using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential)]
public struct AmdsmiVramInfo
{
    /// <summary>
    /// Vram size in MegaBytes
    /// </summary>
    public UInt64 vram_size;
    
    /// <summary>
    /// Bit width in bits
    /// </summary>
    public UInt32 vram_bit_width;
    
    /// <summary>
    /// Max bandwidth at current memory clock, in GigaBytes/s
    /// </summary>
    public UInt64 vram_max_bandwidth;
    
    public UInt64 Reserved1;
    public UInt64 Reserved2;
    public UInt64 Reserved3;
    public UInt64 Reserved4;
    public UInt64 Reserved5;
    public UInt64 Reserved6;
    public UInt64 Reserved7;
    public UInt64 Reserved8;
    public UInt64 Reserved9;
    public UInt64 Reserved10;
    public UInt64 Reserved11;
    public UInt64 Reserved12;
    public UInt64 Reserved13;
    public UInt64 Reserved14;
    public UInt64 Reserved15;
    public UInt64 Reserved16;
    public UInt64 Reserved17;
    public UInt64 Reserved18;
    public UInt64 Reserved19;
    public UInt64 Reserved20;
    public UInt64 Reserved21;
    public UInt64 Reserved22;
    public UInt64 Reserved23;
    public UInt64 Reserved24;
    public UInt64 Reserved25;
    public UInt64 Reserved26;
    public UInt64 Reserved27;
    public UInt64 Reserved28;
    public UInt64 Reserved29;
    public UInt64 Reserved30;
    public UInt64 Reserved31;
    public UInt64 Reserved32;
    public UInt64 Reserved33;
    public UInt64 Reserved34;
    public UInt64 Reserved35;
    public UInt64 Reserved36;
    public UInt64 Reserved37;
}