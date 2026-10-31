using Godot;
using System.Collections.Generic;

/// <summary>
/// Aggregates equipped part data. Registered as the MechStatManager autoload.
/// </summary>
public partial class MechStatManager : Node
{
    public struct MechStats
    {
        public int TotalHP;
        public int TotalWeight;
        public int TotalHeat;
        public int TotalCooling;
        public int TotalEnergyCost;
        public int TotalEnergyCapacity;
        public int TotalAttackPower;
        public int TotalDefense;
    }

    public MechStats CurrentStats { get; private set; }

    public void CalculateStats(IEnumerable<MechPart> equippedParts)
    {
        var totals = new MechStats();

        if (equippedParts != null)
        {
            foreach (MechPart part in equippedParts)
            {
                if (part == null)
                    continue;

                totals.TotalHP += part.HP;
                totals.TotalWeight += part.Weight;
                totals.TotalHeat += part.HeatGenerated;
                totals.TotalCooling += part.Cooling;
                totals.TotalEnergyCost += part.EnergyCost;
                totals.TotalEnergyCapacity += part.EnergyCapacity;
                totals.TotalAttackPower += part.AttackPower;
                totals.TotalDefense += part.Defense;
            }
        }

        CurrentStats = totals;
        GD.Print($"Mech stats updated: Weight {totals.TotalWeight}, HP {totals.TotalHP}, Energy capacity {totals.TotalEnergyCapacity}.");
    }

    public static MechStats WithoutPart(MechStats stats, MechPart part)
    {
        if (part == null)
            return stats;

        stats.TotalHP -= part.HP;
        stats.TotalWeight -= part.Weight;
        stats.TotalHeat -= part.HeatGenerated;
        stats.TotalCooling -= part.Cooling;
        stats.TotalEnergyCost -= part.EnergyCost;
        stats.TotalEnergyCapacity -= part.EnergyCapacity;
        stats.TotalAttackPower -= part.AttackPower;
        stats.TotalDefense -= part.Defense;
        return stats;
    }
}
