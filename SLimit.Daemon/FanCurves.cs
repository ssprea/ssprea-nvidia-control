using System.Collections.Concurrent;
using GpuSSharp.Libs.AmdSmi;
using GpuSSharp.Libs.Nvml;
using GpuSSharp.Types;
using Serilog;

namespace SLimit.Daemon;

public static class FanCurves
{
    public static readonly ConcurrentDictionary<string,Task> RunningFanProfilesTasks = new();
    public static readonly ConcurrentDictionary<string,CancellationTokenSource> RunningFanProfilesCancelTokens = new();
    
    public static async Task FanSpeedProfileThread(int updateDelayMilliseconds,IGpu targetGpu, FanCurve fanCurve,CancellationToken cancelToken)
    {
        int errorCounter = 0;
        int errorQuitThreshold = 50;
        uint lastFanTemp = 0;

        if (targetGpu.Vendor == GpuVendor.Amd)
        {
            Log.Information("Selected GPU {gpuId} is AmdGpu, fan curve thread not required.",targetGpu.DevicePciAddress);
            var amdGpu = (AmdSmiGpu)targetGpu;
            var points = fanCurve.CurvePoints.Select(x => (x.Temperature, x.FanSpeed));
            amdGpu.ApplyFirmwareFanCurve(points.ToList());
            Log.Information("Successfully loaded fan curve to GPU firmware: {fanCurveName}. Exiting.",amdGpu.Name);
            
            return;
        }
        
        Log.Information("Starting fan curve thread for GPU: {gpuName}, update delay: {pollDelay}ms",targetGpu.Name,updateDelayMilliseconds);
        using var timer = new PeriodicTimer(
            TimeSpan.FromMilliseconds(updateDelayMilliseconds));
        
        while (await timer.WaitForNextTickAsync(cancelToken))
        {
            try
            {


                var latestMetrics = targetGpu.GetMetrics();
                var currentTemp = (uint)latestMetrics.GpuTemperature;

                //get gpu temperature
                if (currentTemp == lastFanTemp)
                {
                    Log.Debug("No temp change since last update. skipping");
                    continue;
                }



                Log.Debug("Gpu temp: {gpuTemp}, Fan Speed: {fanSpeed}", currentTemp,
                    fanCurve.GpuTempToFanSpeedMap[currentTemp]);
                if (!targetGpu.ApplySpeedToAllFans(fanCurve.GpuTempToFanSpeedMap[currentTemp]))
                {
                    errorCounter++;
                    Log.Error("({errorCount}) Error while applying fan speed.", errorCounter);
                }
                else
                    errorCounter = 0;


                if (errorCounter > errorQuitThreshold)
                {
                    Log.Fatal("More than {quitThreshold} errors when applying fan curve. Stopping thread.",
                        errorQuitThreshold);
                    return;
                }

                lastFanTemp = (uint)latestMetrics.GpuTemperature;
            }
            catch (Exception ex)
            {
                Log.Error("Error while running fan profile thread on GPU {gpuId}: {ex}", targetGpu.DevicePciAddress, ex);
            }
        }
    }
}