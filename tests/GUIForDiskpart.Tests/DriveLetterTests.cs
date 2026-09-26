using GUIForDiskpart.Model.Data;

namespace GUIForDiskpart.Tests;

public class DriveLetterTests
{
    [Theory]
    [InlineData('A')]
    [InlineData('Z')]
    public void WsmDriveLetterIncludesBothEndpoints(char letter)
    {
        var partition = new Partition { WSM = new WSMModel { DriveLetter = letter } };

        Assert.True(partition.HasDriveLetter());
        Assert.Equal(letter, partition.GetDriveLetter());
    }

    [Theory]
    [InlineData('\0')]
    [InlineData('[')]
    public void MissingOrInvalidDriveLetterIsRejected(char letter)
    {
        var partition = new Partition { WSM = new WSMModel { DriveLetter = letter } };

        Assert.False(partition.HasDriveLetter());
        Assert.Equal(' ', partition.GetDriveLetter());
    }

    [Fact]
    public void WmiLetterIsUsedWhenWsmHasNoLetter()
    {
        var partition = new Partition
        {
            WSM = new WSMModel(),
            WMI = new WMIModel { logicalDiskModel = new LogicalDisk { DriveLetter = "A:" } }
        };

        Assert.True(partition.HasDriveLetter());
        Assert.Equal('A', partition.GetDriveLetter());
    }
}
