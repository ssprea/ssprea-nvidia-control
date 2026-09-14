namespace GpuSSharp.Libs.AmdSmi;

public static class AmdSysfsWrapper
{
    /// <summary>
    /// Sets the fan curve for the gpu firmware to follow
    /// </summary>
    /// <param name="pciAddress">gpu's pci address</param>
    /// <param name="curvePoints">collections of curve points</param>
    public static void SetFirmwareFanCurve(string pciAddress, List<(uint Temperature, uint FanPercent)> curvePoints)
    {
        if (curvePoints.Count != 5)
        {
            throw new ArgumentException("Fan curve points should be 5.");
        }

        bool isValid = curvePoints
            .Zip(curvePoints.Skip(1), (previous, current) =>
                current.Temperature > previous.Temperature &&
                current.FanPercent >= previous.FanPercent)
            .All(valid => valid);

        if (!isValid)
            throw new ArgumentException("Fan curve points should be ordered by increasing speed and temperature.");
        
        string sysfsPath = $"/sys/bus/pci/devices/{pciAddress}/gpu_od/fan_ctrl/fan_curve";
        
        for (int i = 0; i < curvePoints.Count; i++)
        {
            var (temperature, percent) = curvePoints[i];
            File.WriteAllText(sysfsPath, $"{i} {(temperature <25  ? 25 : temperature)} {(percent <15 ? 15 : percent)}\n");
            Console.WriteLine($"{i} {temperature} {percent}\n");
        }

        File.WriteAllText(sysfsPath, "c\n");
    }

    /// <summary>
    /// Resets the GPU fan curve to defaults
    /// </summary>
    /// <param name="pciAddress">gpu's pci address</param>
    public static void ResetFanControl(string pciAddress)
    {
        string sysfsPath = $"/sys/bus/pci/devices/{pciAddress}/gpu_od/fan_ctrl/fan_curve";
        
        
        File.WriteAllText(sysfsPath, "r\n");
        File.WriteAllText(sysfsPath, "c\n");
    }


    public static (int, int) GetVoltageOffsetLimits(string pciAddress)
    {
        string sysfsPath = $"/sys/bus/pci/devices/{pciAddress}/pp_od_clk_voltage";

        var rawLines = File.ReadAllLines(sysfsPath);
        foreach (var l in rawLines )
            if (l.StartsWith("VDDGFX_OFFSET:"))
            {
                var values = l.Trim().Substring("VDDGFX_OFFSET:".Length).Split("mv");
                if (values.Length >= 2 && int.TryParse(values[0].Trim(), out int offsetMin) && int.TryParse(values[1].Trim(), out int offsetMax))
                    return (offsetMin, offsetMax);
               
            }

        return (0, 0);
    }

    public static int GetCurrentVoltageOffset(string pciAddress)
    {
        string sysfsPath = $"/sys/bus/pci/devices/{pciAddress}/pp_od_clk_voltage";
        var rawLines = File.ReadAllLines(sysfsPath);
        for (int i = 0; i < rawLines.Length; i++)
        {
            var l = rawLines[i];
            if (l.StartsWith("OD_VDDGFX_OFFSET:") && i + 1 < rawLines.Length)
            {
                var value = rawLines[i + 1];
                if (int.TryParse(value.Trim(), out int offsetCurr) )
                    return (offsetCurr);
               
            }
        }

        return 0;
    }
    
    public static void SetVoltageOffset(string pciAddress, int offsetMv)
    {
        string sysfsPath = $"/sys/bus/pci/devices/{pciAddress}/pp_od_clk_voltage";
        
        File.WriteAllText(sysfsPath,$"vo {offsetMv}\n");
        File.WriteAllText(sysfsPath,"c\n");
        
    }

    public static void ResetVoltageOffset(string pciAddress)
    {
        SetVoltageOffset(pciAddress, 0);
    }
}