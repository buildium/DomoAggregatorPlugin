﻿using DomoAggregatorPlugin.Controls;
using PluginUtil;
using System;
using System.AddIn;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using WorkbenchPlugin.Views.Plugin.v3;
using WorkbenchPlugin.Views.Plugin.v3.DataProvider;
using WorkbenchPlugin.Views.Plugin.v3.DataReader;
using WorkbenchSDK.Configuration;


namespace DomoAggregatorPlugin
{
    /// <summary>
    /// Provides a way to read a specific data source in a way Domo Workbench can understand.
    /// </summary>
    [AddIn("DomoAggregatorPlugin Reader", Publisher = "", Description = "DomoAggregatorPlugin Workbench Plugin", Version = "1.0.0.0")]
    public class DataReader : IWorkbenchDataReaderPlugin
    {
        private IWorkbenchHost _callbackHost;
        private bool _cancelRequested = false;
        private int _currentRowIndex = -1;
        private IWorkbenchDataProviderPlugin _dataProvider;
        private MyDataReaderProperties _readerProperties;
        private ConnectionMetadata _currentConnection;
        private List<ConnectionMetadata> _connections = new List<ConnectionMetadata>();

        private const string DatabaseSourceColumnName = "subscriber_database";
        private const string LastValueParameter = "lastvalue";

        private int _count;
        private bool _moveNextBool;
        /// <summary>
        /// Any execution characteristics that are needed by this DataReader
        /// </summary>
        public IWorkbenchDataReaderPluginExecutionCharacteristics ExecutionCharacteristics
        {
            get { return null; }
        }

        /// <summary>
        /// The user-friendly name for this DataReader implementation.
        /// </summary>
        public string Name
        {
            get { return "DomoAggregatorPlugin"; }
        }

        /// <summary>
        /// An image to be displayed when this DataReader is selected.
        /// </summary>
        public byte[] NavigationImage
        {
            get { return null; }
        }

        /// <summary>
        /// The recommended DataSet type to upload as
        /// </summary>
        public string RecommendedDataSetType
        {
            get { return DataSetTypes.Odbc; }
        }

        /// <summary>
        /// Whether or not this DataReader requires a 32bit process.
        /// </summary>
        public bool Requires32Bit
        {
            get { return false; }
        }

        /// <summary>
        /// Whether or not this DataReader requires an interactive session.
        /// </summary>
        public bool RequiresInteractiveSession
        {
            get { return false; }
        }

        /// <summary>
        /// Allows the DataReader to define the type of supported input.
        /// This should be used with <see cref="SourceType"/>
        /// </summary>
        public string SupportedSourceTypes
        {
            get { return SourceType.DbConnection; }
        }

        /// <summary>
        /// Called to apply changes to a job. These changes are generated by a call to IWorkbenchHost.SetJobChange().
        /// </summary>
        /// <param name="jobChanges"></param>
        public void ApplyJobChanges(IList<IWorkbenchJobChange> jobChanges)
        {
        }

        /// <summary>
        /// Requests a cancel of a running operation.
        /// </summary>
        public void Cancel()
        {
            _cancelRequested = true;
        }

