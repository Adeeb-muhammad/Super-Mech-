using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Pure JSON encoder/decoder for workshop saves, kept separate from Godot I/O
/// so malformed saves and schema boundaries can be unit-tested.
/// </summary>
public static class WorkshopLoadoutCodec
{
    public const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Serialize(WorkshopLoadoutData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (data.Version != CurrentVersion)
            throw new ArgumentException($"Unsupported save version: {data.Version}.", nameof(data));

        if (data.EquippedPartIds == null)
            throw new ArgumentException("EquippedPartIds cannot be null.", nameof(data));

        return JsonSerializer.Serialize(data, Options);
    }

    public static bool TryDeserialize(string json, out WorkshopLoadoutData data, out string error)
    {
        data = null;
        error = "";

        if (string.IsNullOrWhiteSpace(json))
        {
            error = "Save data is empty.";
            return false;
        }

        try
        {
            WorkshopLoadoutData parsed = JsonSerializer.Deserialize<WorkshopLoadoutData>(json, Options);
            if (parsed == null)
            {
                error = "Save data did not contain a loadout.";
                return false;
            }

            if (parsed.Version != CurrentVersion)
            {
                error = $"Save version {parsed.Version} is not supported.";
                return false;
            }

            if (parsed.EquippedPartIds == null)
            {
                error = "The equipped-parts section is missing.";
                return false;
            }

            if (parsed.EquippedPartIds.Count > Enum.GetValues(typeof(MechPartSlot)).Length)
            {
                error = "The save contains more slots than the workshop supports.";
                return false;
            }

            var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> entry in parsed.EquippedPartIds)
            {
                if (!Enum.TryParse(entry.Key, ignoreCase: true, out MechPartSlot slot) ||
                    !Enum.IsDefined(typeof(MechPartSlot), slot))
                {
                    error = $"Unknown workshop slot '{entry.Key}'.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(entry.Value))
                {
                    error = $"Slot '{entry.Key}' contains an empty part ID.";
                    return false;
                }

                string normalizedSlot = slot.ToString();
                if (normalized.ContainsKey(normalizedSlot))
                {
                    error = $"Slot '{normalizedSlot}' is duplicated.";
                    return false;
                }

                normalized.Add(normalizedSlot, entry.Value.Trim());
            }

            parsed.EquippedPartIds = normalized;
            parsed.BuildName = string.IsNullOrWhiteSpace(parsed.BuildName)
                ? "My Mech"
                : parsed.BuildName.Trim();

            data = parsed;
            return true;
        }
        catch (JsonException exception)
        {
            error = $"Save data is not valid JSON: {exception.Message}";
            return false;
        }
        catch (NotSupportedException exception)
        {
            error = $"Save data format is not supported: {exception.Message}";
            return false;
        }
    }

    public static WorkshopLoadoutData CreateDefault()
    {
        return new WorkshopLoadoutData
        {
            Version = CurrentVersion,
            BuildName = "Starter Build",
            EquippedPartIds = new Dictionary<string, string>
            {
                [MechPartSlot.Frame.ToString()] = "frame_starter",
                [MechPartSlot.Mobility.ToString()] = "mobility_walker"
            }
        };
    }
}
