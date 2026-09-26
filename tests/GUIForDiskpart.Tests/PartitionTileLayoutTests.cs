using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GUIForDiskpart.Presentation.View.UserControls;
using GUIForDiskpart.Presentation.View.Windows;

namespace GUIForDiskpart.Tests;

public class PartitionTileLayoutTests
{
    [Theory]
    [InlineData(15)]
    [InlineData(22)]
    public void PartitionTableLineFitsAboveTheHorizontalScrollbar(double fontSize)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                var window = new MainWindow();
                var tile = new UCPartitionEntry();
                tile.PartitionNumber.Content = "#1";
                tile.DriveNameAndLetter.Content = "No letter";
                tile.TotalSpace.Content = "100 MB";
                tile.FileSystemText.Content = "No filesystem";
                tile.PartitionType.Content = "GPT: System";
                tile.PartitionType.FontSize = fontSize;
                window.PartitionPanel.Stack.Children.Add(tile);

                window.MainGrid.Measure(new Size(800, 760));
                window.MainGrid.Arrange(new Rect(0, 0, 800, 760));
                window.MainGrid.UpdateLayout();

                var scrollViewer = Assert.IsType<ScrollViewer>(window.PartitionPanel.Parent);
                var labelBottom = tile.PartitionType.TranslatePoint(
                    new Point(0, tile.PartitionType.ActualHeight), scrollViewer).Y;
                Assert.True(tile.PartitionType.ActualHeight > 0);
                Assert.True(labelBottom <= scrollViewer.ViewportHeight,
                    $"Partition table line ends at {labelBottom}, but viewport ends at {scrollViewer.ViewportHeight}.");
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
