using System.Collections.Generic;
using System.IO;
using Xunit;

public sealed class WorkshopLoadoutCodecTests
{
    [Fact]
    public void DefaultLoadout_RoundTripsThroughJson()
    {
        WorkshopLoadoutData original = WorkshopLoadoutCodec.CreateDefault();

        string json = WorkshopLoadoutCodec.Serialize(original);

        Assert.True(WorkshopLoadoutCodec.TryDeserialize(json, out WorkshopLoadoutData restored, out string error), error);
        Assert.Equal(WorkshopLoadoutCodec.CurrentVersion, restored.Version);
        Assert.Equal("frame_starter", restored.EquippedPartIds["Frame"]);
        Assert.Equal("mobility_walker", restored.EquippedPartIds["Mobility"]);
    }

    [Fact]
    public void TryDeserialize_RejectsEmptyInput()
    {
        Assert.False(WorkshopLoadoutCodec.TryDeserialize("", out _, out string error));
        Assert.Contains("empty", error);
    }

    [Fact]
    public void TryDeserialize_RejectsMalformedJson()
    {
        Assert.False(WorkshopLoadoutCodec.TryDeserialize("{not-json", out _, out string error));
        Assert.Contains("valid JSON", error);
    }

    [Fact]
    public void TryDeserialize_RejectsUnsupportedVersion()
    {
        const string json = "{\"Version\":99,\"BuildName\":\"Old\",\"EquippedPartIds\":{}}";

        Assert.False(WorkshopLoadoutCodec.TryDeserialize(json, out _, out string error));
        Assert.Contains("not supported", error);
    }

    [Fact]
    public void TryDeserialize_NormalizesSlotNameCasing()
    {
        const string json = "{\"Version\":1,\"BuildName\":\"Test\",\"EquippedPartIds\":{\"frame\":\"frame_starter\"}}";

        Assert.True(WorkshopLoadoutCodec.TryDeserialize(json, out WorkshopLoadoutData data, out string error), error);
        Assert.True(data.EquippedPartIds.ContainsKey("Frame"));
    }

    [Fact]
    public void TryDeserialize_RejectsUnknownSlot()
    {
        const string json = "{\"Version\":1,\"BuildName\":\"Test\",\"EquippedPartIds\":{\"Unknown\":\"part_1\"}}";

        Assert.False(WorkshopLoadoutCodec.TryDeserialize(json, out _, out string error));
        Assert.Contains("Unknown workshop slot", error);
    }

    [Fact]
    public void TryDeserialize_RejectsEmptyPartId()
    {
        const string json = "{\"Version\":1,\"BuildName\":\"Test\",\"EquippedPartIds\":{\"Frame\":\" \"}}";

        Assert.False(WorkshopLoadoutCodec.TryDeserialize(json, out _, out string error));
        Assert.Contains("empty part ID", error);
    }

    [Fact]
    public void Serialize_RejectsNullPayload()
    {
        Assert.Throws<System.ArgumentNullException>(() => WorkshopLoadoutCodec.Serialize(null!));
    }

    [Fact]
    public void CatalogPartsHaveUniqueIdsAndValidData()
    {
        var seen = new HashSet<string>();

        foreach (WorkshopPartDefinition part in WorkshopCatalog.All)
        {
            Assert.True(part.HasValidData());
            Assert.True(seen.Add(part.Id));
        }
    }

    [Fact]
    public void WorkshopStats_AggregateEquippedParts()
    {
        WorkshopBuildStats stats = MechBuildRules.CalculateWorkshopStats(new[]
        {
            WorkshopCatalog.FindById("frame_starter")!,
            WorkshopCatalog.FindById("mobility_walker")!
        });

        Assert.Equal(135, stats.Health);
        Assert.Equal(52, stats.Weight);
        Assert.Equal(21, stats.BuildCost);
        Assert.Equal(14, stats.EnergyCapacity);
        Assert.Equal(9, stats.Cooling);
        Assert.Equal(17, stats.Armor);
    }

    [Fact]
    public void ValidateWorkshopLoadout_RequiresFrameAndMobility()
    {
        var loadout = new Dictionary<MechPartSlot, WorkshopPartDefinition>
        {
            [MechPartSlot.Frame] = WorkshopCatalog.FindById("frame_starter")!
        };

        Assert.False(MechBuildRules.TryValidateWorkshopLoadout(loadout, 100, 50, out _, out string error));
        Assert.Contains("required", error);
    }

