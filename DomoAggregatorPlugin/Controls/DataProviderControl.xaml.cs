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
using WorkbenchPlugin.Views.Plugin.v3.DataProvider;

namespace DomoAggregatorPlugin.Controls
{
    /// <summary>
    /// Interaction logic for DataProviderControl.xaml
    /// </summary>
    public partial class DataProviderControl : UserControl, IWorkbenchDataProviderPluginEditor
    {
        private readonly int DesiredHeight;
        private readonly DataProviderControlViewModel ViewModel;

        public DataProviderControl(DataProviderControlViewModel viewModel)
        {
            InitializeComponent();

            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            DesiredHeight = Convert.ToInt32(DesiredSize.Height);

            DataContext = ViewModel = viewModel;
        }

        /// <summary>
        /// A user-friendly description of the data provider to be shown in the UI.
        /// </summary>
        public string DataProviderDescription
        {
            get
            {
                return "DomoPlugin1 description";
                //return "DomoAggregatorPlugin description";
            }
        }

        /// <summary>
        /// An event to inform Domo Workbench that properties changed.
        /// </summary>
        public event EventHandler OnPropertiesUpdated;

        public FrameworkElement GetWpfControl(out int desiredHeight)
        {
            desiredHeight = DesiredHeight;
            return this;
        }

        /// <summary>
        /// Informs this control that a save has occurred.
        /// </summary>
        public void ProviderPropertiesSaved()
        {
        }
    }
}
