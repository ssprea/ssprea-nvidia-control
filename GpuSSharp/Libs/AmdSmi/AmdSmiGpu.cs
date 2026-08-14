using System.Runtime.InteropServices;
using GpuSSharp.Libs.AmdSmi.AmdSmiTypes;
using GpuSSharp.Types;

namespace GpuSSharp.Libs.AmdSmi;

public class AmdSmiGpu : IGpu
{
    private IntPtr _processorHandle;

    public AmdSmiGpu(IntPtr processorHandle)
    {
        _processorHandle = processorHandle;
        
        Capabilities = new GpuCapabilities(GpuClockTuningMode.Overdrive, GpuClockTuningMode.Overdrive, true,_supportsFanSpeedControl);
        
        //get pci address
        if (AmdSmiWrapper.amdsmi_get_gpu_device_bdf(_processorHandle, out var bdfInfo) ==
            AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            DevicePciAddress = bdfInfo.ToString();
        }
        
        //get name
        if (AmdSmiWrapper.amdsmi_get_gpu_board_info(_processorHandle, out var boardInfo) ==
            AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            Name = boardInfo.product_name;
        }
        
        //power cap info
        if (AmdSmiWrapper.amdsmi_get_power_cap_info(_processorHandle, 0, out var powerCapInfo) ==
            AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            PowerLimitMinMw = (uint)(powerCapInfo.min_power_cap / 1000);
            PowerLimitMaxMw = (uint)(powerCapInfo.max_power_cap / 1000);
            PowerLimitDefaultMw = (uint)(powerCapInfo.default_power_cap / 1000);
        }
        
        //temp thresholds
        if (AmdSmiWrapper.amdsmi_get_temp_metric(_processorHandle, AmdsmiTemperatureType.AMDSMI_TEMPERATURE_TYPE_EDGE,AmdsmiTemperatureMetric.AMDSMI_TEMP_SHUTDOWN, out var tempThresh) ==
            AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            TemperatureThresholdShutdown = (uint)tempThresh;
        }
        
        
        if (AmdSmiWrapper.amdsmi_get_temp_metric(_processorHandle, AmdsmiTemperatureType.AMDSMI_TEMPERATURE_TYPE_EDGE,AmdsmiTemperatureMetric.AMDSMI_TEMP_CRITICAL, out tempThresh) ==
            AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            TemperatureThresholdSlowdown = (uint)tempThresh;
        }
        
        
        if (AmdSmiWrapper.amdsmi_get_temp_metric(_processorHandle, AmdsmiTemperatureType.AMDSMI_TEMPERATURE_TYPE_EDGE,AmdsmiTemperatureMetric.AMDSMI_TEMP_MAX, out tempThresh) ==
            AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            TemperatureThresholdThrottle = (uint)tempThresh;
        }
        
        //max fan speed
        if (AmdSmiWrapper.amdsmi_get_gpu_fan_speed_max(_processorHandle,0,out var maxSpeed) == AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            _maxFanSpeed = maxSpeed;
        }
        
        
        //max and min clock values
        
        if (AmdSmiWrapper.amdsmi_get_clock_info(_processorHandle, AmdSmiClockType.AMDSMI_CLK_TYPE_GFX, out var coreClockInfo) ==  AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            _defaultCoreMaxClockMhz = coreClockInfo.max_clk;
            _defaultCoreMinClockMhz = coreClockInfo.min_clk;
        }
        
        if (AmdSmiWrapper.amdsmi_get_clock_info(_processorHandle, AmdSmiClockType.AMDSMI_CLK_TYPE_MEM, out var memClockInfo) ==  AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
        {
            _defaultMemMaxClockMhz = memClockInfo.max_clk;
            _defaultMemMinClockMhz = memClockInfo.min_clk;
        }
        
        Console.WriteLine("core max: "+_defaultCoreMaxClockMhz);
        Console.WriteLine("core min: "+_defaultCoreMinClockMhz);
        Console.WriteLine("mem max: "+_defaultMemMaxClockMhz);
        Console.WriteLine("mem min: "+_defaultMemMinClockMhz);
    }
    

    public uint DeviceIndex { get; }
    public string DevicePciAddress { get; }
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

    private bool _supportsFanSpeedControl;

    private uint _defaultCoreMaxClockMhz;
    private uint _defaultCoreMinClockMhz;
    private uint _defaultMemMaxClockMhz;
    private uint _defaultMemMinClockMhz;
    
    private UInt64 _maxFanSpeed;
    
    public GpuMetrics GetMetrics()
    {
        Console.WriteLine ("clkinfo: "+     AmdSmiWrapper.amdsmi_get_clock_info(_processorHandle, AmdSmiClockType.AMDSMI_CLK_TYPE_GFX, out var coreClockInfo));
        Console.WriteLine ("meminfo: "+     AmdSmiWrapper.amdsmi_get_clock_info(_processorHandle, AmdSmiClockType.AMDSMI_CLK_TYPE_MEM, out var memClockInfo));
        Console.WriteLine ("vidclockinfo: "+AmdSmiWrapper.amdsmi_get_clock_info(_processorHandle, AmdSmiClockType.AMDSMI_CLK_TYPE_VCLK0, out var videoClockInfo));
        Console.WriteLine ("powercapinfo: "+AmdSmiWrapper.amdsmi_get_power_cap_info(_processorHandle,0, out var powerCapInfo));
        Console.WriteLine ("powerinfo: "+   AmdSmiWrapper.amdsmi_get_power_info(_processorHandle,out var powerInfo));
        Console.WriteLine ("vramusage: "+   AmdSmiWrapper.amdsmi_get_gpu_vram_usage(_processorHandle,out var vramUsageInfo));
        Console.WriteLine ("gpuactivity: "+ AmdSmiWrapper.amdsmi_get_gpu_activity(_processorHandle,out var gpuActivityInfo));
        Console.WriteLine ("tempmetric: "+  AmdSmiWrapper.amdsmi_get_temp_metric(_processorHandle,AmdsmiTemperatureType.AMDSMI_TEMPERATURE_TYPE_HOTSPOT,AmdsmiTemperatureMetric.AMDSMI_TEMP_CURRENT,out var currentTempInfo));
        Console.WriteLine ("fanspeed: "+    AmdSmiWrapper.amdsmi_get_gpu_fan_speed(_processorHandle,0,out var fanSpeed));
        
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
            GpuPState.GpuPstateUnknown,
            new GpuFansMetrics((fanSpeedPercent))

        );
    }
    
    
    public bool SetCoreTuning(GpuClockTune tuneSettings)
    {
        
        
        
        // if (Capabilities.CoreClockTuningMode )
        //
        // if (pState != GpuPState.GpuPstate0 || tuningMode != Capabilities.CoreClockTuningMode)
        //     return false;
        //
        // if (tuningValue <= 0)
        //     ResetGpuPerformanceLevel();

        return false;
    }

