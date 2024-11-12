using System;
using System.Runtime.InteropServices;
using System.Text;
using ssprea_nvidia_control.NVML.NvmlTypes;

namespace ssprea_nvidia_control.NVML;



public static class NvmlWrapper
{
    
#if LINUX
    private const string NVML_DLL = "libnvidia-ml.so";
#elif WINDOWS
    private const string NVML_DLL = "nvml.dll";
#endif
    

    /// <summary>
    /// Initializes Nvml
    /// </summary>
    /// <returns>
    /// NVML_SUCCESS if NVML has been properly initialized
    /// NVML_ERROR_DRIVER_NOT_LOADED if NVIDIA driver is not running
    /// NVML_ERROR_NO_PERMISSION if NVML does not have permission to talk to the driver
    /// NVML_ERROR_UNKNOWN on any unexpected error
    /// </returns>
    /// <remarks>
    /// Needs to be called before making any other nvml calls. Reference counted,
    /// nvml shutdown only occurs when reference count hits 0
    /// </remarks>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlInit();

    /// <summary>
    /// Shuts down Nvml
    /// </summary>
    /// <returns>
    /// NVML_SUCCESS if NVML has been properly shut down
    /// NVML_ERROR_UNINITIALIZED if the library has not been successfully initialized
    /// NVML_ERROR_UNKNOWN on any unexpected error
    /// </returns>
    /// <remarks>
    /// Reference counted, nvml shutdown only occurs when reference count hits 0
    /// </remarks>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlShutdown();

    /// <summary>
    /// Queries nvml for GPU device count
    /// </summary>
    /// <param name="deviceCount">out parameter containing device count</param>
    /// <returns>
    /// NVML_SUCCESS if deviceCount has been set
    /// NVML_ERROR_UNINITIALIZED if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT if deviceCount is NULL
    /// NVML_ERROR_UNKNOWN on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetCount(out uint deviceCount);

    /// <summary>
    /// Queries device for name
    /// </summary>
    /// <param name="device">Device handle</param>
    /// <param name="name">"out" parameter containing the device name</param>
    /// <param name="length">maximum length of the string returned by name</param>
    /// <returns>
    /// NVML_SUCCESS if name has been set
    /// NVML_ERROR_UNINITIALIZED if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT if device is invalid, or name is NULL
    /// NVML_ERROR_INSUFFICIENT_SIZE if length is too small
    /// NVML_ERROR_GPU_IS_LOST if the target GPU has fallen off the bus or is otherwise inaccessible
    /// NVML_ERROR_UNKNOWN on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetName(IntPtr device, [MarshalAs(UnmanagedType.LPStr)] StringBuilder name, uint length);

    /// <summary>
    /// Queries device handle by index
    /// </summary>
    /// <param name="index">Device index</param>
    /// <param name="device">out parameter for device handle</param>
    /// <returns>
    /// NVML_SUCCESS if device has been set
    /// NVML_ERROR_UNINITIALIZED if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT if index is invalid or device is NULL
    /// NVML_ERROR_INSUFFICIENT_POWER if any attached devices have improperly attached external power cables
    /// NVML_ERROR_NO_PERMISSION if the user doesn't have permission to talk to this device
    /// NVML_ERROR_IRQ_ISSUE if NVIDIA kernel detected an interrupt issue with the attached GPUs
    /// NVML_ERROR_GPU_IS_LOST if the target GPU has fallen off the bus or is otherwise inaccessible
    /// NVML_ERROR_UNKNOWN on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetHandleByIndex(uint index, out IntPtr device);

    /// <summary>
    /// Queries temperature of the device
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="sensorType">sensor type, api currently only supports one value here</param>
    /// <param name="temp">out parameter containing gpu temperature</param>
    /// <returns>
    /// NVML_SUCCESS if temp has been set
    /// NVML_ERROR_UNINITIALIZED if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT if device is invalid, sensorType is invalid or temp is NULL
    /// NVML_ERROR_NOT_SUPPORTED if the device does not have the specified sensor
    /// NVML_ERROR_GPU_IS_LOST if the target GPU has fallen off the bus or is otherwise inaccessible
    /// NVML_ERROR_UNKNOWN on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetTemperature(IntPtr device, NvmlTemperatureSensors sensorType, out uint temp);

