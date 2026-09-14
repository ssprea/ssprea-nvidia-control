using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential)]

public struct AmdsmiOdVoltCurve
{
    // private const int AMDSMI_NUM_VOLTAGE_CURVE_POINTS = 3;
    
    
    public AmdsmiOdVddcPoint VcPoint0;
    public AmdsmiOdVddcPoint VcPoint1;
    public AmdsmiOdVddcPoint VcPoint2;
    
    
    public readonly AmdsmiOdVddcPoint GetPoint(int index) => index switch
    {
        0 => VcPoint0,
        1 => VcPoint1,
        2 => VcPoint2,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };
}