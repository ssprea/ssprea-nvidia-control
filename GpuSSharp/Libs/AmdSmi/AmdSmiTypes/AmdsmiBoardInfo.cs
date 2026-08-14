using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct AmdsmiBoardInfo
{
    private const int AMDSMI_MAX_STRING_LENGTH = 256;
    
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = AMDSMI_MAX_STRING_LENGTH)]
    public string model_number;
    
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = AMDSMI_MAX_STRING_LENGTH)]
    public string product_serial;
    
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = AMDSMI_MAX_STRING_LENGTH)]
    public string fru_id;
    
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = AMDSMI_MAX_STRING_LENGTH)]
    public string product_name;
    
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = AMDSMI_MAX_STRING_LENGTH)]
    public string manufacturer_name;
    
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
    public UInt64 Reserved38;
    public UInt64 Reserved39;
    public UInt64 Reserved40;
    public UInt64 Reserved41;
    public UInt64 Reserved42;
    public UInt64 Reserved43;
    public UInt64 Reserved44;
    public UInt64 Reserved45;
    public UInt64 Reserved46;
    public UInt64 Reserved47;
    public UInt64 Reserved48;
    public UInt64 Reserved49;
    public UInt64 Reserved50;
    public UInt64 Reserved51;
    public UInt64 Reserved52;
    public UInt64 Reserved53;
    public UInt64 Reserved54;
    public UInt64 Reserved55;
    public UInt64 Reserved56;
    public UInt64 Reserved57;
    public UInt64 Reserved58;
    public UInt64 Reserved59;
    public UInt64 Reserved60;
    public UInt64 Reserved61;
    public UInt64 Reserved62;
    public UInt64 Reserved63;
    public UInt64 Reserved64;
}