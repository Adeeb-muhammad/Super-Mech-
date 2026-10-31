using Godot;

/// <summary>
/// Connects visual assembly changes to the global mech-stat aggregator.
/// </summary>
public partial class PlayerMechController : Node2D
{
    private MechAssembler _assembler;
    private MechStatManager _statManager;

    public override void _Ready()
    {
        _assembler = GetNodeOrNull<MechAssembler>("MechAssembler");
        _statManager = GetNodeOrNull<MechStatManager>("/root/MechStatManager");

        if (_assembler == null)
        {
            GD.PushError("PlayerMechController: MechAssembler child was not found.");
            return;
        }

        if (_statManager == null)
        {
            GD.PushError("PlayerMechController: MechStatManager autoload was not found.");
            return;
        }

        _assembler.MechChanged += OnMechChanged;
        OnMechChanged();
    }

    private void OnMechChanged()
    {
        if (_assembler == null || _statManager == null)
            return;

        _statManager.CalculateStats(_assembler.GetEquippedParts());
    }

    public override void _ExitTree()
    {
        if (_assembler != null)
            _assembler.MechChanged -= OnMechChanged;
    }
}
