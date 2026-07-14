using System.Windows.Controls;

namespace LensLab.Views
{
    /// <summary>
    /// Interaction logic for AdjustmentPanel.xaml
    /// </summary>
    public partial class AdjustmentPanel : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdjustmentPanel"/> class.
        /// </summary>
        public AdjustmentPanel()
        {
            InitializeComponent();
        }

        private void Slider_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Slider slider)
            {
                if (slider.Tag != null && double.TryParse(slider.Tag.ToString(), out double defaultValue))
                {
                    slider.Value = defaultValue;
                }
                else
                {
                    slider.Value = 0.0;
                }
            }
        }

    }
}
