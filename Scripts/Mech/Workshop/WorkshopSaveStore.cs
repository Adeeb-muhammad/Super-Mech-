using Godot;

/// <summary>
/// Godot adapter for local workshop persistence. All file behavior and error
/// handling live in WorkshopFileStore so they can be tested independently.
/// </summary>
public static class WorkshopSaveStore
{
    private const string SavePath = "user://mech_loadout.json";

    public static bool TryLoad(out WorkshopLoadoutData data, out string message)
    {
        string absolutePath = ProjectSettings.GlobalizePath(SavePath);
        return WorkshopFileStore.TryLoad(absolutePath, out data, out message);
    }

    public static bool TrySave(WorkshopLoadoutData data, out string message)
    {
        string absolutePath = ProjectSettings.GlobalizePath(SavePath);
        return WorkshopFileStore.TrySave(absolutePath, data, out message);
    }
}
