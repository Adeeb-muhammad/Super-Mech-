public class CombatManager
{
    /// <summary>
    /// Calculates damage based on attacker's power and defender's defense.
    /// Damage arithmetic is widened before subtraction and safely clamped.
    /// </summary>
    public int CalculateDamage(ICombatant attacker, ICombatant defender)
    {
        if (attacker == null || defender == null)
            return 0;

        return CombatRules.CalculateDamage(attacker.AttackPower, defender.Defense);
    }

    /// <summary>
    /// The multiplier is applied to raw AttackPower before defense is subtracted.
    /// Invalid multipliers return zero; callers should validate the full ability
    /// definition before consuming energy or executing an action.
    /// </summary>
    public int CalculateAbilityDamage(ICombatant attacker, ICombatant defender, Ability ability)
    {
        if (attacker == null || defender == null || ability == null)
            return 0;

        return CombatRules.CalculateAbilityDamage(
            attacker.AttackPower,
            defender.Defense,
            ability.DamageMultiplier);
    }
}