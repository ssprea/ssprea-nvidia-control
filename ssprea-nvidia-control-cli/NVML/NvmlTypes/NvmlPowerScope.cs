namespace ssprea_nvidia_control_cli.NVML.NvmlTypes;

public enum NvmlPowerScope
{
    L_POWER_SCOPE_GPU =   0,   //!< Targets only GPU
    NVML_POWER_SCOPE_MODULE = 1,    //!< Targets the whole module
    NVML_POWER_SCOPE_MEMORY = 2,    //< Targets the GPU Memory
}