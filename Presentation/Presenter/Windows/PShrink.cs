global using PShrink =
   GUIForDiskpart.Presentation.Presenter.Windows.PShrink<GUIForDiskpart.Presentation.View.Windows.WShrink>;

using System;
using System.Windows;
using System.Windows.Controls;

using GUIForDiskpart.Model.Logic.Diskpart;
using GUIForDiskpart.Presentation.Presenter.UserControls;
using GUIForDiskpart.Presentation.View.UserControls;
using GUIForDiskpart.Utils;


namespace GUIForDiskpart.Presentation.Presenter.Windows
{
    /// <summary>
    /// Constructed with: 
    /// <value><c>PartitionModel</c> Partition</value>
    /// <br/><br/>
    /// Must be instanced with <c>App.Instance.WIM.CreateWPresenter</c> method.<br/>
    /// See code example:
    /// <para>
    /// <code>
    /// App.Instance.WIM.CreateWPresenter&lt;PShrink&gt;(true, Partition);
    /// </code>
    /// </para>
    /// </summary>
    public class PShrink<T> : WPresenter<T> where T : WShrink
    {
        private PLog<UCLog> Log;

        public PartitionModel Partition { get; private set; }

        private UInt64 availableInByte;
        private UInt64 desiredInByte;
        private UInt64 minimumInByte;
        private const UInt64 BYTES_PER_MB = 1024UL * 1024UL;
        private bool updatingSizeControls;

        private UInt64 AvailableInMB => Math.Min(availableInByte / BYTES_PER_MB, UInt32.MaxValue);

        private void OnDesiredSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OnSliderChanged(Window.DesiredSlider, Window.DesiredSizeValue, Window.DesiredFormatted, ref desiredInByte);
        }

        private void OnMinimumSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OnSliderChanged(Window.MinimumSlider, Window.MinimumSizeValue, Window.MinFormatted, ref minimumInByte);
        }

        private void OnSliderChanged(Slider slider, TextBox textBox, Label label, ref ulong bytes)
        {
            if (updatingSizeControls) return;

            updatingSizeControls = true;
            try
            {
                bytes = System.Convert.ToUInt64(slider.Value) * BYTES_PER_MB;
                SetTextBox(bytes, textBox);
                SetFormattedLabel(bytes, label);
            }
            finally { updatingSizeControls = false; }
        }

        private void OnTextChanged(object sender, Slider slider, TextBox textBox, Label label, ref ulong bytes)
        {
            if (updatingSizeControls) return;

            updatingSizeControls = true;
            try
            {
                string digits = textBox.Text.RemoveAllButNumbers();
                if (textBox.Text != digits)
                {
                    textBox.Text = digits;
                    textBox.CaretIndex = digits.Length;
                }

                UInt64.TryParse(digits, out UInt64 value);
                value = Math.Min(value, AvailableInMB);
                bytes = value * BYTES_PER_MB;
                slider.Value = value;
                if (digits.Length > 0 && digits != value.ToString())
                {
                    textBox.Text = value.ToString();
                    textBox.CaretIndex = textBox.Text.Length;
                }
                SetFormattedLabel(bytes, label);
            }
            finally { updatingSizeControls = false; }
        }

        private void OnDesiredSizeValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnTextChanged(sender, Window.DesiredSlider, Window.DesiredSizeValue, Window.DesiredFormatted, ref desiredInByte);
        }

        private void OnMinimumSizeValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnTextChanged(sender, Window.MinimumSlider, Window.MinimumSizeValue, Window.MinFormatted, ref minimumInByte);
        }

        public void SetSliderValue(double value, Slider slider)
        {
            slider.Value = value;
        }

        public void SetFormattedLabel(ulong size, Label label)
        {
            label.Content = ByteFormatter.BytesToAsString(size);
        }

        public void SetAvailableLabel(ulong size)
        {
            Window.AvailableLabel.Content = ByteFormatter.BytesToAsString(size);
        }

        internal void ConfigureAvailableSize(ulong size)
        {
            availableInByte = size;
            SetupMinimumSlider(0.0d, AvailableInMB);
            SetupDesiredSlider(0.0d, AvailableInMB);
            SetAvailableLabel(size);
        }

        public void SetTextBox(ulong size, TextBox textBox)
        {
            textBox.Text = ByteFormatter.BytesToAsString(size, false, Unit.MB, 0);
        }

        public void SetupMinimumSlider(double min, double max)
        {
            Window.MinimumSlider.Minimum = min;
            Window.MinimumSlider.Maximum = max;
        }

        public void SetupDesiredSlider(double min, double max)
        {
            Window.DesiredSlider.Minimum = min;
            Window.DesiredSlider.Maximum = max;
        }

        #region OnClick

        private void OnConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string output = string.Empty;

            char driveLetter = Partition.WSM.DriveLetter;
            uint desiredMB = ByteFormatter.BytesTo<UInt64, UInt32>(desiredInByte, Unit.MB);
            uint minimumMB = ByteFormatter.BytesTo<UInt64, UInt32>(minimumInByte, Unit.MB);

            output += DPFunctions.Shrink(driveLetter, desiredMB, minimumMB, false, false);

            MainWindow.Log.Print(output);
            MainWindow.UpdatePanels(false);

            Close();
        }

        private void OnCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion OnClick

        #region WPresenter

        public override void Setup()
        {
            Partition.DefragAnalysis = DAService.AnalyzeVolumeDefrag(Partition);
            string output = string.Empty;
            output += Partition.WSM.GetOutputAsString();
            output += Partition.DefragAnalysis.GetOutputAsString();
            Log.Print(output, true);

            ConfigureAvailableSize(Partition.DefragAnalysis.AvailableForShrink);
        }

        protected override void AddCustomArgs(params object?[] args)
        {
            Partition = (PartitionModel)args[0];
        }

        protected override void RegisterEventsInternal()
        {
            base.RegisterEventsInternal();

            Window.EConfirm += OnConfirmButton_Click;
            Window.ECancel += OnCancelButton_Click;
            Window.EMinTextChanged += OnMinimumSizeValue_TextChanged;
            Window.EDesiredTextChanged += OnDesiredSizeValue_TextChanged;
            Window.EMinSlider += OnMinimumSlider_ValueChanged;
            Window.EDesiredSlider += OnDesiredSlider_ValueChanged;
        }

        public override void InitPresenters()
        {
            Log = CreateUCPresenter<PLog<UCLog>>(Window.Log);
        }

        #endregion WPresenter
    }
}
