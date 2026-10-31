using Godot;
using System.Collections.Generic;

/// <summary>
/// Attaches component sprites to the configured mech sockets and keeps the
/// equipped-component list synchronized with the visual assembly.
/// </summary>
public partial class MechAssembler : Node2D
{
    [Export] public Node2D TorsoSocket { get; set; }
    [Export] public Node2D LegSocket { get; set; }
    [Export] public Node2D LeftArmSocket { get; set; }
    [Export] public Node2D RightArmSocket { get; set; }

    [Signal]
    public delegate void MechChangedEventHandler();

    private sealed class SlotAssignment
    {
        public MechPart Part { get; }
        public Sprite2D Sprite { get; }

        public SlotAssignment(MechPart part, Sprite2D sprite)
        {
            Part = part;
            Sprite = sprite;
        }
    }

    private readonly List<MechPart> _equippedParts = new();
    private readonly Dictionary<Node2D, SlotAssignment> _slotAssignments = new();
    private MechStatManager _statManager;

    public override void _Ready()
    {
        _statManager = GetNodeOrNull<MechStatManager>("/root/MechStatManager");
        if (_statManager == null)
            GD.PushError("MechAssembler: MechStatManager autoload is missing.");
    }

    public IReadOnlyList<MechPart> GetEquippedParts()
    {
        return _equippedParts.AsReadOnly();
    }

    /// <summary>
    /// Equips a component in a socket. Equipping into an occupied socket replaces
    /// that socket's previous component rather than stacking another sprite.
    /// </summary>
    public bool TryAttachPart(MechPart part, Node2D socket)
    {
        if (part == null || part.Sprite == null)
        {
            GD.PushWarning("MechAssembler: A component and its sprite are required.");
            return false;
        }

        if (socket == null || !GodotObject.IsInstanceValid(socket))
        {
            GD.PushWarning("MechAssembler: The target socket is missing or no longer valid.");
            return false;
        }

        if (_statManager == null || !GodotObject.IsInstanceValid(_statManager))
        {
            GD.PushError("MechAssembler: Cannot equip components without MechStatManager.");
            return false;
        }

        _slotAssignments.TryGetValue(socket, out SlotAssignment previous);
        MechStatManager.MechStats statsForValidation = _statManager.CurrentStats;

        if (previous != null)
            statsForValidation = MechStatManager.WithoutPart(statsForValidation, previous.Part);

        if (!MechValidator.CanEquipPart(statsForValidation, part))
            return false;

        var sprite = new Sprite2D
        {
            Texture = part.Sprite,
            Name = "EquippedComponent"
        };
        socket.AddChild(sprite);

        if (previous != null)
        {
            _equippedParts.Remove(previous.Part);
            if (GodotObject.IsInstanceValid(previous.Sprite))
                previous.Sprite.QueueFree();
        }

        _equippedParts.Add(part);
        _slotAssignments[socket] = new SlotAssignment(part, sprite);
        EmitSignal(SignalName.MechChanged);
        return true;
    }

    /// <summary>
    /// Backwards-compatible wrapper for existing editor/test callers.
    /// </summary>
    public bool AttachPart(MechPart part, Node2D socket)
    {
        return TryAttachPart(part, socket);
    }
}
