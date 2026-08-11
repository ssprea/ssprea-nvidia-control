namespace GpuSSharp.Types;

public class GpuFansMetrics
{

    public GpuFansMetrics(params uint[] fansSpeeds)
    {
        FansSpeed = fansSpeeds;
    }
    
    public uint this[int index] => FansSpeed[index];
    
    public uint[] FansSpeed { get; }
    public uint Fan0Speed  => FansSpeed[0];
}

