using System;

/// <summary>
/// Pure combat calculations and input-data validation. Kept independent of
/// Godot so range, damage, and ability boundaries can be unit-tested.
/// </summary>
public static class CombatRules
{
    public static long Distance(int firstPosition, int secondPosition)
    {
        return Math.Abs((long)firstPosition - secondPosition);
    }

    public static int RegenerateEnergy(int currentEnergy, int maxEnergy, int regenerationAmount)
    {
        long maximum = Math.Max(0, maxEnergy);
        long regenerated = (long)currentEnergy + Math.Max(0, regenerationAmount);
        return (int)Math.Clamp(regenerated, 0L, maximum);
    }

    public static bool IsValidAbility(int energyCost, float damageMultiplier, int minRange, int maxRange)
    {
        return energyCost >= 0 &&
               float.IsFinite(damageMultiplier) &&
               damageMultiplier > 0.0f &&
               minRange >= 0 &&
               maxRange >= minRange;
    }

    public static int CalculateDamage(int attackPower, int defense)
    {
        long damage = (long)attackPower - defense;
        return ClampDamage(damage);
    }

    public static int CalculateAbilityDamage(int attackPower, int defense, float damageMultiplier)
    {
        if (!float.IsFinite(damageMultiplier) || damageMultiplier <= 0.0f)
            return 0;

        double damage = ((double)attackPower * damageMultiplier) - defense;
        if (double.IsNaN(damage))
            return 0;

        if (damage <= 1.0d)
            return 1;

        if (damage >= int.MaxValue)
            return int.MaxValue;

        return (int)damage;
    }

    private static int ClampDamage(long damage)
    {
        if (damage <= 1)
            return 1;

        return damage >= int.MaxValue ? int.MaxValue : (int)damage;
    }
}
