using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace sspreaNvidiaControl.Controls;

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