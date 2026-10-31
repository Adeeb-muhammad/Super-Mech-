using System;
using System.IO;
using Godot;

/// <summary>
/// Local user:// persistence for a workshop loadout. The data codec itself is
/// pure C#; this class only handles the Godot per-user storage location.
/// </summary>
public static class WorkshopSaveStore
{
    private const string SavePath = "user://mech_loadout.json";

    public static bool TryLoad(out WorkshopLoadoutData data, out string message)
    {
        data = WorkshopLoadoutCodec.CreateDefault();
        message = "";

        try
        {
            string absolutePath = ProjectSettings.GlobalizePath(SavePath);
            if (!File.Exists(absolutePath))
            {
                message = "New local build created.";
                return false;
            }

            string json = File.ReadAllText(absolutePath);
            if (!WorkshopLoadoutCodec.TryDeserialize(json, out data, out message))
            {
                data = WorkshopLoadoutCodec.CreateDefault();
                return false;
            }

            message = "Local build loaded.";
            return true;
        }
        catch (Exception exception) when (
            exception is IOException ||
            exception is UnauthorizedAccessException ||
            exception is ArgumentException)
        {
            message = $"Could not read local save: {exception.Message}";
            data = WorkshopLoadoutCodec.CreateDefault();
            return false;
        }
    }

    public static bool TrySave(WorkshopLoadoutData data, out string message)
    {
        message = "";

        try
        {
            string absolutePath = ProjectSettings.GlobalizePath(SavePath);
            string directory = Path.GetDirectoryName(absolutePath);

            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string json = WorkshopLoadoutCodec.Serialize(data);
            string temporaryPath = absolutePath + ".tmp";
            File.WriteAllText(temporaryPath, json);

            if (File.Exists(absolutePath))
                File.Delete(absolutePath);

            File.Move(temporaryPath, absolutePath, overwrite: true);
            message = "Build saved locally.";
            return true;
        }
        catch (Exception exception) when (
            exception is IOException ||
            exception is UnauthorizedAccessException ||
            exception is ArgumentException ||
            exception is System.Text.Json.JsonException)
        {
            message = $"Could not save local build: {exception.Message}";
            return false;
        }
    }
}
