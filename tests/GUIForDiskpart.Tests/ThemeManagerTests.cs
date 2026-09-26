using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GUIForDiskpart.Presentation;

namespace GUIForDiskpart.Tests;

public class ThemeManagerTests
{
    [Fact]
    public void ApplyingPaletteUpdatesExistingDynamicResources()
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                var resources = new ResourceDictionary();
                var label = new Label { Resources = resources };
                label.SetResourceReference(Control.ForegroundProperty, "ThemeTextBrush");

                ThemeManager.ApplyPalette(resources, true);
                Assert.Equal(Color.FromRgb(240, 242, 245), ((SolidColorBrush)label.Foreground).Color);
                Assert.Equal(Color.FromRgb(32, 36, 43),
                    ((SolidColorBrush)resources["ThemeWindowBrush"]).Color);

                ThemeManager.ApplyPalette(resources, false);
                Assert.Equal(Colors.Black, ((SolidColorBrush)label.Foreground).Color);
                Assert.Equal(Color.FromRgb(140, 140, 140),
                    ((SolidColorBrush)resources["ThemeWindowBrush"]).Color);
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
