using System.Runtime.InteropServices;
using GpuSSharp.Libs.AmdSmi.AmdSmiTypes;
using GpuSSharp.Libs.Nvml.NvmlTypes;
using GpuSSharp.Types;

namespace GpuSSharp.Libs.AmdSmi;

public class AmdSmiGpu : IGpu
{
    private IntPtr _processorHandle;

    public AmdSmiGpu(IntPtr processorHandle)
    {
        _processorHandle = processorHandle;
        
        
        //get pci address
        if (AmdSmiWrapper.amdsmi_get_gpu_device_bdf(_processorHandle, out var bdfInfo) ==
            AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            DevicePciAddress = bdfInfo.ToString();
        }
        else
        {
            throw new  Exception("Unable to get gpu board info");
        }
        
        //get name
        if (AmdSmiWrapper.amdsmi_get_gpu_board_info(_processorHandle, out var boardInfo) ==
            AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            Name = boardInfo.product_name;
        }
        else
        {
            throw new  Exception("Unable to get gpu board info");
        }
        
        //power cap info
        if (AmdSmiWrapper.amdsmi_get_power_cap_info(_processorHandle, 0, out var powerCapInfo) ==
            AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            PowerLimitMinMw = (uint)(powerCapInfo.min_power_cap / 1000);
            PowerLimitMaxMw = (uint)(powerCapInfo.max_power_cap / 1000);
            PowerLimitDefaultMw = (uint)(powerCapInfo.default_power_cap / 1000);
        }

        var threshShutdown = AmdSmiWrapper.amdsmi_get_temp_metric(_processorHandle,
            AmdsmiTemperatureType.AMDSMI_TEMPERATURE_TYPE_EDGE, AmdsmiTemperatureMetric.AMDSMI_TEMP_SHUTDOWN,
            out var tempThresh);
            
        
        //temp thresholds
        if (threshShutdown  == AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            TemperatureThresholdShutdown = (uint)tempThresh;
        }
        // Console.WriteLine("shutdown "+threshShutdown);


        var threshSlow = AmdSmiWrapper.amdsmi_get_temp_metric(_processorHandle,
            AmdsmiTemperatureType.AMDSMI_TEMPERATURE_TYPE_EDGE, AmdsmiTemperatureMetric.AMDSMI_TEMP_CRITICAL,
            out tempThresh);
        
        if (threshSlow == AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            TemperatureThresholdSlowdown = (uint)tempThresh;
            
        }
        // Console.WriteLine("slowdown "+threshSlow);

        var threshThrottle = AmdSmiWrapper.amdsmi_get_temp_metric(_processorHandle,
            AmdsmiTemperatureType.AMDSMI_TEMPERATURE_TYPE_EDGE, AmdsmiTemperatureMetric.AMDSMI_TEMP_MAX,
            out tempThresh);
        
        if (threshThrottle == AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            TemperatureThresholdThrottle = (uint)tempThresh;
            
        }
        // Console.WriteLine("throttle "+threshThrottle);
        
        //max fan speed
        if (AmdSmiWrapper.amdsmi_get_gpu_fan_speed_max(_processorHandle,0,out var maxSpeed) == AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            _maxFanSpeed = maxSpeed;
        }
        
        
        //max and min clock values

        var gpuOdVoltInfoSuccess = AmdSmiWrapper.amdsmi_get_gpu_od_volt_info(_processorHandle, out var clockBoundsInfo);
        
        // Console.WriteLine("clock od volt info: "+gpuOdVoltInfoSuccess);
        
        if (gpuOdVoltInfoSuccess ==  AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            var coreLimits = clockBoundsInfo.sclk_freq_limits;
            var memLimits = clockBoundsInfo.mclk_freq_limits;
            
            ClockCoreMaxMhz = (uint)(coreLimits.upper_bound/1000000);
            ClockCoreMinMhz = (uint)(coreLimits.lower_bound/1000000);
            ClockMemMaxMhz  = (uint)(memLimits.upper_bound /1000000);
            ClockMemMinMhz  = (uint)(memLimits.lower_bound /1000000);
        }
        
        //get driver version
        // Console.WriteLine("driver info: "+ AmdSmiWrapper.amdsmi_get_gpu_driver_info(_processorHandle, out var driverInfo));
        DriverVersion = GetDriverVer();



        if (DevicePciAddress is not null)
        {
            var voltageCapInfo = AmdSysfsWrapper.GetVoltageOffsetLimits(DevicePciAddress);
            VoltageCoreMinOffsetMv = voltageCapInfo.Item1;
            VoltageCoreMaxOffsetMv = voltageCapInfo.Item2;
        }
        else
        {
            VoltageCoreMinOffsetMv = 0;
            VoltageCoreMaxOffsetMv = 0;
        }
        // Console.WriteLine($"volt of max: {VoltageCoreMaxOffsetMv} min: {VoltageCoreMinOffsetMv}");
        
        Capabilities = new GpuCapabilities(GpuClockTuningMode.ClockRange, GpuClockTuningMode.ClockRange,true, false,true,true);
        
        // ApplyAutoSpeedToAllFans();
        
        
        // Console.WriteLine("core max: "+ClockCoreMaxMhz);
        // Console.WriteLine("core min: "+ClockCoreMinMhz);
        // Console.WriteLine("mem max: "+ ClockMemMaxMhz );
        // Console.WriteLine("mem min: "+ ClockMemMinMhz );
    }
    