        /// <summary>
        /// Cleans up resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                foreach (var connectionMetadata in _connections)
                {
                    connectionMetadata.Reader?.Close();
                    connectionMetadata.Reader?.Dispose();
                    connectionMetadata.Connection?.Close();
                }
            }
            catch (Exception e)
            {
                new EmailNotification().EmailNotificationSender(e.ToString(), _callbackHost.GetJob().ToString(), "Dispose()");
                throw new Exception(e.ToString());

            }
        }

     

        /// <summary>
        /// Gets the editor for this DataReader.
        /// </summary>
        /// <returns>Returns the editor that allows configuration for this DataReader</returns>
        public IWorkbenchDataReaderPluginEditor GetDataReaderEditor()
        {
            LogEvent(LogMessageType.Progress, "GetDataReader called");
            return new DataReaderControl(new DataReaderControlViewModel(_callbackHost, _dataProvider));
        }

        /// <summary>
        /// Gets the column headers for each row.
        /// </summary>
        /// <returns>A list of column header strings</returns>
        public IList<string> GetHeaders()
        {
            var headers = new List<string>();
            // send back column headers to demonstrate this plugin
            for (var i = 0; i < _connections.First().Reader.FieldCount; i++)
            {
                headers.Add(_connections.First().Reader.GetName(i));
            }
            headers.Add(DatabaseSourceColumnName);
            return headers;
        }

        /// <summary>
        /// While the <see cref="DomoAggregatorPlugin.DataReader.MoveNext"/> returns true, this method will be invoked to get the
        /// row data.  The data needs to be in the same order that the <see cref="DomoAggregatorPlugin.DataReader.GetHeaders"/> returns.
        /// </summary>
        /// <returns>A list of row data.</returns>
        public List<object> GetRowData()
        {
            try
            {

                if (_moveNextBool)
                {
                    MoveNext();
                    _moveNextBool = false;
                }
           
                LogEvent(LogMessageType.Progress, "GetRowData Start" + _currentConnection.DSN.ToString());
          
                List<object> rowData = new List<object>();

                // send the row data back in the same order as the headers
                foreach (var header in GetHeaders())
                {
                    //Add data for additional data source column allowing us to determine where the query results originated from
                    if (DatabaseSourceColumnName.Equals(header))
                    {
                        rowData.Add(_currentConnection.DSN);
                        continue;
                    }
                    if (!_currentConnection.Reader.HasRows)
                    {
                        LogEvent(LogMessageType.Progress, "GetRowData !_currentConnection.Reader.HasRows");
                        return rowData;
                    }
                    rowData.Add(_currentConnection.Reader[header]);

                    var key = $"{_currentConnection.DSN}:{header}";
                    if (_readerProperties.QueryVariables.ContainsKey(key) && _currentConnection.Reader[header] != null)
                    {
                        _readerProperties.QueryVariables[key] = _currentConnection.Reader[header].ToString();
                        _callbackHost.SetReaderProperties(PropertyHelper.Serialize(_readerProperties));
                    }

                    if (_cancelRequested)
                    {
                        throw new OperationCanceledException();
                    }
                }

                LogEvent(LogMessageType.Progress, "GetRowData End");
           
                return rowData;
            }
            catch (Exception e)
            {
                new EmailNotification().EmailNotificationSender(e.ToString(), _callbackHost.GetJob().ToString(), "GetRowData()");
                throw new Exception(e.ToString());

            }
        }

        public void Initialize(IWorkbenchHost workbenchHost, IWorkbenchDataProviderPlugin dataProvider)
        {
            _callbackHost = workbenchHost;
            _dataProvider = dataProvider;
        }

        /// <summary>
        /// Moves the reader to the next row.
        /// </summary>
        /// <returns>Whether or not there are more rows to read.</returns>
        public bool MoveNext()
        {
            try
            {
                if (_connections[_count-1].Reader.Read())
                {
                    LogEvent(LogMessageType.Progress, "setting the _connections and count is" + _count);
                    _currentConnection = _connections[_count - 1];
                    return true;
                }

                var dataProviderProperties =
                    PropertyHelper.Deserialize<MyDataProviderProperties>(_callbackHost.GetProviderProperties());

                if (_count < dataProviderProperties.ConnectionStrings.Count)
                {
                    LogEvent(LogMessageType.Progress, "in count less ten reopening connection");

                    OpenConnection();
                    _moveNextBool = true;
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                if (e.GetType().IsAssignableFrom(typeof(System.Data.Odbc.OdbcException)))
                {
                    restartConnection();
                    return true;
                }

                new EmailNotification().EmailNotificationSender(e.ToString(), _callbackHost.GetJob().ToString(), "MoveNext()");
                throw new Exception(e.ToString());
            }
        }
        private void restartConnection()
        {
            _connections.RemoveAt(_count - 1);
            _count = _count - 1; 
            Open();
            MoveNext();
        }

        /// <summary>
        /// Opens the data set in preparation for the Read operation.
        /// </summary>
        public void Open()
        {

            // load the properties from the UI
            _readerProperties = PropertyHelper.Deserialize<MyDataReaderProperties>(_callbackHost.GetReaderProperties());

            if (string.IsNullOrEmpty(_readerProperties.Query))
            {
                LogEvent(LogMessageType.Error, "No Query was provided. Please update the Source with a valid SQL query.");
            }
            OpenConnection();
        }



        private void OpenConnection()
        {
            LogEvent(LogMessageType.Progress, "Open start");

            var dataProviderProperties = PropertyHelper.Deserialize<MyDataProviderProperties>(_callbackHost.GetProviderProperties());

            var systemDSN = dataProviderProperties.ConnectionStrings[_count];
            var parsedQuery = FindReplacementParameters(_readerProperties.Query, systemDSN,
                _readerProperties.QueryVariables);
            var connectionString = $"Dsn={systemDSN};";
            LogEvent(LogMessageType.Warning, connectionString);
            var odbcConnection = new OdbcConnection(connectionString);
            var command = new OdbcCommand(parsedQuery, odbcConnection);
            command.CommandTimeout = _readerProperties.Timeout;
            odbcConnection.Open();
            var odbcReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            _connections.Add(new ConnectionMetadata(systemDSN, odbcConnection, odbcReader));
            _count++;
            _currentConnection = _connections[_count - 1];
            LogEvent(LogMessageType.Progress, "Open end");

        }

        private string FindReplacementParameters(string query, string systemDSN, IDictionary<string, string> replacementValues)
        {
            var pattern = "!{lastvalue:(?<param>[^!{{}}]+)}!";
            var stringBuilder = new StringBuilder(query);
            var regex = new Regex(pattern);
            regex.Matches(query);
            for (var match = regex.Match(stringBuilder.ToString()); match.Success; match = regex.Match(stringBuilder.ToString()))
            {
                //Get key based off the data source used and the column specified
                var key = $"{systemDSN}:{match.Groups["param"].Value}";
                LogEvent(LogMessageType.Progress, key);
                if (!replacementValues.ContainsKey(key))
                {
                    replacementValues.Add(key, "0");
                }
                LogEvent(LogMessageType.Progress, $"Replacing Last Value query parameter at position {match.Index}");
                var replacementValue = replacementValues[key];
                stringBuilder.Remove(match.Index, match.Length);
                stringBuilder.Insert(match.Index, replacementValue);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Validates DataReader settings when the DataProvider gets updated.
        /// </summary>
        /// <returns>A <see cref="IWorkbenchDataReaderPluginValidationResult"/></returns>
        public IWorkbenchDataReaderPluginValidationResult ValidateProviderSettings()
        {
            return null;
        }

        /// <summary>
        /// Send log events back to the Workbench host
        /// </summary>
        /// <param name="logMessageType">The <see cref="LogMessageType"/> of the log</param>
        /// <param name="message">The message to log</param>
        /// <param name="ex">The optional <see cref="Exception"/> to log</param>
        private void LogEvent(LogMessageType logMessageType, string message, Exception ex = null)
        {
            //Better used for testing to reduce the amount of I/O
            //_callbackHost.LogEvent(logMessageType, message, ex);
        }
    }

    /// <summary>
    /// Stores the values from the user control for the reader to use.
    /// </summary>
    public class MyDataReaderProperties
    {

        public MyDataReaderProperties()
        {
            Timeout = 300;
            QueryVariables = new Dictionary<string, string>();
        }

        /// <summary>
        /// Demonstrates how to use the properties properly between the Control and the Reader
        /// </summary>
        public string Query { get; set; }

        public IDictionary<string, string> QueryVariables { get; set; }

        public int Timeout { get; set; }
    }

    public class ConnectionMetadata
    {

        public ConnectionMetadata(string dsn, OdbcConnection connection, OdbcDataReader reader)
        {
            DSN = dsn;
            Connection = connection;
            Reader = reader;
        }
        
        public string DSN { get; }
        public OdbcConnection Connection { get; }
        public OdbcDataReader Reader { get; }
    }
}