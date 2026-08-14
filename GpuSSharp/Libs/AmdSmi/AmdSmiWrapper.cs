using System.Runtime.InteropServices;
using GpuSSharp.Libs.AmdSmi.AmdSmiTypes;

namespace GpuSSharp.Libs.AmdSmi;

public static class AmdSmiWrapper
{
    //AMD SMI LIB - VERSION 26.5.0
    private const string AMDSMI_DLL = "libamd_smi.so";
    
    public static bool IsAmdSmiLibPresent()
    {
        return NativeLibrary.TryLoad(AMDSMI_DLL, out var lib);
    }
    
    /// <summary>
    /// Initializes AmdSmi, needs to be called before any other functions.
    /// </summary>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_init(AmdSmiInitFlags initFlags);
    
    /// <summary>
    /// Shuts down AMDSmi, needs to be called before exiting program.
    /// </summary>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_shut_down();

    /// <summary>
    /// Get available socket handles.
    /// </summary>
    /// <param name="socketCount">Max number of sockets to read. If socketHandles is null, socketCount returns the available sockets count. </param>
    /// <param name="socketHandles">IntPtr array that will be populated with socket handles</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_socket_handles(ref uint socketCount, [Out] IntPtr[]? socketHandles);


    /// <summary>
    /// Gets processors associated to a socket.
    /// </summary>
    /// <param name="socketHandle">Handle of the socket from wich to read processors.  </param>
    /// <param name="processorCount">Number of processors to read. If processorHandles is null, processorCount returns the available sockets count.</param>
    /// <param name="processorHandles">IntPtr array that will be populated with processorHandles</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_processor_handles(IntPtr socketHandle, ref UInt32 processorCount,
        [Out] IntPtr[]? processorHandles);
    
    /// <summary>
    /// Gets BDF info of processor.
    /// </summary>
    /// <param name="processorHandle"> processor handle  </param>
    /// <param name="bdf">BDF info struct</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_gpu_device_bdf(IntPtr processorHandle, out AmdsmiBdf bdf);
    
    /// <summary>
    /// Gets board info of processor.
    /// </summary>
    /// <param name="processorHandle"> processor handle  </param>
    /// <param name="info">Board info struct.</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_gpu_board_info(IntPtr processorHandle, out AmdsmiBoardInfo info);

    
    /// <summary>
    /// Gets power cap info.
    /// </summary>
    /// <param name="processorHandle"> processor handle  </param>
    /// <param name="sensorInd">Sensor index, normally this will be 0.</param>
    /// <param name="info">Power cap info struct.</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_power_cap_info(IntPtr processorHandle, UInt32 sensorInd, out AmdsmiPowerCapInfo info);
    

    
    
    
    
    #region GPU METRICS
    
    /// <summary>
    /// Gets GPU engine usage .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="info">AmdsmiEngineUsage specifying device usage.</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_gpu_activity(IntPtr processorHandle, out AmdsmiEngineUsage info);

    /// <summary>
    /// Gets GPU power usage information .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="info">AmdsmiEngineUsage specifying device usage.</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_power_info(IntPtr processorHandle, out AmdsmiPowerInfo info);
    
    /// <summary>
    /// Gets temperature metrics.
    /// </summary>
    /// <param name="processorHandle"> processor handle  </param>
    /// <param name="sensorType">Sensor type.</param>
    /// <param name="metric">metric.</param>
    /// <param name="temperature">Resulting temperature in degrees celsius.</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_temp_metric(IntPtr processorHandle, AmdsmiTemperatureType sensorType, AmdsmiTemperatureMetric metric, out UInt64 temperature);
    
    /// <summary>
    /// Gets GPU clocks information .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="clkType">type of clock to query.  </param>
    /// <param name="info">AmdsmiEngineUsage specifying device usage.</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_clock_info(IntPtr processorHandle, AmdSmiClockType clkType, out AmdsmiClockInfo info);
    
    
    /// <summary>
    /// Gets GPU busy percent .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="busyPercent">Busy%.</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_gpu_busy_percent(IntPtr processorHandle,  out UInt32 busyPercent);
    
    /// <summary>
    /// Gets GPU performance level .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="perfLevel">Current performance level</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_gpu_perf_level(IntPtr processorHandle, out AmdsmiDevPerfLevel perfLevel);
    
    /// <summary>
    /// Gets GPU power management status .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="enabled">Is power management enabled</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_is_gpu_power_management_enabled(IntPtr processorHandle, out bool enabled);
    
    /// <summary>
    /// Gets GPU VRAM usage .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="info">Vram usage struct containing info</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_gpu_vram_usage(IntPtr processorHandle, out AmdsmiVramUsage info);
    
    /// <summary>
    /// Gets GPU fan speed .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="sensorInd">Sensor index, normally 0</param>
    /// <param name="speed">Speed relative to fan speed max</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_gpu_fan_speed(IntPtr processorHandle,UInt32 sensorInd, out UInt64 speed);
    
    /// <summary>
    /// Gets max GPU fan speed .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="sensorInd">Sensor index, normally 0</param>
    /// <param name="maxSpeed">Fan speed max</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_get_gpu_fan_speed_max(IntPtr processorHandle,UInt32 sensorInd, out UInt64 maxSpeed);
    
    #endregion
    
    #region SETTERS
    
    /// <summary>
    /// Sets GPU Power Limit .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="sensorInd">Sensor index, normally 0</param>
    /// <param name="cap">Requested power cap, must be between power cap max and min</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_set_power_cap(IntPtr processorHandle,UInt32 sensorInd, UInt64 cap);
    
    /// <summary>
    /// Sets GPU static fan speed for all fans .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="sensorInd">Sensor index, normally 0</param>
    /// <param name="speed">Requested fan speed, must be between 0 and 255</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_set_gpu_fan_speed(IntPtr processorHandle,UInt32 sensorInd, UInt64 speed);
    
    /// <summary>
    /// Resets GPU fans to automatic fan speed set by GPU bios .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="sensorInd">Sensor index, normally 0</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_reset_gpu_fan(IntPtr processorHandle,UInt32 sensorInd);
    
    /// <summary>
    /// Sets GPU clock limits .
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="clkType">Clock type</param>
    /// <param name="limitType">Limit type, min or max</param>
    /// <param name="clkValueMhz">Desired clock value in MHz</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_set_gpu_clk_limit(IntPtr processorHandle,AmdSmiClockType clkType, AmdsmiClkLimitType limitType, UInt64 clkValueMhz);
    
    /// <summary>
    /// Sets GPU performance level.
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="perfLevel">Performance level</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_set_gpu_perf_level(IntPtr processorHandle,AmdsmiDevPerfLevel perfLevel);
    
    
    /// <summary>
    /// Sets GPU performance level.
    /// </summary>
    /// <param name="processorHandle">processor handle.  </param>
    /// <param name="overdriveLevel">Overdrive level 0-20</param>
    /// <returns> Operation result status </returns>
    [DllImport(AMDSMI_DLL)]
    public static extern AmdsmiStatus amdsmi_set_gpu_overdrive_level(IntPtr processorHandle,UInt32 overdriveLevel);
    
    
    #endregion
}