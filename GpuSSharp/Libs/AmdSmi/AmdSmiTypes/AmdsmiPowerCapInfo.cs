using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential)]
public struct AmdsmiPowerCapInfo
{
    /// <summary>
    /// current power cap Units uW {@linux_bm} or W {@host} 
    /// </summary>
    public UInt64 power_cap;


    /// <summary>
    /// default power cap Units uW {@linux_bm} or W {@host} 
    /// </summary>
    public UInt64 default_power_cap;


    /// <summary>
    /// dpm power cap Units MHz {@linux_bm} or Hz {@host} 
    /// </summary>
    public UInt64 dpm_cap;


    /// <summary>
    /// minimum power cap Units uW {@linux_bm} or W {@host} 
    /// </summary>
    public UInt64 min_power_cap;


    /// <summary>
    /// maximum power cap Units uW {@linux_bm} or W {@host} 
    /// </summary>
    public UInt64 max_power_cap;
    
    public UInt64 Reserved1;
    public UInt64 Reserved2;
    public UInt64 Reserved3;
}