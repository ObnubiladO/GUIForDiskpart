using System.Threading;
using System.Windows;
using System.Windows.Media;
using GUIForDiskpart.Presentation;
using GUIForDiskpart.Presentation.View.UserControls;
using GUIForDiskpart.Presentation.View.Windows;

namespace GUIForDiskpart.Tests;

[CollectionDefinition("Application resources", DisableParallelization = true)]
public class ApplicationResourcesCollection { }

[Collection("Application resources")]
public class DarkThemeIntegrationTests
{
    [Fact]
    public void MainWindowAndDialogUseTheSelectedPalette()
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            App? app = null;
            try
            {
                app = new App();
                app.InitializeComponent();
                ThemeManager.ApplyPalette(app.Resources, true);

                var main = new MainWindow();
                var tile = new UCPartitionEntry();
                main.PartitionPanel.Stack.Children.Add(tile);
                var dialog = new WAssignLetter();

                main.MainGrid.Measure(new Size(800, 760));
                main.MainGrid.Arrange(new Rect(0, 0, 800, 760));
                main.MainGrid.UpdateLayout();
                dialog.MainGrid.Measure(new Size(575, 280));
                dialog.MainGrid.Arrange(new Rect(0, 0, 575, 280));

                Assert.Equal(Color.FromRgb(32, 36, 43), ColorOf(main.MainGrid.Background));
                Assert.Equal(Color.FromRgb(52, 58, 68), ColorOf(tile.PartitionEntryButton.Background));
                Assert.Equal(Color.FromRgb(240, 242, 245), ColorOf(tile.PartitionType.Foreground));
                Assert.Equal(Color.FromRgb(49, 56, 69), ColorOf(main.EntryDataUI.EntryDataGrid.RowBackground));
                Assert.Equal(Color.FromRgb(32, 36, 43), ColorOf(dialog.MainGrid.Background));

                ThemeManager.ApplyPalette(app.Resources, false);
                Assert.Equal(Color.FromRgb(140, 140, 140), ColorOf(main.MainGrid.Background));
                Assert.Equal(Colors.Black, ColorOf(tile.PartitionType.Foreground));

                dialog.Close();
                main.Close();
            }
            catch (Exception exception)
            {
                error = exception;
            }
            finally
            {
                app?.Shutdown();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "WPF test thread did not finish.");
        if (error != null) throw error;
    }

    private static Color ColorOf(Brush brush) => Assert.IsType<SolidColorBrush>(brush).Color;
}
