using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorkbenchPlugin.Views.Plugin.v3.DataReader;

namespace DomoAggregatorPlugin.Controls
{
    /// <summary>
    /// Interaction logic for DataReaderControl.xaml
    /// </summary>
    public partial class DataReaderControl : UserControl, IWorkbenchDataReaderPluginEditor
    {
        private readonly int DesiredHeight;
        private readonly DataReaderControlViewModel ViewModel;

        public DataReaderControl(DataReaderControlViewModel viewModel)
        {
            InitializeComponent();

            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            DesiredHeight = Convert.ToInt32(DesiredSize.Height);

            DataContext = ViewModel = viewModel;
        }

        public IList<IWorkbenchDataReaderPluginEditorActionButton> EditorRibbonButtons
        {
            get { return null; }
        }

        public FrameworkElement GetWpfControl(out int desiredHeight)
        {
            desiredHeight = DesiredHeight;
            return this;
        }

        public void LoadReaderProperties()
        {
        }

        public void ReaderPropertiesSaved()
        {
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
