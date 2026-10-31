using Godot;

/// <summary>
/// Adapts the player's assembled mech stats to the combatant interface.
/// </summary>
public partial class MechCombatant : Node, ICombatant
{
    private MechStatManager _statManager;

    public int CurrentHP { get; set; }
    public int CurrentEnergy { get; set; }

    public int MaxHP
    {
        get => _statManager?.CurrentStats.TotalHP ?? 0;
        set { }
    }

    /// <summary>
    /// The battle reserve comes from EnergyCapacity, not the build-budget
    /// EnergyCost consumed by equipping components.
    /// </summary>
    public int MaxEnergy
    {
        get => _statManager?.CurrentStats.TotalEnergyCapacity ?? 0;
        set { }
    }

    public int AttackPower
    {
        get => _statManager?.CurrentStats.TotalAttackPower ?? 0;
        set { }
    }

    public int Defense
    {
        get => _statManager?.CurrentStats.TotalDefense ?? 0;
        set { }
    }

    public override void _Ready()
    {
        _statManager = GetNodeOrNull<MechStatManager>("/root/MechStatManager");
        if (_statManager == null)
            GD.PushError("MechCombatant: MechStatManager autoload was not found.");
    }

    public void InitializeForCombat()
    {
        if (_statManager == null)
        {
            GD.PushError("MechCombatant: Cannot initialize without MechStatManager.");
            CurrentHP = 0;
            CurrentEnergy = 0;
            return;
        }

        CurrentHP = System.Math.Max(0, MaxHP);
        CurrentEnergy = System.Math.Max(0, MaxEnergy);

        if (MaxHP <= 0)
            GD.PushWarning("MechCombatant: The assembled mech has no HP; assign valid component resources before combat.");

        GD.Print($"Mech ready: HP {CurrentHP}/{MaxHP}, Energy {CurrentEnergy}/{MaxEnergy}.");
    }
}
