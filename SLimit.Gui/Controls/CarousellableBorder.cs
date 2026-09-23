using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace SLimit.Gui.Controls;

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