using System.Collections.Generic;

/// <summary>
/// Versioned save payload. Slot names are strings so the JSON remains readable
/// and can be migrated without depending on enum numeric values.
/// </summary>
public sealed class WorkshopLoadoutData
{
    public int Version { get; set; } = WorkshopLoadoutCodec.CurrentVersion;
    public string BuildName { get; set; } = "My Mech";
    public Dictionary<string, string> EquippedPartIds { get; set; } = new();
}