    /// <summary>
    /// Queries device utilization information
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="utilization">out parameter containing utilization info</param>
    /// <returns>
    /// NVML_SUCCESS if utilization has been populated
    /// NVML_ERROR_UNINITIALIZED if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT if device is invalid or utilization is NULL
    /// NVML_ERROR_NOT_SUPPORTED if the device does not support this feature
    /// NVML_ERROR_GPU_IS_LOST if the target GPU has fallen off the bus or is otherwise inaccessible
    /// NVML_ERROR_UNKNOWN on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetUtilizationRates(IntPtr device, out NvmlUtilization utilization);
    
    
    /// <summary>
    /// Queries min, max and current clock offset of some clock domain for a given PState
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="info">Structure specifying the clock type (input) and the pstate (input) retrieved clock offset value (output), min clock offset (output) and max clock offset (output)</param>
    /// <returns>
    /// NVML_SUCCESS                         if everything worked
    /// NVML_ERROR_UNINITIALIZED             if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT          if device, type or pstate are invalid or both minClockOffsetMHz and maxClockOffsetMHz are NULL
    /// NVML_ERROR_ARGUMENT_VERSION_MISMATCH if the provided version is invalid/unsupported
    /// NVML_ERROR_NOT_SUPPORTED             if the device does not support this feature
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetClockOffsets(IntPtr device, ref NvmlClockOffset_v1 info);
    
    
    /// <summary>
    /// Control current clock offset of some clock domain for a given PState
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="info">Structure specifying the clock type (input), the pstate (input) and clock offset value (input)</param>
    /// <returns>
    /// NVML_SUCCESS                         if everything worked
    /// NVML_ERROR_UNINITIALIZED             if the library has not been successfully initialized
    /// NVML_ERROR_NO_PERMISSION             if the user doesn't have permission to perform this operation
    /// NVML_ERROR_INVALID_ARGUMENT          if device, type or pstate are invalid or both clockOffsetMHz is out of allowed range.
    /// NVML_ERROR_ARGUMENT_VERSION_MISMATCH if the provided version is invalid/unsupported
    /// NVML_ERROR_NOT_SUPPORTED             if the device does not support this feature
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceSetClockOffsets(IntPtr device, ref NvmlClockOffset_v1 info);
    
    
    /// <summary>
    /// Queries PState of the device
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="pState">out parameter containing the device's current PState</param>
    /// <returns>
    /// NVML_SUCCESS                 if a pState has been set
    /// NVML_ERROR_UNINITIALIZED     if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT  if \a device is invalid or \a pState is NULL
    /// NVML_ERROR_NOT_SUPPORTED     if the device does not support this feature
    /// NVML_ERROR_GPU_IS_LOST       if the target GPU has fallen off the bus or is otherwise inaccessible
    /// NVML_ERROR_UNKNOWN           on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetPerformanceState(IntPtr device, out NvmlPStates pState);
    
    /// <summary>
    /// Retrieves the current clock speeds for the device
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="type">Identify which clock domain to query</param>
    /// <param name="clock">Reference in which to return the clock speed in MHz</param>
    /// <returns>
    ///  NVML_SUCCESS                 if clock has been set
    ///  NVML_ERROR_UNINITIALIZED     if the library has not been successfully initialized
    ///  NVML_ERROR_INVALID_ARGUMENT  if device is invalid or clock is NULL
    ///  NVML_ERROR_NOT_SUPPORTED     if the device cannot report the specified clock
    ///  NVML_ERROR_GPU_IS_LOST       if the target GPU has fallen off the bus or is otherwise inaccessible
    ///  NVML_ERROR_UNKNOWN           on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetClockInfo(IntPtr device, NvmlClockType type, out uint clock);
    
    /// <summary>
    /// Retrieves the power management limit associated with this device.
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="limit">Reference in which to return the power management limit in milliwatts</param>
    /// <returns>
    /// NVML_SUCCESS                 if \a limit has been set
    /// NVML_ERROR_UNINITIALIZED     if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT  if \a device is invalid or \a limit is NULL
    /// NVML_ERROR_NOT_SUPPORTED     if the device does not support this feature
    /// NVML_ERROR_GPU_IS_LOST       if the target GPU has fallen off the bus or is otherwise inaccessible
    /// NVML_ERROR_UNKNOWN           on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetPowerManagementLimit(IntPtr device, out uint limit);
    
