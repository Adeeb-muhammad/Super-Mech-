#nullable enable
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Small, original prototype catalogue for the local workshop. No third-party
/// item names, artwork, or source-game balance values are asserted here.
/// </summary>
public static class WorkshopCatalog
{
    private static readonly IReadOnlyList<WorkshopPartDefinition> Catalogue =
        new List<WorkshopPartDefinition>
        {
            new()
            {
                Id = "frame_starter",
                DisplayName = "Starter Frame",
                Description = "A dependable chassis for a first workshop build.",
                Slot = MechPartSlot.Frame,
                Health = 100, Weight = 34, BuildCost = 12,
                EnergyCapacity = 10, Cooling = 6, Armor = 12,
                PreviewColor = "#5e829d"
            },
            new()
            {
                Id = "frame_bastion",
                DisplayName = "Bastion Frame",
                Description = "A heavier frame with more structural protection.",
                Slot = MechPartSlot.Frame,
                Health = 145, Weight = 48, BuildCost = 20,
                EnergyCapacity = 8, Cooling = 3, Armor = 22,
                PreviewColor = "#7b8797"
            },
            new()
            {
                Id = "mobility_walker",
                DisplayName = "Walker Legs",
                Description = "Balanced mobility supports for a general-purpose build.",
                Slot = MechPartSlot.Mobility,
                Health = 35, Weight = 18, BuildCost = 9,
                EnergyCapacity = 4, Cooling = 3, Armor = 5,
                PreviewColor = "#6a9b93"
            },
            new()
            {
                Id = "mobility_scout",
                DisplayName = "Scout Treads",
                Description = "A lighter mobility set that keeps the overall build nimble.",
                Slot = MechPartSlot.Mobility,
                Health = 25, Weight = 12, BuildCost = 7,
                EnergyCapacity = 5, Cooling = 5, Armor = 3,
                PreviewColor = "#8aa66a"
            },
            new()
            {
                Id = "core_compact",
                DisplayName = "Compact Cell",
                Description = "An optional reserve cell for longer operating time.",
                Slot = MechPartSlot.Core,
                Health = 0, Weight = 8, BuildCost = 6,
                EnergyCapacity = 22, Cooling = 0, Armor = 0,
                PreviewColor = "#d0a34e"
            },
            new()
            {
                Id = "core_coolant",
                DisplayName = "Coolant Core",
                Description = "A support core focused on thermal recovery.",
                Slot = MechPartSlot.Core,
                Health = 0, Weight = 9, BuildCost = 7,
                EnergyCapacity = 8, Cooling = 18, Armor = 0,
                PreviewColor = "#61a9bb"
            },
            new()
            {
                Id = "utility_plating",
                DisplayName = "Utility Plating",
                Description = "An optional layer that adds health and structural protection.",
                Slot = MechPartSlot.Utility,
                Health = 18, Weight = 8, BuildCost = 4,
                EnergyCapacity = 0, Cooling = 0, Armor = 9,
                PreviewColor = "#9b8db7"
            },
            new()
            {
                Id = "utility_service",
                DisplayName = "Service Module",
                Description = "A light support module for extra reserve and cooling.",
                Slot = MechPartSlot.Utility,
                Health = 4, Weight = 5, BuildCost = 3,
                EnergyCapacity = 5, Cooling = 6, Armor = 1,
                PreviewColor = "#b28f78"
            }
        }.AsReadOnly();

    public static IReadOnlyList<WorkshopPartDefinition> All => Catalogue;

    public static WorkshopPartDefinition? FindById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        return Catalogue.FirstOrDefault(part => part.Id == id);
    }

    public static IReadOnlyList<WorkshopPartDefinition> ForSlot(MechPartSlot slot)
    {
        return Catalogue.Where(part => part.Slot == slot).ToList().AsReadOnly();
    }
}
