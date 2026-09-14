using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;



[StructLayout(LayoutKind.Explicit, Size = 184)]
public struct AmdsmiOdVoltFreqData
{
    [FieldOffset(0)]
    public AmdsmiRange curr_sclk_range;   //!< The current SCLK frequency range in MHz
    
    [FieldOffset(16)]
    public AmdsmiRange curr_mclk_range;   //!< The current MCLK frequency range, upper bound only in MHz
    
    [FieldOffset(32)]
    public AmdsmiRange sclk_freq_limits;  //!< The range possible of SCLK values in MHz
    
    [FieldOffset(48)]
    public AmdsmiRange mclk_freq_limits;  //!< The range possible of MCLK values in MHz
    
    [FieldOffset(64)]
    public AmdsmiOdVoltCurve curve;     //!< The current voltage curve
    
    [FieldOffset(112)]
    public UInt32 num_regions;             //!< The number of voltage curve regions
}