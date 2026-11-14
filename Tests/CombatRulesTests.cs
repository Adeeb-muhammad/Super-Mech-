using Xunit;

public sealed class CombatRulesTests
{
    [Fact]
    public void Distance_UsesWideArithmeticAtIntegerBoundaries()
    {
        Assert.Equal(4294967295L, CombatRules.Distance(int.MinValue, int.MaxValue));
    }

    [Theory]
    [InlineData(10, 30, 5, 15)]
    [InlineData(29, 30, 5, 30)]
    [InlineData(-10, 30, 5, 0)]
    [InlineData(10, -30, 5, 0)]
    public void RegenerateEnergy_ClampsToValidRange(int current, int maximum, int amount, int expected)
    {
        Assert.Equal(expected, CombatRules.RegenerateEnergy(current, maximum, amount));
    }

    [Fact]
    public void IsValidAbility_AcceptsSensibleAbilityData()
    {
        Assert.True(CombatRules.IsValidAbility(5, 1.5f, 1, 4));
    }

    [Theory]
    [InlineData(-1, 1.0f, 0, 1)]
    [InlineData(0, 0.0f, 0, 1)]
    [InlineData(0, -1.0f, 0, 1)]
    [InlineData(0, 1.0f, -1, 1)]
    [InlineData(0, 1.0f, 2, 1)]
    public void IsValidAbility_RejectsInvalidValues(int energyCost, float multiplier, int minRange, int maxRange)
    {
        Assert.False(CombatRules.IsValidAbility(energyCost, multiplier, minRange, maxRange));
    }

    [Fact]
    public void IsValidAbility_RejectsNonFiniteMultipliers()
    {
        Assert.False(CombatRules.IsValidAbility(0, float.NaN, 0, 1));
        Assert.False(CombatRules.IsValidAbility(0, float.PositiveInfinity, 0, 1));
    }

    [Fact]
    public void CalculateDamage_EnforcesMinimumDamageAndPreventsOverflow()
    {
        Assert.Equal(1, CombatRules.CalculateDamage(3, 10));
        Assert.Equal(12, CombatRules.CalculateDamage(20, 8));
        Assert.Equal(int.MaxValue, CombatRules.CalculateDamage(int.MaxValue, int.MinValue));
    }

    [Fact]
    public void CalculateAbilityDamage_UsesRawAttackBeforeDefense()
    {
        Assert.Equal(25, CombatRules.CalculateAbilityDamage(20, 5, 1.5f));
    }

    [Fact]
    public void CalculateAbilityDamage_ClampsLargeResultsAndRejectsInvalidMultiplier()
    {
        Assert.Equal(int.MaxValue, CombatRules.CalculateAbilityDamage(int.MaxValue, int.MinValue, float.MaxValue));
        Assert.Equal(0, CombatRules.CalculateAbilityDamage(100, 0, float.NaN));
        Assert.Equal(0, CombatRules.CalculateAbilityDamage(100, 0, -1.0f));
    }
}