    public bool SetMemTuning(GpuClockTune tuneSettings)
    {
        // if (pState != GpuPState.GpuPstate0 || tuningMode != Capabilities.MemoryClockTuningMode)
        //     return false;
        //
        // if (tuningValue <= 0)
        //     ResetGpuPerformanceLevel();
        
        return false;
    }

    public bool SetGpuPowerLimit(uint limitMw)
    {
        if (limitMw < PowerLimitMinMw || limitMw > PowerLimitMaxMw)
            return false;

        ulong requestedLimitUWatt = limitMw * 1000UL;

        return AmdSmiWrapper.amdsmi_set_power_cap(_processorHandle, 0, requestedLimitUWatt) ==
               AmdsmiStatus.AMDSMI_STATUS_SUCCESS;

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
    

    public bool ApplyAutoSpeedToAllFans()
    {
        for (uint i = 0; i < FansCount; i++)
        {
            var status = AmdSmiWrapper.amdsmi_reset_gpu_fan(_processorHandle, i);

            if (status != AmdsmiStatus.AMDSMI_STATUS_SUCCESS)
                return false;
        }

        return true;
        
    }

    private bool ResetGpuPerformanceLevel()
    {
        return AmdSmiWrapper.amdsmi_set_gpu_perf_level(_processorHandle, AmdsmiDevPerfLevel.AMDSMI_DEV_PERF_LEVEL_AUTO) == AmdsmiStatus.AMDSMI_STATUS_SUCCESS;
    }
    
}