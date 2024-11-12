namespace ssprea_nvidia_control.Models;

public class FanCurvePoint(uint temperature, uint fanSpeed)
{
    public uint Temperature { get; set; } = temperature;
    public uint FanSpeed  { get; set; } = fanSpeed;

    public FanCurvePoint() : this(0, 0)
    {
        
    }
}