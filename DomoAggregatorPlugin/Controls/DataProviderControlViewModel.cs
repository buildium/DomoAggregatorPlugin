using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PluginUtil;
using System;
using WorkbenchPlugin.Views.Plugin.v3;

namespace DomoAggregatorPlugin.Controls
{
    public class DataProviderControlViewModel : ViewModelBase
    {
        private readonly IWorkbenchHost _callbackHost;
        private string _dataSources;
        private RelayCommand _applyChangesCommand;

        public DataProviderControlViewModel(IWorkbenchHost callbackHost)
        {
            _callbackHost = callbackHost;

            var p = PropertyHelper.Deserialize<MyDataProviderProperties>(_callbackHost.GetProviderProperties());

            _dataSources = p.DataSources;
        }

        public event EventHandler OnPropertiesUpdated;

        public string DataSources
        {
            get { return _dataSources; }
            set { Set("DataSources", ref _dataSources, value); }
        }

        public RelayCommand ApplyChangesCommand
        {
            get
            {
                return _applyChangesCommand
                  ?? (_applyChangesCommand = new RelayCommand(ExecuteApplyChangesCommand));
            }
        }

        private void ExecuteApplyChangesCommand()
        {
            var p = new MyDataProviderProperties
            {
                DataSources = _dataSources
            };

            _callbackHost.SetProviderProperties(PropertyHelper.Serialize(p));

            if (OnPropertiesUpdated != null)
            {
                OnPropertiesUpdated(this, EventArgs.Empty);
            }
        }
    }
}
