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
        private IDictionary<string, string> _queryVariables;
        private string _delimitedQueryVariables;
        private bool _isUsingLastValue;

        public DataReaderControlViewModel(IWorkbenchHost callbackHost, IWorkbenchDataProviderPlugin dataProvider)
        {
            _callbackHost = callbackHost;

            var p = PropertyHelper.Deserialize<MyDataReaderProperties>(_callbackHost.GetReaderProperties());

            _query = p.Query;
            _isUsingLastValue = p.IsUsingLastValue;
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

        public bool IsUsingLastValue
        {
            get { return _isUsingLastValue; }
            set
            {
                if (Set("IsUsingLastValue", ref _isUsingLastValue, value))
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
                IsUsingLastValue = _isUsingLastValue
            };

            _callbackHost.SetReaderProperties(PropertyHelper.Serialize(p));
        }
    }
}