    public uint DeviceIndex { get; }
    public string DevicePciAddress { get; } = null!;
    public string Name { get; }
    public GpuVendor Vendor => GpuVendor.Amd;
        
    public GpuCapabilities Capabilities { get; } 
    
    

    public uint PowerLimitMinMw { get; }
    public uint PowerLimitMaxMw { get; }
    public uint PowerLimitDefaultMw { get; }
    
    //TODO: implement multiple fans
    public uint FansCount => 1; 
    public uint TemperatureThresholdShutdown { get; }
    public uint TemperatureThresholdSlowdown { get; }
    public uint TemperatureThresholdThrottle { get; }
    
    //clock limits
    
    public uint ClockCoreMaxMhz { get; }
    public uint ClockCoreMinMhz { get; }
    public uint ClockMemMaxMhz { get; }
    public uint ClockMemMinMhz { get; }
    public int VoltageCoreMaxOffsetMv { get; }
    public int VoltageCoreMinOffsetMv { get; }

    //driver

    public string DriverVersion { get; }


    private UInt64 _maxFanSpeed;
    
    public GpuMetrics GetMetrics()
    {
        AmdSmiWrapper.amdsmi_get_clock_info(_processorHandle, AmdSmiClockType.AMDSMI_CLK_TYPE_GFX, out var coreClockInfo);
        AmdSmiWrapper.amdsmi_get_clock_info(_processorHandle, AmdSmiClockType.AMDSMI_CLK_TYPE_MEM, out var memClockInfo);
        AmdSmiWrapper.amdsmi_get_clock_info(_processorHandle, AmdSmiClockType.AMDSMI_CLK_TYPE_VCLK0, out var videoClockInfo);
        AmdSmiWrapper.amdsmi_get_power_cap_info(_processorHandle,0, out var powerCapInfo);
        AmdSmiWrapper.amdsmi_get_power_info(_processorHandle,out var powerInfo);
        AmdSmiWrapper.amdsmi_get_gpu_vram_usage(_processorHandle,out var vramUsageInfo);
        AmdSmiWrapper.amdsmi_get_gpu_activity(_processorHandle,out var gpuActivityInfo);
        AmdSmiWrapper.amdsmi_get_temp_metric(_processorHandle,AmdsmiTemperatureType.AMDSMI_TEMPERATURE_TYPE_EDGE,AmdsmiTemperatureMetric.AMDSMI_TEMP_CURRENT,out var currentTempInfo);
        AmdSmiWrapper.amdsmi_get_temp_metric(_processorHandle,AmdsmiTemperatureType.AMDSMI_TEMPERATURE_TYPE_HOTSPOT,AmdsmiTemperatureMetric.AMDSMI_TEMP_CURRENT,out var currentTempHotspotInfo);
        AmdSmiWrapper.amdsmi_get_gpu_fan_speed(_processorHandle,0,out var fanSpeed);
        var clockBounds = GetClockBounds().Item2;

        var vOffset = AmdSysfsWrapper.GetCurrentVoltageOffset(DevicePciAddress);
        
        uint fanSpeedPercent = _maxFanSpeed == 0 ? 0 : (uint)(fanSpeed * 100 / _maxFanSpeed);
        
        return new GpuMetrics(
            coreClockInfo.clk,
            memClockInfo.clk,
            coreClockInfo.clk,
            videoClockInfo.clk,
            (uint)(powerCapInfo.power_cap / 1000),
            (uint)(powerInfo.socket_power * 1000),
            
            (vramUsageInfo.vram_total - vramUsageInfo.vram_used) * 1000 * 1000,
            vramUsageInfo.vram_used * 1000 * 1000,
            vramUsageInfo.vram_total * 1000 * 1000,
            gpuActivityInfo.gfx_activity,
            gpuActivityInfo.umc_activity,
            currentTempInfo,
            currentTempHotspotInfo,
            GpuPState.GpuPstateUnknown,
            new GpuFansMetrics((fanSpeedPercent)),
            (uint)(clockBounds.curr_sclk_range.upper_bound/1000000),
            (uint)(clockBounds.curr_mclk_range.upper_bound/1000000),
            (uint)(clockBounds.curr_sclk_range.lower_bound/1000000),
            (uint)(clockBounds.curr_mclk_range.lower_bound/1000000),
            vOffset,
            0

        );
    }

