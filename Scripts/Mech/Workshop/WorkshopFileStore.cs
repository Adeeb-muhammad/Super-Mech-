#nullable enable
using System;
using System.IO;

/// <summary>
/// File-system persistence separated from Godot so local save behavior can be
/// tested with temporary directories and without launching the engine.
/// </summary>
public static class WorkshopFileStore
{
    public static bool TryLoad(string absolutePath, out WorkshopLoadoutData data, out string message)
    {
        data = WorkshopLoadoutCodec.CreateDefault();
        message = "";

        if (string.IsNullOrWhiteSpace(absolutePath))
        {
            message = "A save-file path is required.";
            return false;
        }

        try
        {
            if (!File.Exists(absolutePath))
            {
                message = "No saved build exists yet.";
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

    public static bool TrySave(string absolutePath, WorkshopLoadoutData data, out string message)
    {
        message = "";

        if (string.IsNullOrWhiteSpace(absolutePath))
        {
            message = "A save-file path is required.";
            return false;
        }

        try
        {
            string? directory = Path.GetDirectoryName(Path.GetFullPath(absolutePath));
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string json = WorkshopLoadoutCodec.Serialize(data);
            string temporaryPath = absolutePath + ".tmp";
            File.WriteAllText(temporaryPath, json);
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
