using System.Threading;
using GUIForDiskpart.Presentation.Presenter.Windows;
using GUIForDiskpart.Presentation.View.Windows;

namespace GUIForDiskpart.Tests;

public class SizeControlTests
{
    private const ulong MB = 1024UL * 1024UL;

    [Fact]
    public void ExtendSliderAndTextStayInWholeMegabytes()
    {
        RunSta(() =>
        {
            var window = new WExtend();
            try
            {
                var presenter = new PExtend<WExtend> { Window = window };
                presenter.RegisterEvents();
                presenter.ConfigureAvailableSize(100 * MB);

                window.DesiredSlider.Value = 37;
                Assert.Equal("37", window.DesiredSizeValue.Text);

                window.DesiredSizeValue.Text = "45";
                Assert.Equal(45, window.DesiredSlider.Value);

                window.DesiredSizeValue.Text = "200";
                Assert.Equal("100", window.DesiredSizeValue.Text);
                Assert.Equal(100, window.DesiredSlider.Value);
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void ShrinkSlidersAndTextStayIndependent()
    {
        RunSta(() =>
        {
            var window = new WShrink();
            try
            {
                var presenter = new PShrink<WShrink> { Window = window };
                presenter.RegisterEvents();
                presenter.ConfigureAvailableSize(100 * MB);

                window.MinimumSlider.Value = 12;
                Assert.Equal("12", window.MinimumSizeValue.Text);

                window.DesiredSizeValue.Text = "34";
                Assert.Equal(34, window.DesiredSlider.Value);
                Assert.Equal(12, window.MinimumSlider.Value);

                window.MinimumSizeValue.Text = "150";
                Assert.Equal("100", window.MinimumSizeValue.Text);
                Assert.Equal(100, window.MinimumSlider.Value);
            }
            finally { window.Close(); }
        });
    }

    private static void RunSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { error = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "WPF test thread did not finish.");
        if (error != null) throw error;
    }
}
