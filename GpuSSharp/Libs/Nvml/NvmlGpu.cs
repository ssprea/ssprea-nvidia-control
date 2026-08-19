using System.Text;
using GpuSSharp.Libs.Nvml.NvmlTypes;
using GpuSSharp.Types;

namespace GpuSSharp.Libs.Nvml;


    /// <summary>
    /// Encapsulates a GPU Device in way that a csharp user doesn't have
    /// to worry about Nvml native interop
    /// </summary>
    /// <remarks>
    /// To Use:
    /// 1. Call static nvmlInit before anything else
    /// 2. Use static GetDeviceCount to enumerate devices
    /// 3. Create an instance of NvGpu for each device
    /// 4. Call static nvmlShutdown() when done with all NvGpu instances
    /// GetDeviceCount is not guaranteed to enumerate devices in the same 
    /// order across reboots
    /// </remarks>
    public class NvmlGpu : IGpu
    {
        private const uint MAX_NAME_LENGTH = 100;

        private IntPtr _handle;

        public uint DeviceIndex { get; private set; }
        public string DevicePciAddress => PciInfo.busId.Substring(4,12);
        private NvmlPciInfo PciInfo { get; set; }
        public GpuVendor Vendor => GpuVendor.Nvidia;

        public GpuCapabilities Capabilities { get; } = new GpuCapabilities(GpuClockTuningMode.Offset, GpuClockTuningMode.Offset, true,true);

        private void ReadFixedProperties()
        {
            
            

            NvmlWrapper.nvmlDeviceGetPciInfo_v3(_handle, out var pci);
            PciInfo = pci;
            
            
            var powerLimitConstraints = GetPowerLimitConstraints();
            
            PowerLimitMinMw =  powerLimitConstraints.Item2;
            PowerLimitMaxMw =  powerLimitConstraints.Item3;
            PowerLimitDefaultMw = GetPowerLimitDefault().Item2;

            FansCount = GetFanCount().Item2;
            
            TemperatureThresholdShutdown = GetTemperatureThreshold(NvlmTemperatureThreshold.NVML_TEMPERATURE_THRESHOLD_SHUTDOWN).Item2;
            TemperatureThresholdSlowdown = GetTemperatureThreshold(NvlmTemperatureThreshold.NVML_TEMPERATURE_THRESHOLD_SLOWDOWN).Item2;
            TemperatureThresholdThrottle = GetTemperatureThreshold(NvlmTemperatureThreshold.NVML_TEMPERATURE_THRESHOLD_GPU_MAX).Item2;

            
        }
        
        #region Fixed Properties
        
        
        /// <summary>
        /// GPU Name
        /// </summary>
        public string Name { get; }


        // public uint GpuClockCurrent => GetCurrentClock(NvmlClockType.NVML_CLOCK_GRAPHICS).Item2;
        // public uint MemClockCurrent => GetCurrentClock(NvmlClockType.NVML_CLOCK_MEM).Item2;
        // public uint SmClockCurrent => GetCurrentClock(NvmlClockType.NVML_CLOCK_SM).Item2;
        // public uint VideoClockCurrent => GetCurrentClock(NvmlClockType.NVML_CLOCK_VIDEO).Item2;
    
        
        
        
        // public uint PowerLimitCurrentMw => GetPowerLimitCurrent().Item2;
        public uint PowerLimitMinMw { get; private set; }
        public uint PowerLimitMaxMw { get; private set; }
        public uint PowerLimitDefaultMw { get; private set; }
        
        public uint FansCount { get; private set; }
        
        
    
        // public ulong MemoryFree => GetMemoryUsage().Item2.Free;
        // public ulong MemoryUsed => GetMemoryUsage().Item2.Used;

        // public double MemoryTotalMB => MemoryTotal / 1000000f;
        // public double MemoryFreeMB => MemoryFree / 1000000f;
        // public double MemoryUsedMB => MemoryUsed / 1000000f;
        

        
    
        public uint TemperatureThresholdShutdown { get; private set; }
        public uint TemperatureThresholdSlowdown { get; private set; }
        public uint TemperatureThresholdThrottle { get; private set; }

        
        #endregion
        /// <summary>
        /// Initializes a new instance of NvGpu, using device index
        /// to initialize handle and name for the device
        /// </summary>
        /// <param name="deviceIdx">device index</param>
        public NvmlGpu(uint deviceIdx)
        {
            DeviceIndex = deviceIdx;
            var r = NvmlWrapper.nvmlDeviceGetHandleByIndex(deviceIdx, out _handle);
            if(r != NvmlReturnCode.NVML_SUCCESS)
            {
                throw new Exception($"Unable to get device by handle: {r.ToString()}");
            }

            
            var name = new StringBuilder();
            r = NvmlWrapper.nvmlDeviceGetName(_handle, name, MAX_NAME_LENGTH);
            if(r != NvmlReturnCode.NVML_SUCCESS)
            {
                throw new Exception($"Unable to get device name: {r.ToString()}");
            }

            Name = name.ToString();
            // Console.WriteLine(DevicePciAddress);
            
            
            
            //Read fixed values
            ReadFixedProperties();
        }

        public GpuMetrics GetMetrics()
        {
            var memoryInfo = GetMemoryUsage().Item2;
            
            var utilizationInfo =  GetUtilization().Item2;
            
            return new GpuMetrics(
                GetCurrentClock(NvmlClockType.NVML_CLOCK_GRAPHICS).Item2,
                GetCurrentClock(NvmlClockType.NVML_CLOCK_MEM).Item2,
                GetCurrentClock(NvmlClockType.NVML_CLOCK_SM).Item2,
                GetCurrentClock(NvmlClockType.NVML_CLOCK_VIDEO).Item2,
                
                GetPowerLimitCurrent().Item2,
                GetPowerUsage().Item2,
                
                memoryInfo.Free,
                memoryInfo.Used,
                memoryInfo.Total,
                
                utilizationInfo.gpu,
                utilizationInfo.memory,
                
                GetTemperature().Item2,
                ConvertPState(GetPState().Item2),
                
                new GpuFansMetrics(GetFansSpeeds())
                
                );
        }
        
        
    
        // public double GpuTemperature => GetTemperature().Item2;
        // public uint GpuPowerUsage => GetPowerUsage().Item2;
        

        // public uint Fan0SpeedPercent { get; }


        /// <summary>
        /// Gets device utilization info
        /// </summary>
        /// <returns>utilization info and nvml return code</returns>
        public (NvmlReturnCode, NvmlUtilization) GetUtilization()
        {
            var r = NvmlWrapper.nvmlDeviceGetUtilizationRates(_handle, out NvmlUtilization u);
            return (r,u);
        }

        private static GpuPState ConvertPState(NvmlPStates pstate)
        {
            return  (GpuPState)pstate;
        }
        
        private uint[] GetFansSpeeds()
        {
            var fans = GetFansCount();
            
            var fansSpeeds = new uint[fans.Item2];

            for (uint i = 0; i < fans.Item2; i++)
            {
                fansSpeeds[i] = GetFanCurrentSpeed(i).Item2;
                
            }

            return fansSpeeds.Length > 0 ? fansSpeeds : [0];
        }

        public bool SetGpuPowerLimit(uint limitMw)
        {
            return SetPowerLimit(limitMw) == NvmlReturnCode.NVML_SUCCESS;
        }

        public bool ApplySpeedToAllFans(uint speed)
        {
            bool result = true;
            for(uint i = 0; i < FansCount; i++)
                result &= (SetFanSpeed(i,speed) ==  NvmlReturnCode.NVML_SUCCESS);
            return result;
        }

        public bool ApplyAutoSpeedToAllFans()
        {
            return ApplyPolicyToAllFans(NvmlFanControlPolicy.NVML_FAN_POLICY_TEMPERATURE_CONTINOUS_SW);
        }

        public (NvmlReturnCode,uint) GetFansCount()
        {
            var r = NvmlWrapper.nvmlDeviceGetNumFans(_handle, out uint numFans);
            return (r, numFans);
        }

        public bool ApplyPolicyToAllFans(NvmlFanControlPolicy policy)
        {
            bool result = true;
            for (uint i = 0; i < FansCount; i++)
                result &= (SetFanControlPolicy(i, policy) == NvmlReturnCode.NVML_SUCCESS);
            return result;
        }
        
        
        /// <summary>
        /// Gets device temperature in degrees celsius
        /// </summary>
        /// <returns>device temperature and nvml return code</returns>
        public (NvmlReturnCode,uint) GetTemperature()
        {
            var r = NvmlWrapper.nvmlDeviceGetTemperature(_handle, NvmlTemperatureSensors.NVML_TEMPERATURE_GPU, out uint t);
            return (r,t);
        }
        
        
        public (NvmlReturnCode,NvmlMemory) GetMemoryUsage()
        {
            var r = NvmlWrapper.nvmlDeviceGetMemoryInfo(_handle, out NvmlMemory m);
            return (r,m);
        }

        public (NvmlReturnCode,NvmlPStates) GetPState()
        {
            var r = NvmlWrapper.nvmlDeviceGetPerformanceState(_handle, out NvmlPStates p);
            return (r,p);
        }

        public (NvmlReturnCode,NvmlClockOffset_v1) GetClockOffset(NvmlClockType clockType, NvmlPStates pState)
        {
            var clockOffset = new NvmlClockOffset_v1()
            {
                Type = clockType,
                PState = pState
            };

            var r = NvmlWrapper.nvmlDeviceGetClockOffsets(_handle, ref clockOffset);
            return (r,clockOffset);
        }
        
        public (NvmlReturnCode, uint) GetCurrentClock(NvmlClockType type)
        {
           

            var r = NvmlWrapper.nvmlDeviceGetClockInfo(_handle, type,out uint c);
            return (r,c);
        }
        
        public NvmlReturnCode SetClockOffset(NvmlClockType clockType, GpuPState pState, int clockOffsetMhz)
        {
            var clockOffset = new NvmlClockOffset_v1()
            {
                Type = clockType,
                PState = (NvmlPStates)pState,
                ClockOffsetMHz = clockOffsetMhz
            };

            return NvmlWrapper.nvmlDeviceSetClockOffsets(_handle, ref clockOffset);
        }

        public bool SetCoreTuning(GpuClockTune tuneSettings)
        {
            return tuneSettings switch
            {
                GpuClockTune.Offset offset =>
                    SetClockOffset(NvmlClockType.NVML_CLOCK_GRAPHICS, offset.PState, offset.OffsetMhz) == NvmlReturnCode.NVML_SUCCESS,

                _ => false
            };
            
        }
        
        public bool SetMemTuning(GpuClockTune tuneSettings)
        {
            return tuneSettings switch
            {
                GpuClockTune.Offset offset =>
                    SetClockOffset(NvmlClockType.NVML_CLOCK_MEM, offset.PState, offset.OffsetMhz) == NvmlReturnCode.NVML_SUCCESS,

                _ => false
            };
        }

        public (NvmlReturnCode,uint) GetPowerLimitCurrent()
        {
            return (NvmlWrapper.nvmlDeviceGetPowerManagementLimit(_handle, out uint limit),limit);
        }

        public (NvmlReturnCode, uint,uint) GetPowerLimitConstraints()
        {
            return (NvmlWrapper.nvmlDeviceGetPowerManagementLimitConstraints(_handle, out uint minLimit, out uint maxLimit),minLimit,maxLimit); 
        }
        
        public (NvmlReturnCode,uint) GetPowerLimitDefault()
        {
            return (NvmlWrapper.nvmlDeviceGetPowerManagementDefaultLimit(_handle, out uint limit),limit);
        }
        
        public (NvmlReturnCode,uint) GetPowerUsage()
        {
            return (NvmlWrapper.nvmlDeviceGetPowerUsage(_handle, out uint power),power);
        }

        

        public NvmlReturnCode SetPowerLimit(uint limitMw)
        {
            return NvmlWrapper.nvmlDeviceSetPowerManagementLimit(_handle,limitMw);
        }

        public NvmlReturnCode SetFanControlPolicy(uint fanId,NvmlFanControlPolicy policy)
        {
            return(NvmlWrapper.nvmlDeviceSetFanControlPolicy(_handle,fanId,policy));
        }
        
        public NvmlReturnCode SetFanSpeed(uint fanId,uint speed)
        {
            return(NvmlWrapper.nvmlDeviceSetFanSpeed_v2(_handle,fanId,speed));
        }

        public (NvmlReturnCode,uint) GetFanCount()
        {
            return(NvmlWrapper.nvmlDeviceGetNumFans(_handle, out uint fanCount),fanCount);
        }
        
        public (NvmlReturnCode,uint) GetFanTargetSpeed(uint fanId)
        {
            return(NvmlWrapper.nvmlDeviceGetTargetFanSpeed(_handle, fanId,out uint tSpeed),tSpeed);
        }
        
        public (NvmlReturnCode,uint) GetFanCurrentSpeed(uint fanId)
        {
            return(NvmlWrapper.nvmlDeviceGetFanSpeed_v2(_handle, fanId,out uint speed),speed);
        }
        
        public (NvmlReturnCode,uint) GetTemperatureThreshold(NvlmTemperatureThreshold temperatureThresholdType)
        {
            return(NvmlWrapper.nvmlDeviceGetTemperatureThreshold(_handle,temperatureThresholdType,out uint temperatureThreshold),temperatureThreshold);
        }
        
    }