    /// <summary>
    /// Retrieves power usage for this GPU in milliwatts and its associated circuitry (e.g. memory)
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="power">Reference in which to return the power usage information</param>
    /// <returns>
    /// NVML_SUCCESS                 if \a power has been populated
    /// NVML_ERROR_UNINITIALIZED     if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT  if \a device is invalid or \a power is NULL
    /// NVML_ERROR_NOT_SUPPORTED     if the device does not support power readings
    /// NVML_ERROR_GPU_IS_LOST       if the target GPU has fallen off the bus or is otherwise inaccessible
    /// NVML_ERROR_UNKNOWN           on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetPowerUsage(IntPtr device, out uint power);
    
    /// <summary>
    /// Retrieves information about possible values of power management limits on this device.
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="minLimit">Reference in which to return the minimum power management limit in milliwatts</param>
    /// <param name="maxLimit">Reference in which to return the maximum power management limit in milliwatts</param>
    /// <returns>
    /// NVML_SUCCESS                 if \a minLimit and \a maxLimit have been set
    /// NVML_ERROR_UNINITIALIZED     if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT  if \a device is invalid or \a minLimit or \a maxLimit is NULL
    /// NVML_ERROR_NOT_SUPPORTED     if the device does not support this feature
    /// NVML_ERROR_GPU_IS_LOST       if the target GPU has fallen off the bus or is otherwise inaccessible
    /// NVML_ERROR_UNKNOWN           on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetPowerManagementLimitConstraints(IntPtr device, out uint minLimit, out uint maxLimit);
    
    /// <summary>
    /// Retrieves default power management limit on this device, in milliwatts.
    /// Default power management limit is a power management limit that the device boots with.
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="defaultLimit">Reference in which to return the default power management limit in milliwatts</param>
    /// <returns>
    ///  NVML_SUCCESS                 if \a defaultLimit has been set
    ///  NVML_ERROR_UNINITIALIZED     if the library has not been successfully initialized
    ///  NVML_ERROR_INVALID_ARGUMENT  if \a device is invalid or \a defaultLimit is NULL
    ///  NVML_ERROR_NOT_SUPPORTED     if the device does not support this feature
    ///  NVML_ERROR_GPU_IS_LOST       if the target GPU has fallen off the bus or is otherwise inaccessible
    ///  NVML_ERROR_UNKNOWN           on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetPowerManagementDefaultLimit(IntPtr device, out uint defaultLimit);
    
    /// <summary>
    /// Sets current fan control policy.
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="fan">The index of the fan, starting at zero</param>
    /// <param name="policy">The fan control policy to set</param>
    /// <returns>
    /// NVML_SUCCESS                 if \a policy has been set
    /// NVML_ERROR_UNINITIALIZED     if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT  if \a device is invalid or \a policy is null or the \a fan given doesn't reference
    ///                                    a fan that exists.
    /// NVML_ERROR_NOT_SUPPORTED     if the \a device is older than Maxwell
    /// NVML_ERROR_UNKNOWN           on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceSetFanControlPolicy(IntPtr device,uint fan, NvmlFanControlPolicy policy);
    
    /// <summary>
    /// Retrieves the number of fans on the device.
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="numFans">The number of fans</param>
    /// <returns>
    ///  NVML_SUCCESS                 if \a fan number query was successful
    ///  NVML_ERROR_UNINITIALIZED     if the library has not been successfully initialized
    ///  NVML_ERROR_INVALID_ARGUMENT  if \a device is invalid or \a numFans is NULL
    ///  NVML_ERROR_NOT_SUPPORTED     if the device does not have a fan
    ///  NVML_ERROR_GPU_IS_LOST       if the target GPU has fallen off the bus or is otherwise inaccessible
    ///  NVML_ERROR_UNKNOWN           on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetNumFans(IntPtr device, out uint numFans);
    
    /// <summary>
    /// Retrieves the intended target speed of the device's specified fan.
    /// Normally, the driver dynamically adjusts the fan based on
    /// the needs of the GPU.  But when user set fan speed using nvmlDeviceSetFanSpeed_v2,
    /// the driver will attempt to make the fan achieve the setting in
    /// nvmlDeviceSetFanSpeed_v2.  The actual current speed of the fan
    /// is reported in nvmlDeviceGetFanSpeed_v2.
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="fan">The index of the target fan, zero indexed.</param>
    /// <param name="targetSpeed">Reference in which to return the fan speed percentage</param>
    /// <returns>
    ///  NVML_SUCCESS                   if \a speed has been set
    ///  NVML_ERROR_UNINITIALIZED       if the library has not been successfully initialized
    ///  NVML_ERROR_INVALID_ARGUMENT    if \a device is invalid, \a fan is not an acceptable index, or \a speed is NULL
    ///  NVML_ERROR_NOT_SUPPORTED       if the device does not have a fan or is newer than Maxwell
    ///  NVML_ERROR_GPU_IS_LOST         if the target GPU has fallen off the bus or is otherwise inaccessible
    ///  NVML_ERROR_UNKNOWN             on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetTargetFanSpeed(IntPtr device, uint fan, out uint targetSpeed);
    
