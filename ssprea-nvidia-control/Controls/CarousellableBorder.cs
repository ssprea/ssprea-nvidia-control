using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ssprea_nvidia_control.Controls;

[PseudoClasses(":carouselled")]
public class CarousellableBorder : Border
{
    public void SetCarouselled()
    {
        PseudoClasses.Set(":carouselled", true);
    }

    public void UnsetCarouselled()
    {
        PseudoClasses.Set(":carouselled", false);
        
    }
    
}