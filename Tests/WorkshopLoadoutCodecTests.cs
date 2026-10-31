using System.Collections.Generic;
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
            WorkshopCatalog.FindById("frame_starter"),
            WorkshopCatalog.FindById("mobility_walker")
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
            [MechPartSlot.Frame] = WorkshopCatalog.FindById("frame_starter")
        };

        Assert.False(MechBuildRules.TryValidateWorkshopLoadout(loadout, 100, 50, out _, out string error));
        Assert.Contains("required", error);
    }

    [Fact]
    public void ValidateWorkshopLoadout_RejectsComponentInWrongSlot()
    {
        var loadout = new Dictionary<MechPartSlot, WorkshopPartDefinition>
        {
            [MechPartSlot.Frame] = WorkshopCatalog.FindById("mobility_walker"),
            [MechPartSlot.Mobility] = WorkshopCatalog.FindById("frame_starter")
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
            [MechPartSlot.Frame] = WorkshopCatalog.FindById(data.EquippedPartIds["Frame"]),
            [MechPartSlot.Mobility] = WorkshopCatalog.FindById(data.EquippedPartIds["Mobility"])
        };

        Assert.True(MechBuildRules.TryValidateWorkshopLoadout(loadout, 100, 50, out WorkshopBuildStats stats, out string error), error);
        Assert.Equal(52, stats.Weight);
    }
}
