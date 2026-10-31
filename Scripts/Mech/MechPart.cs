using Godot;

/// <summary>
/// Data resource for a single modular mech component.
/// EnergyCost is the build-budget cost; EnergyCapacity contributes to the
/// in-battle energy reserve. These values are deliberately kept separate.
/// </summary>
[GlobalClass]
public partial class MechPart : Resource
{
    [Export] public string PartName { get; set; } = "";
    [Export] public int HP { get; set; }
    [Export] public int Weight { get; set; }
    [Export] public int EnergyCost { get; set; }
    [Export] public int EnergyCapacity { get; set; }
    [Export] public int HeatGenerated { get; set; }
    [Export] public int Cooling { get; set; }
    [Export] public int AttackPower { get; set; }
    [Export] public int Defense { get; set; }
    [Export] public Texture2D Sprite { get; set; }
}