    [Fact]
    public void ValidateWorkshopLoadout_RejectsComponentInWrongSlot()
    {
        var loadout = new Dictionary<MechPartSlot, WorkshopPartDefinition>
        {
            [MechPartSlot.Frame] = WorkshopCatalog.FindById("mobility_walker")!,
            [MechPartSlot.Mobility] = WorkshopCatalog.FindById("frame_starter")!
        };

        Assert.False(MechBuildRules.TryValidateWorkshopLoadout(loadout, 100, 50, out _, out string error));
        Assert.Contains("wrong slot", error);
    }

    [Fact]
    public void ValidateWorkshopLoadout_AcceptsStarterBuildWithinBudgets()
    {
        WorkshopLoadoutData data = WorkshopLoadoutCodec.CreateDefault();
        var loadout = new Dictionary<MechPartSlot, WorkshopPartDefinition>
        {
            [MechPartSlot.Frame] = WorkshopCatalog.FindById(data.EquippedPartIds["Frame"])!,
            [MechPartSlot.Mobility] = WorkshopCatalog.FindById(data.EquippedPartIds["Mobility"])!
        };

        Assert.True(MechBuildRules.TryValidateWorkshopLoadout(loadout, 100, 50, out WorkshopBuildStats stats, out string error), error);
        Assert.Equal(52, stats.Weight);
    }

    [Fact]
    public void FileStore_SavesAndLoadsVersionedBuild()
    {
        string directory = Path.Combine(Path.GetTempPath(), "super-mech-workshop-" + System.Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "mech_loadout.json");

        try
        {
            WorkshopLoadoutData original = WorkshopLoadoutCodec.CreateDefault();
            Assert.True(WorkshopFileStore.TrySave(path, original, out string saveMessage), saveMessage);
            Assert.True(File.Exists(path));

            Assert.True(WorkshopFileStore.TryLoad(path, out WorkshopLoadoutData restored, out string loadMessage), loadMessage);
            Assert.Equal("frame_starter", restored.EquippedPartIds["Frame"]);
            Assert.Equal("mobility_walker", restored.EquippedPartIds["Mobility"]);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void FileStore_ReturnsDefaultBuildWhenSaveIsMissing()
    {
        string directory = Path.Combine(Path.GetTempPath(), "super-mech-workshop-" + System.Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "missing.json");

        Assert.False(WorkshopFileStore.TryLoad(path, out WorkshopLoadoutData data, out string message));
        Assert.Equal("frame_starter", data.EquippedPartIds["Frame"]);
        Assert.Contains("No saved build", message);
    }

    [Fact]
    public void FileStore_DoesNotReplaceGoodSaveWhenNewPayloadIsInvalid()
    {
        string directory = Path.Combine(Path.GetTempPath(), "super-mech-workshop-" + System.Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "mech_loadout.json");

        try
        {
            WorkshopLoadoutData original = WorkshopLoadoutCodec.CreateDefault();
            Assert.True(WorkshopFileStore.TrySave(path, original, out string saveMessage), saveMessage);

            var invalid = new WorkshopLoadoutData { Version = 99 };
            Assert.False(WorkshopFileStore.TrySave(path, invalid, out _));

            Assert.True(WorkshopFileStore.TryLoad(path, out WorkshopLoadoutData restored, out string loadMessage), loadMessage);
            Assert.Equal("frame_starter", restored.EquippedPartIds["Frame"]);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }


    [Fact]
    public void LayoutRules_UseCompactLayoutBelowBreakpoint()
    {
        Assert.True(WorkshopLayoutRules.UseCompactLayout(1023.9f));
        Assert.False(WorkshopLayoutRules.UseCompactLayout(1024.0f));
        Assert.False(WorkshopLayoutRules.UseCompactLayout(1280.0f));
    }

    [Fact]
    public void LayoutRules_UseSmallerMarginsOnNarrowScreens()
    {
        Assert.Equal(10, WorkshopLayoutRules.HorizontalMargin(390.0f));
        Assert.Equal(16, WorkshopLayoutRules.HorizontalMargin(600.0f));
        Assert.Equal(16, WorkshopLayoutRules.HorizontalMargin(900.0f));
    }

}
