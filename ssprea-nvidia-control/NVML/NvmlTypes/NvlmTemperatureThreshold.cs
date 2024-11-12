namespace ssprea_nvidia_control.NVML.NvmlTypes;

public enum NvlmTemperatureThreshold
{
    NVML_TEMPERATURE_THRESHOLD_SHUTDOWN      = 0,   // Temperature at which the GPU will
                                                    // shut down for HW protection
                                                    
    NVML_TEMPERATURE_THRESHOLD_SLOWDOWN      = 1,   // Temperature at which the GPU will
                                                    // begin HW slowdown
                                                    
    NVML_TEMPERATURE_THRESHOLD_MEM_MAX       = 2,   // Memory Temperature at which the GPU will
                                                    // begin SW slowdown
                                                    
    NVML_TEMPERATURE_THRESHOLD_GPU_MAX       = 3,   // GPU Temperature at which the GPU
                                                    // can be throttled below base clock
                                                    
    NVML_TEMPERATURE_THRESHOLD_ACOUSTIC_MIN  = 4,   // Minimum GPU Temperature that can be
                                                    // set as acoustic threshold
                                                    
    NVML_TEMPERATURE_THRESHOLD_ACOUSTIC_CURR = 5,   // Current temperature that is set as
                                                    // acoustic threshold.
                                                    
    NVML_TEMPERATURE_THRESHOLD_ACOUSTIC_MAX  = 6,   // Maximum GPU temperature that can be
                                                    // set as acoustic threshold.
                                                    
    NVML_TEMPERATURE_THRESHOLD_GPS_CURR      = 7,   // Current temperature that is set as
                                                    // gps threshold.
    // Keep this last
    NVML_TEMPERATURE_THRESHOLD_COUNT
}