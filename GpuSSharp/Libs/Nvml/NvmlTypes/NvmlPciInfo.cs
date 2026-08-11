using System.Runtime.InteropServices;
using System.Text;

namespace GpuSSharp.Libs.Nvml.NvmlTypes;

public struct NvmlPciInfo
{
    public uint bus;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string busIdLegacy;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string busId;
    public uint device;
    public uint domain;
    public uint pciDeviceId;
    public uint pciSubSystemId;
}