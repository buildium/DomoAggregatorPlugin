﻿using DomoAggregatorPlugin.Controls;
using PluginUtil;
using System.AddIn;
using System.Collections.Generic;
using System.Linq;
using WorkbenchPlugin.Views.Plugin.v3;
using WorkbenchPlugin.Views.Plugin.v3.DataProvider;


namespace DomoAggregatorPlugin
{
    /// <summary>
    /// Provides a way to add a way to add files into Domo Workbench.
    /// </summary>
    [AddIn("DomoAggregatorPlugin Provider", Publisher = "", Description = "DomoAggregatorPlugin Workbench Plugin", Version = "1.0.0.0")]
    public class DataProvider : IWorkbenchDataProviderPlugin
    {
        private IWorkbenchHost _callbackHost;
        private bool _cancelRequested = false;

        /// <summary>
        /// Allows a reader to configure a provider such that it can copy files locally while executing jobs.
        /// </summary>
        public bool AllowLocalFileCopy { get; set; }

        /// <summary>
        /// The type served by this DataProvider
        /// </summary>
        public string DataProviderType
        {
            get { return DataSetTypes.Odbc; }
        }

        /// <summary>
        /// The description for the data provider instance. (usually returns something that describes specifics of the provider, such as filename, URL, ODBC connection string, etc.)
        /// May be displayed in log files, or in the user interface.
        /// For example on the Job Source page when the panel is collapsed, this is shown.
        /// </summary>
        public string Description
        {
            get { return "DomoAggregatorPlugin plugin"; }
        }

        /// <summary>
        /// Returns the user-friendly name for this DataProvider.
        /// </summary>
        public string Name
        {
            get { return "DomoAggregatorPlugin"; }
        }

        /// <summary>
        /// Returns the type of data this provider creates.
        /// This should be used with <see cref="SourceType"/>
        /// </summary>
        public string ProviderSourceType
        {
            get { return SourceType.DbConnection; }
        }

        /// <summary>
        /// Whether or not the DataProvider requires a 32bit process.
        /// </summary>
        public bool Requires32Bit
        {
            get { return false; }
        }

        /// <summary>
        /// Whether or not the DataProvider requires impersonation.
        /// </summary>
        public bool RequiresImpersonation
        {
            get { return false; }
        }

        /// <summary>
        /// Whether or not the DataProvider requires an interactive session.
        /// </summary>
        public bool RequiresInteractiveSession
        {
            get { return false; }
        }

        /// <summary>
        /// Whether or not the DataProvider requires validation.
        /// </summary>
        public bool RequiresVerification
        {
            get { return false; }
        }

        /// <summary>
        /// Whether files provided by this provider can be monitored via the file system for changes that trigger job execution.
        /// </summary>
        public bool SupportsFileWatcher
        {
            get { return false; }
        }

        /// <summary>
        /// Called to apply changes to a job. These changes are generated by a call to IWorkbenchHost.SetJobChange().
        /// </summary>
        /// <param name="jobChanges"></param>
        public void ApplyJobChanges(IList<IWorkbenchJobChange> jobChanges)
        {
        }

        /// <summary>
        /// Requests a cancel to any running operation.
        /// </summary>
        public void Cancel()
        {
            _cancelRequested = true;
        }

        /// <summary>
        /// Cleans up any resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// For DataProviders that produce a DatabaseConnection data source, this method will
        /// return the connection helper.
        /// </summary>
        /// <returns></returns>
        public IWorkbenchDataProviderPluginDatabaseHandler GetDatabaseResultHandler()
        {
            throw new WorkbenchPluginException(this.GetType().Name + " does not support GetDatabaseResultHandler");
        }

        /// <summary>
        /// Gets the editor for this data provider.
        /// </summary>
        /// <returns>Returns the UI editor for this data provider.</returns>
        public IWorkbenchDataProviderPluginEditor GetDataProviderEditor()
        {
            return new DataProviderControl(new DataProviderControlViewModel(_callbackHost));
        }

        /// <summary>
        /// Gets the complete schema of the data source
        /// </summary>
        /// <param name="collection">The collection type to retrieve</param>
        /// <param name="filters">Any additional filters</param>
        /// <returns></returns>
        public List<Dictionary<string, string>> GetSchema(string collection, string[] filters)
        {
            throw new WorkbenchPluginException(this.GetType().Name + " does not support GetSchema");
        }

        /// <summary>
        /// For DataProviders that produce a FlatFile data source, this method will
        /// return the file path.
        /// </summary>
        /// <returns></returns>
        public string GetSourceFilePath()
        {
            return null;
        }

        /// <summary>
        /// Initializes the DataProvider
        /// </summary>
        /// <param name="hostObj">The <see cref="IWorkbenchHost"/> used to pull information from Workbench</param>
        public void Initialize(IWorkbenchHost hostObj)
        {
            _callbackHost = hostObj;
        }

        public bool SupportsHighBandwidthExecutionPipeline
        {
            get { return false; }
        }
  
    }

    /// <summary>
    /// Stores the values from the user control for the provider to use.
    /// </summary>
    public class MyDataProviderProperties
    {

        public MyDataProviderProperties()
        {
            // optional default value
            DataSources = "subscribera,subscribera2,subscriberb,subscriberc,subscriberd,subscribere,subscriberf,subscriberg,subscriberh,subscriberi";
        }

        /// <summary>
        /// The database connection to grab the data from
        /// </summary>
        public string DataSources { get; set; }

        public List<string> ConnectionStrings => DataSources.Split(',').ToList();
    }
}