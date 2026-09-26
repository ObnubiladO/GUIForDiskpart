using System.Threading;
using GUIForDiskpart.Presentation.View.Windows;

namespace GUIForDiskpart.Tests;

public class DriveLetterDialogTests
{
    [Fact]
    public void EscapeCanOnlyTriggerCancel()
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                var window = new WAssignLetter();
                Assert.True(window.CancelButton.IsCancel);
                Assert.False(window.RemoveButton.IsCancel);
                window.Close();
            }
            catch (Exception exception)
            {
                error = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "WPF test thread did not finish.");
        if (error != null) throw error;
    }
}
