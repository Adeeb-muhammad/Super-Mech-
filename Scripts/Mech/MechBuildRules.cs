using System;
using System.Collections.Generic;

/// <summary>
/// Pure build validation and stat aggregation. Kept independent of Godot so
/// workshop rules can be tested with an ordinary .NET test runner.
/// </summary>
public static class MechBuildRules
{
    public static bool CanEquip(
        int currentWeight,
        int addedWeight,
        int maxWeight,
        int currentEnergyCost,
        int addedEnergyCost,
        int maxEnergyCost)
    {
        if (currentWeight < 0 || addedWeight < 0 || maxWeight < 0 ||
            currentEnergyCost < 0 || addedEnergyCost < 0 || maxEnergyCost < 0)
        {
            return false;
        }

        return (long)currentWeight + addedWeight <= maxWeight &&
               (long)currentEnergyCost + addedEnergyCost <= maxEnergyCost;
    }

    public static WorkshopBuildStats CalculateWorkshopStats(IEnumerable<WorkshopPartDefinition> parts)
    {
        long health = 0;
        long weight = 0;
        long buildCost = 0;
        long energyCapacity = 0;
        long cooling = 0;
        long armor = 0;

        if (parts != null)
        {
            foreach (WorkshopPartDefinition part in parts)
            {
                if (part == null)
                    continue;

                health += Math.Max(0, part.Health);
                weight += Math.Max(0, part.Weight);
                buildCost += Math.Max(0, part.BuildCost);
                energyCapacity += Math.Max(0, part.EnergyCapacity);
                cooling += Math.Max(0, part.Cooling);
                armor += Math.Max(0, part.Armor);
            }
        }

        return new WorkshopBuildStats
        {
            Health = ClampToInt(health),
            Weight = ClampToInt(weight),
            BuildCost = ClampToInt(buildCost),
            EnergyCapacity = ClampToInt(energyCapacity),
            Cooling = ClampToInt(cooling),
            Armor = ClampToInt(armor)
        };
    }

    public static bool TryValidateWorkshopLoadout(
        IReadOnlyDictionary<MechPartSlot, WorkshopPartDefinition> loadout,
        int maxWeight,
        int maxBuildCost,
        out WorkshopBuildStats stats,
        out string error)
    {
        stats = new WorkshopBuildStats();
        error = "";

        if (loadout == null)
        {
            error = "No loadout was supplied.";
            return false;
        }

        if (maxWeight < 0 || maxBuildCost < 0)
        {
            error = "Build limits cannot be negative.";
            return false;
        }

        if (!loadout.ContainsKey(MechPartSlot.Frame) ||
            !loadout.ContainsKey(MechPartSlot.Mobility))
        {
            error = "A frame and mobility component are required.";
            return false;
        }

        foreach (KeyValuePair<MechPartSlot, WorkshopPartDefinition> entry in loadout)
        {
            if (entry.Value == null || !entry.Value.HasValidData())
            {
                error = $"Slot '{entry.Key}' contains an invalid component.";
                return false;
            }

            if (entry.Value.Slot != entry.Key)
            {
                error = $"Component '{entry.Value.DisplayName}' is in the wrong slot.";
                return false;
            }
        }

        stats = CalculateWorkshopStats(loadout.Values);
        if (stats.Weight > maxWeight)
        {
            error = $"Weight limit exceeded ({stats.Weight}/{maxWeight}).";
            return false;
        }

        if (stats.BuildCost > maxBuildCost)
        {
            error = $"Build-cost limit exceeded ({stats.BuildCost}/{maxBuildCost}).";
            return false;
        }

        return true;
    }

    private static int ClampToInt(long value)
    {
        return value > int.MaxValue ? int.MaxValue : (int)value;
    }
}
