using Godot;

/// <summary>
/// Validates a proposed component against the current build budgets.
/// </summary>
public static class MechValidator
{
    public static int MaxWeight { get; set; } = 100;
    public static int MaxEnergy { get; set; } = 50;

    public static bool CanEquipPart(MechStatManager.MechStats currentStats, MechPart newPart)
    {
        if (newPart == null)
            return false;

        if (newPart.Weight < 0 || newPart.EnergyCost < 0 ||
            newPart.HP < 0 || newPart.EnergyCapacity < 0 ||
            newPart.HeatGenerated < 0 || newPart.Cooling < 0 ||
            newPart.AttackPower < 0 || newPart.Defense < 0)
        {
            GD.PushWarning($"Cannot equip '{GetPartName(newPart)}': component stats cannot be negative.");
            return false;
        }

        bool valid = MechBuildRules.CanEquip(
            currentStats.TotalWeight,
            newPart.Weight,
            MaxWeight,
            currentStats.TotalEnergyCost,
            newPart.EnergyCost,
            MaxEnergy);

        if (!valid)
        {
            long nextWeight = (long)currentStats.TotalWeight + newPart.Weight;
            long nextEnergy = (long)currentStats.TotalEnergyCost + newPart.EnergyCost;
            GD.PrintErr($"Cannot equip '{GetPartName(newPart)}': build budget exceeded " +
                        $"(weight {nextWeight}/{MaxWeight}, energy cost {nextEnergy}/{MaxEnergy}).");
        }

        return valid;
    }

    private static string GetPartName(MechPart part)
    {
        if (!string.IsNullOrWhiteSpace(part.PartName))
            return part.PartName;

        if (!string.IsNullOrWhiteSpace(part.ResourceName))
            return part.ResourceName;

        return "unnamed component";
    }
}
