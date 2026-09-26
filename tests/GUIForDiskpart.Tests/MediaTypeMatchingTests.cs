using DiskRetriever = GUIForDiskpart.Database.Retrievers.Disk;

namespace GUIForDiskpart.Tests;

public class MediaTypeMatchingTests
{
    private static readonly IReadOnlyDictionary<string, ushort> MediaTypes = new Dictionary<string, ushort>
    {
        ["Generic External"] = 0,
        ["WD_BLACK SN7100 1TB"] = 4,
        ["WD_BLACK"] = 3,
    };

    [Fact]
    public void ExactNameWinsOverShorterPrefix()
    {
        Assert.Equal((ushort)4, DiskRetriever.MatchMediaType("WD_BLACK SN7100 1TB", MediaTypes));
    }

    [Fact]
    public void DeviceSuffixUsesLongestMatchingName()
    {
        Assert.Equal((ushort)4, DiskRetriever.MatchMediaType("WD_BLACK SN7100 1TB SCSI Disk Device", MediaTypes));
    }

    [Fact]
    public void UnknownNameHasNoMediaType()
    {
        Assert.Null(DiskRetriever.MatchMediaType("Other USB Drive", MediaTypes));
    }

    [Fact]
    public void EquallySpecificMatchesAreAmbiguous()
    {
        var names = new Dictionary<string, ushort> { ["Disk A"] = 3, ["Disk B"] = 4 };

        Assert.Null(DiskRetriever.MatchMediaType("Disk A Disk B", names));
    }
}
