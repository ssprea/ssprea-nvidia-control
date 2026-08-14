using System.Runtime.InteropServices;
using System.Text;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct AmdsmiAsicInfo
{
    private const int AMDSMI_MAX_STRING_LENGTH = 256;

    public UInt32 vendor_id;
    public UInt32 subvendor_id;
    public UInt64 device_id;
    public UInt32 rev_id;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = AMDSMI_MAX_STRING_LENGTH)]
    public string asic_serial;
    public UInt32 oam_id;
    public UInt64 num_of_compute_units;
    public UInt64 target_graphics_version;
    public UInt64 flags;


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
    public UInt32 Reserved14;
    public UInt32 Reserved15;
    public UInt32 Reserved16;
    public UInt32 Reserved17;
    public UInt32 Reserved18;
    public UInt32 Reserved19;
    public UInt32 Reserved20;
    public UInt32 Reserved21;
}