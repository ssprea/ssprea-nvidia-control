using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct AmdsmiDriverInfo
{
    private const int AmdsmiMaxStringLength = 256;
    
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = AmdsmiMaxStringLength)]
    public string DriverVersion;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = AmdsmiMaxStringLength)]
    public string DriverDate;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = AmdsmiMaxStringLength)]
    public string DriverName;
}