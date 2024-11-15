using McMaster.Extensions.CommandLineUtils;

namespace ssprea_nvidia_control_cli;

public class Program
{
    [Option(CommandOptionType.SingleValue,Description = "set core offset mHz", LongName = "coreOffset")]
    public int CoreOffset { get; set; }
        
    [Option(CommandOptionType.SingleValue, Description = "set mem offset mHz", LongName = "memoryOffset")]
    public int MemoryOffset { get; set; }
    
    
    public static void Main(string[] args)
    {
        
        
    }
}