    /// <summary>
    /// Retrieves the intended operating speed of the device's specified fan.
    /// Note: The reported speed is the intended fan speed. If the fan is physically blocked and unable to spin, the
    /// output will not match the actual fan speed.
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="fan">The index of the target fan, zero indexed.</param>
    /// <param name="speed">Reference in which to return the fan speed percentage</param>
    /// <returns>
    /// NVML_SUCCESS                   if \a speed has been set
    /// NVML_ERROR_UNINITIALIZED       if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT    if \a device is invalid, \a fan is not an acceptable index, or \a speed is NULL
    /// NVML_ERROR_NOT_SUPPORTED       if the device does not have a fan or is newer than Maxwell
    /// NVML_ERROR_GPU_IS_LOST         if the target GPU has fallen off the bus or is otherwise inaccessible
    /// NVML_ERROR_UNKNOWN             on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetFanSpeed_v2(IntPtr device, uint fan, out uint speed);
    
    /// <summary>
    ///Sets the speed of a specified fan.
    /// WARNING: This function changes the fan control policy to manual. It means that YOU have to monitor
    /// the temperature and adjust the fan speed accordingly.
    /// If you set the fan speed too low you can burn your GPU!
    ///  Use nvmlDeviceSetDefaultFanSpeed_v2 to restore default control policy.
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="fan">The index of the fan, starting at zero</param>
    /// <param name="speed">The target speed of the fan [0-100] in % of max speed</param>
    /// <returns>
    /// NVML_SUCCESS                 if \a offset has been set
    /// NVML_ERROR_UNINITIALIZED     if the library has not been successfully initialized
    /// NVML_ERROR_INVALID_ARGUMENT  if \a device is invalid or \a offset is NULL
    /// NVML_ERROR_NOT_SUPPORTED     if the device does not support this feature
    /// NVML_ERROR_GPU_IS_LOST       if the target GPU has fallen off the bus or is otherwise inaccessible
    /// NVML_ERROR_UNKNOWN           on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceSetFanSpeed_v2(IntPtr device, uint fan, uint speed);
    
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceSetPowerManagementLimit_v2(IntPtr device, NvmlPowerValue_v2 powerValue);
    
    
    /// <summary>
    /// Set new power limit of this device.
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="limit">Power management limit in milliwatts to set</param>
    /// <returns>
    ///  NVML_SUCCESS                 if \a limit has been set
    ///  NVML_ERROR_UNINITIALIZED     if the library has not been successfully initialized
    ///  NVML_ERROR_INVALID_ARGUMENT  if \a device is invalid or \a defaultLimit is out of range
    ///  NVML_ERROR_NOT_SUPPORTED     if the device does not support this feature
    ///  NVML_ERROR_GPU_IS_LOST       if the target GPU has fallen off the bus or is otherwise inaccessible
    ///  NVML_ERROR_UNKNOWN           on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceSetPowerManagementLimit(IntPtr device, uint limit);
    
    /// <summary>
    /// Retrieves the temperature threshold for the GPU with the specified threshold type in degrees C.
    /// </summary>
    /// <param name="device">device handle</param>
    /// <param name="thresholdType">The type of threshold value queried</param>
    /// <param name="temp">Reference in which to return the temperature reading</param>
    /// <returns>
    ///  NVML_SUCCESS                 if \a temp has been set
    ///  NVML_ERROR_UNINITIALIZED     if the library has not been successfully initialized
    ///  NVML_ERROR_INVALID_ARGUMENT  if \a device is invalid, \a thresholdType is invalid or \a temp is NULL
    ///  NVML_ERROR_NOT_SUPPORTED     if the device does not have a temperature sensor or is unsupported
    ///  NVML_ERROR_GPU_IS_LOST       if the target GPU has fallen off the bus or is otherwise inaccessible
    ///  NVML_ERROR_UNKNOWN           on any unexpected error
    /// </returns>
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetTemperatureThreshold(IntPtr device, NvlmTemperatureThreshold thresholdType, out uint temp);
    
}