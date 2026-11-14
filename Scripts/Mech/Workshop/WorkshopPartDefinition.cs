using System;

/// <summary>
/// A project-authored workshop item. These starter values are prototype balance
/// values and are not claimed to reproduce the original game's item database.
/// </summary>
public enum MechPartSlot
{
    Frame,
    Mobility,
    Core,
    Utility
}

public sealed class WorkshopPartDefinition
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
    public MechPartSlot Slot { get; set; }
    public int Health { get; set; }
    public int Weight { get; set; }
    public int BuildCost { get; set; }
    public int EnergyCapacity { get; set; }
    public int Cooling { get; set; }
    public int Armor { get; set; }
    public string PreviewColor { get; set; } = "#6c8da8";

    public bool HasValidData()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               !string.IsNullOrWhiteSpace(DisplayName) &&
               Enum.IsDefined(typeof(MechPartSlot), Slot) &&
               HasValidPreviewColor() &&
               Health >= 0 &&
               Weight >= 0 &&
               BuildCost >= 0 &&
               EnergyCapacity >= 0 &&
               Cooling >= 0 &&
               Armor >= 0;
    }

    private bool HasValidPreviewColor()
    {
        if (string.IsNullOrWhiteSpace(PreviewColor) ||
            PreviewColor[0] != '#' ||
            (PreviewColor.Length != 7 && PreviewColor.Length != 9))
            return false;

        for (int index = 1; index < PreviewColor.Length; index++)
        {
            if (!Uri.IsHexDigit(PreviewColor[index]))
                return false;
        }

        return true;
    }
}

public sealed class WorkshopBuildStats
{
    public int Health { get; set; }
    public int Weight { get; set; }
    public int BuildCost { get; set; }
    public int EnergyCapacity { get; set; }
    public int Cooling { get; set; }
    public int Armor { get; set; }
}
