using GUIForDiskpart.Model.Logic;

namespace GUIForDiskpart.Tests;

public class PowerShellRuntimeTests
{
    [Fact]
    public void EmbeddedPowerShellCanRunAReadOnlyCommand()
    {
        var results = CommandExecuter.IssuePowershellCommand("Write-Output", "42");

        Assert.Single(results);
        Assert.Equal("42", results[0].ToString());
    }

    [Theory]
    [InlineData("Get-Partition")]
    [InlineData("Invoke-CimMethod")]
    public void EmbeddedPowerShellCanFindStorageCommands(string command)
    {
        Assert.NotEmpty(CommandExecuter.IssuePowershellCommand("Get-Command", command));
    }
}
