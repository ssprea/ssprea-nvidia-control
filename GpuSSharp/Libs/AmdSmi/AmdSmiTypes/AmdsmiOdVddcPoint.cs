using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential)]

public struct AmdsmiOdVddcPoint
{
    public UInt64 frequency;  //!< Frequency coordinate (in Hz)
    public UInt64 voltage;    //!< Voltage coordinate (in mV)
}