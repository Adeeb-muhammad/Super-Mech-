using Xunit;

public sealed class MechBuildRulesTests
{
    [Fact]
    public void CanEquip_ReturnsTrue_WhenBothBudgetsRemainWithinLimits()
    {
        bool result = MechBuildRules.CanEquip(
            currentWeight: 40,
            addedWeight: 20,
            maxWeight: 100,
            currentEnergyCost: 10,
            addedEnergyCost: 5,
            maxEnergyCost: 50);

        Assert.True(result);
    }

    [Fact]
    public void CanEquip_ReturnsFalse_WhenWeightBudgetWouldBeExceeded()
    {
        bool result = MechBuildRules.CanEquip(
            currentWeight: 90,
            addedWeight: 11,
            maxWeight: 100,
            currentEnergyCost: 10,
            addedEnergyCost: 5,
            maxEnergyCost: 50);

        Assert.False(result);
    }

    [Fact]
    public void CanEquip_ReturnsFalse_WhenEnergyBudgetWouldBeExceeded()
    {
        bool result = MechBuildRules.CanEquip(
            currentWeight: 40,
            addedWeight: 20,
            maxWeight: 100,
            currentEnergyCost: 48,
            addedEnergyCost: 3,
            maxEnergyCost: 50);

        Assert.False(result);
    }

    [Fact]
    public void CanEquip_ReturnsFalse_ForNegativeInput()
    {
        bool result = MechBuildRules.CanEquip(
            currentWeight: 10,
            addedWeight: -1,
            maxWeight: 100,
            currentEnergyCost: 10,
            addedEnergyCost: 1,
            maxEnergyCost: 50);

        Assert.False(result);
    }

    [Fact]
    public void CanEquip_UsesWideArithmetic_ToAvoidIntegerOverflow()
    {
        bool result = MechBuildRules.CanEquip(
            currentWeight: int.MaxValue,
            addedWeight: int.MaxValue,
            maxWeight: int.MaxValue,
            currentEnergyCost: 0,
            addedEnergyCost: 0,
            maxEnergyCost: 50);

        Assert.False(result);
    }

    [Fact]
    public void CanEquip_ReturnsTrue_WhenValuesExactlyMatchBothBudgets()
    {
        bool result = MechBuildRules.CanEquip(
            currentWeight: 90,
            addedWeight: 10,
            maxWeight: 100,
            currentEnergyCost: 45,
            addedEnergyCost: 5,
            maxEnergyCost: 50);

        Assert.True(result);
    }

    [Fact]
    public void CanEquip_ReturnsFalse_WhenExistingTotalsAreNegative()
    {
        bool result = MechBuildRules.CanEquip(
            currentWeight: -1,
            addedWeight: 0,
            maxWeight: 100,
            currentEnergyCost: 1,
            addedEnergyCost: 1,
            maxEnergyCost: 50);

        Assert.False(result);
    }

    [Fact]
    public void CanEquip_ReturnsFalse_WhenLimitsAreNegative()
    {
        bool result = MechBuildRules.CanEquip(
            currentWeight: 0,
            addedWeight: 0,
            maxWeight: -1,
            currentEnergyCost: 0,
            addedEnergyCost: 0,
            maxEnergyCost: 50);

        Assert.False(result);
    }
}
