namespace ssprea_nvidia_control.Models;

public class SettingsCategory
{
    public string Name { private set; get; }
    public string GridName { private set; get; }

    public SettingsCategory(string name, string gridName)
    {
        Name = name;
        GridName = gridName;
    }

    public static SettingsCategory GetGuiCategory()
    {
        return new SettingsCategory("Gui", @"Gui");
    }
}