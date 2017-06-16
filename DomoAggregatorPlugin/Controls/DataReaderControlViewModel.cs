using System.Collections.Generic;
using GalaSoft.MvvmLight;
using PluginUtil;
using WorkbenchPlugin.Views.Plugin.v2;
using WorkbenchPlugin.Views.Plugin.v2.DataProvider;

namespace DomoAggregatorPlugin.Controls
{
    public class DataReaderControlViewModel : ViewModelBase
    {
        private readonly IWorkbenchHost _callbackHost;
        private string _query;
        private int _timeout;

        public DataReaderControlViewModel(IWorkbenchHost callbackHost, IWorkbenchDataProviderPlugin dataProvider)
        {
            _callbackHost = callbackHost;

            var p = PropertyHelper.Deserialize<MyDataReaderProperties>(_callbackHost.GetReaderProperties());

            _query = p.Query;
            _timeout = p.Timeout;
            QueryVariables = p.QueryVariables;
        }

        public IDictionary<string, string> QueryVariables { get; set; }

        public string Query
        {
            get { return _query; }
            set
            {
                if (Set("Query", ref _query, value))
                {
                    SavePropertyChanges();
                }
            }
        }

        public int Timeout
        {
            get { return _timeout; }
            set
            {
                if (Set("Timeout", ref _timeout, value))
                {
                    SavePropertyChanges();
                }
            }
        }

        private void SavePropertyChanges()
        {
            var p = new MyDataReaderProperties
            {
                Query = _query,
                Timeout = _timeout
            };

            _callbackHost.SetReaderProperties(PropertyHelper.Serialize(p));
        }
    }
}
