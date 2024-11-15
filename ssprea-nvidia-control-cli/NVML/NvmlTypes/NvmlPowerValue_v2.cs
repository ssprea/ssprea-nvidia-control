namespace ssprea_nvidia_control_cli.NVML.NvmlTypes;

public struct NvmlPowerValue_v2
{
    public NvmlPowerValue_v2()
    {
        Version = 33554444;
    }
    
    public uint Version;
    public NvmlPowerScope PowerScope;
    public uint PowerValueMw;
}