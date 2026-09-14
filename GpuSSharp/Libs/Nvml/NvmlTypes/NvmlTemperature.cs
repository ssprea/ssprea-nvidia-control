using System.Runtime.InteropServices;

namespace GpuSSharp.Libs.Nvml.NvmlTypes;

[StructLayout(LayoutKind.Sequential)]
public struct NvmlTemperature
{
    public NvmlTemperature()
    {
        Version = (uint)Marshal.SizeOf<NvmlTemperature>() | (1u << 24);
    }
    
    public uint Version;
    public NvmlTemperatureSensors SensorType;
    public int Temperature;
}