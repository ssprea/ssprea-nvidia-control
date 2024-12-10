using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace ssprea_nvidia_control_cli.Types;

public class FanCurve
{
    public string Name { get; set; }
    public ObservableCollection<FanCurvePoint> CurvePoints { get; set; }
    
    [JsonIgnore]
    public uint[] GpuTempToFanSpeedMap { get; private set; } = new uint[101];

    [JsonIgnore]
    public bool NeedsUpdate { get; private set; } = false;
    
    public FanCurve(params FanCurvePoint[] curvePoints) : this("New Curve", curvePoints)
    {
    } 

    [JsonConstructor]
    public FanCurve(string name, FanCurvePoint[] curvePoints)
    {
        Name = name;
        CurvePoints = new(curvePoints);
        GenerateGpuTempToFanSpeedMap();
    }

    public void GenerateGpuTempToFanSpeedMap()
    {
        for (int i = 0; i < CurvePoints.Count-1; i++) //per ogni punto
        {
            for (uint j = CurvePoints[i].Temperature; j <= CurvePoints[i+1].Temperature; j++) //per ogni temperatura j tra i due punti
            {
                GpuTempToFanSpeedMap[j] = MapGpuTempToFanPercent(CurvePoints[i].Temperature, CurvePoints[i + 1].Temperature,CurvePoints[i].FanSpeed,CurvePoints[i+1].FanSpeed,j);
            }
        }
     
    }
    
    public static FanCurve DefaultFanCurve()
    {
        return new FanCurve(
            new FanCurvePoint()
            {
                Temperature = 0,
                FanSpeed = 0
            },
            new FanCurvePoint()
            {
                Temperature = 45,
                FanSpeed = 0
            },
            new FanCurvePoint()
            {
                Temperature = 55,
                FanSpeed = 20
            },
            new FanCurvePoint()
            {
                Temperature = 65,
                FanSpeed = 40
            },
            new FanCurvePoint()
            {
                Temperature = 80,
                FanSpeed = 80
            },
            new FanCurvePoint()
            {
                Temperature = 100,
                FanSpeed = 100
            });
    }

    public static FanCurve FromSetSpeed(uint speed)
    {
        return new FanCurve(
            new FanCurvePoint()
            {
                Temperature = 0,
                FanSpeed = speed
            },
            new FanCurvePoint()
            {
                Temperature = 100,
                FanSpeed = speed
            }
        );
    }
    
    /// <summary>
    /// Map GPU temperature (°C) to fan speed based on the available points
    /// </summary>
    /// <param name="temp1">temperature of curvepoint1</param>
    /// <param name="temp2">temperature of curvepoint2</param>
    /// <param name="perc1">fan speed of curvepoint1</param>
    /// <param name="perc2">fan speed of curvepoint2</param>
    /// <param name="intemp">the temperature to convert into fan speed</param>
    /// <returns>fan speed % at the given intemp</returns>
    private uint MapGpuTempToFanPercent(uint temp1, uint temp2, uint perc1, uint perc2, uint intemp)
    {
        return perc1 + (intemp-temp1)*(perc2-perc1)/(temp2-temp1);
    }

    public override string ToString()
    {
        var final = "";

        for (int i = 0; i < GpuTempToFanSpeedMap.Length; i++)
        {
            final += $"GpuTemp: {i}, FanSpeed: {GpuTempToFanSpeedMap[i]}\n";
        }

        return final;
    }
    
    // public string ToJson()
    // {
    //     return JsonSerializer.Serialize(this);
    // }
}