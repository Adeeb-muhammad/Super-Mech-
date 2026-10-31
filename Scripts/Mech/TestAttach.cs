using Godot;

/// <summary>
/// Optional editor test helper. Assign a MechPart resource in the Inspector;
/// no hard-coded resource path is assumed.
/// </summary>
public partial class TestAttach : Node
{
    [Export] public MechPart PartToAttach { get; set; }
    [Export] public NodePath AssemblerPath { get; set; } = new NodePath("MechAssembler");

    public override void _Ready()
    {
        if (PartToAttach == null)
        {
            GD.PushWarning("TestAttach: Assign PartToAttach in the Inspector before using this helper.");
            return;
        }

        MechAssembler assembler = GetNodeOrNull<MechAssembler>(AssemblerPath);
        if (assembler == null)
        {
            GD.PushError($"TestAttach: MechAssembler was not found at '{AssemblerPath}'.");
            return;
        }

        if (assembler.TorsoSocket == null)
        {
            GD.PushError("TestAttach: MechAssembler.TorsoSocket is not assigned.");
            return;
        }

        if (!assembler.TryAttachPart(PartToAttach, assembler.TorsoSocket))
            GD.PushWarning("TestAttach: The component could not be equipped.");
    }
}
