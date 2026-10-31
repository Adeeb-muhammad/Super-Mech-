/// <summary>
/// Pure C# validation rules for the mech construction budget.
/// Kept independent of Godot so the rules can be unit-tested outside the editor.
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
}
