using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

[StructLayout(LayoutKind.Sequential)]
public struct AmdsmiPowerInfo
{
    public UInt64 socket_power;


    public UInt32 current_socket_power;


    public UInt32 average_socket_power;


    public UInt64 gfx_voltage;


    public UInt64 soc_voltage;


    public UInt64 mem_voltage;


    public UInt32 power_limit;

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

}