    private string GetDriverVer()
    {
        //check /sys/module/amdgpu/version
        if (File.Exists("/sys/module/amdgpu/version"))
            return File.ReadAllText("/sys/module/amdgpu/version");

        return $"amdgpu - {File.ReadAllText("/proc/sys/kernel/osrelease")}";

    }
    
    private (AmdsmiStatus, AmdsmiOdVoltFreqData) GetClockBounds()
    {
        var gpuOdVoltInfoSuccess = AmdSmiWrapper.amdsmi_get_gpu_od_volt_info(_processorHandle, out var clockBoundsInfo);
        return (gpuOdVoltInfoSuccess, clockBoundsInfo);
    }
    
    
    
    public bool SetCoreTuning(GpuClockTune tuneSettings)
    {
        
        return tuneSettings switch
        {
            GpuClockTune.ClockRange range =>
                SetClockRange(range.MinMhz, range.MaxMhz, AmdSmiClockType.AMDSMI_CLK_TYPE_GFX),
        
            _ => false
        };
        
        // if (Capabilities.CoreClockTuningMode )
        //
        // if (pState != GpuPState.GpuPstate0 || tuningMode != Capabilities.CoreClockTuningMode)
        //     return false;
        //
        // if (tuningValue <= 0)
        //     ResetGpuPerformanceLevel();
    }

    private bool SetClockRange(ulong minClock, ulong maxClock, AmdSmiClockType clockType)
    {
        var minResult = AmdSmiWrapper.amdsmi_set_gpu_clk_limit(_processorHandle, clockType, AmdsmiClkLimitType.CLK_LIMIT_MIN, minClock);
        var maxResult = AmdSmiWrapper.amdsmi_set_gpu_clk_limit(_processorHandle, clockType, AmdsmiClkLimitType.CLK_LIMIT_MAX, maxClock);
        
        Console.WriteLine("min: "+minResult);
        Console.WriteLine("max: "+maxResult);
        
        return (minResult == AmdsmiStatus.AMDSMI_STATUS_SUCCESS && maxResult == AmdsmiStatus.AMDSMI_STATUS_SUCCESS);
    }
    

    public bool SetMemTuning(GpuClockTune tuneSettings)
    {
        
        return tuneSettings switch
        {
            GpuClockTune.ClockRange range =>
                SetClockRange(range.MinMhz, range.MaxMhz, AmdSmiClockType.AMDSMI_CLK_TYPE_MEM),
        
            _ => false
        };
    }

    public bool SetGpuPowerLimit(uint limitMw)
    {
        if (limitMw < PowerLimitMinMw || limitMw > PowerLimitMaxMw)
            return false;

        ulong requestedLimitUWatt = limitMw * 1000UL;

        return AmdSmiWrapper.amdsmi_set_power_cap(_processorHandle, 0, requestedLimitUWatt) ==
               AmdsmiStatus.AMDSMI_STATUS_SUCCESS;

    }

    public bool SetCoreVoltageOffset(int voltageOffset)
    {
        if (voltageOffset < VoltageCoreMinOffsetMv)
            voltageOffset =  VoltageCoreMinOffsetMv;
        if (voltageOffset > VoltageCoreMaxOffsetMv)
            voltageOffset = VoltageCoreMaxOffsetMv;
        
        AmdSysfsWrapper.SetVoltageOffset(DevicePciAddress,voltageOffset);
        return true;
    }

    public bool SetMemoryVoltageOffset(int voltageOffset)
    {
        return false;
    }

    public bool ApplySpeedToAllFans(uint speed)
    {
        if (speed > 100)
            return false;
        
        for (uint i = 0; i < FansCount; i++)
        {

            ulong nativeSpeed = speed * _maxFanSpeed / 100UL;

            var status = AmdSmiWrapper.amdsmi_set_gpu_fan_speed(_processorHandle, i, nativeSpeed);

            Console.WriteLine ("speed: "+status+"  id: "+i + "speed: "+nativeSpeed);
            
            if (status != AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
                return false;
        }

        return true;
    }

    public bool ApplyFirmwareFanCurve(List<(uint Temperature, uint FanPercent)> curve)
    {
        AmdSysfsWrapper.SetFirmwareFanCurve(DevicePciAddress,curve);
        return true;
    }

    public bool ApplyAutoSpeedToAllFans()
    {
        // for (uint i = 0; i < FansCount; i++)
        // {
        //     var status = AmdSmiWrapper.amdsmi_reset_gpu_fan(_processorHandle, i);
        //
        //     if (status != AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        //         return false;
        // }
        //
        // return true;
        AmdSysfsWrapper.ResetFanControl(DevicePciAddress);
        return true;

    }

    private bool ResetGpuPerformanceLevel()
    {
        return AmdSmiWrapper.amdsmi_set_gpu_perf_level(_processorHandle, AmdsmiDevPerfLevel.AMDSMI_DEV_PERF_LEVEL_AUTO) == AmdsmiStatus.AMDSMI_STATUS_SUCCESS;
    }
    
}