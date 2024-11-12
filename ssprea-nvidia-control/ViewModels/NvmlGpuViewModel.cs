using ssprea_nvidia_control.NVML;

namespace ssprea_nvidia_control.ViewModels;

public class NvmlGpuViewModel : ViewModelBase
{
    //  LA CLASSE BASE DEVE AVERE SOLO I METODI O LE PROPRIETÀ CHE ESEGUONO IL METODO DIRETTAMENTE, QUA INVECE CI DEVONO ESSERE DELLE PROPRIETÀ NUMERICHE CHE OGNI TOT VENGONO AGGIORNATE ESEGUENDO I METODI DELLA CLASSE BASE COSÌ LA UI SI AGGIORNA DA SOLA PER BENE
    private readonly NvmlGpu _nvmlGpu;

    public NvmlGpuViewModel(NvmlGpu nvmlGpu)
    {
        _nvmlGpu = nvmlGpu;
    }
    
    public uint GpuTemperature { private set; get; }
    public uint GpuPowerUsage { private set; get; }
    
    
}