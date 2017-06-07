// Decompiled with JetBrains decompiler
// Type: DomoDataReaders.OdbcDataReader.Controls.OdbcEditor
// Assembly: DomoDataReaders, Version=4.0.5850.36731, Culture=neutral, PublicKeyToken=null
// MVID: 2B9CB23F-AD68-4C35-8601-31AD23BC1C4E
// Assembly location: C:\Users\josh.molloy\Downloads\DomoDataReaders.dll

/*using ActiproSoftware.SyntaxEditor;
using ActiproSoftware.SyntaxEditor.Addons.Dynamic;
using ActiproSoftware.SyntaxEditor.Languages;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Container;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using DevExpress.XtraBars.Docking2010.Views;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraSpreadsheet;
using WorkbenchSDK.Configuration;
using WorkbenchSDK.Data;
using WorkbenchSDK.Data.DataReader;

namespace DomoDataReaders.OdbcDataReader.Controls
{
  public class OdbcEditor : UserControl, IDataReaderEditor
  {
    private DataTable dataTable = new DataTable();
    private DataTable globalDataTable = new DataTable();
    private OdbcReaderProperties originalProperties;
    private bool loading;
    private WorkbenchJob job;
    private IContainer components;
    private LabelControl labelControl1;
    private SpinEdit timoutEdit;
    private XtraTabControl xtraTabControl;
    private XtraTabPage xtraTabPage1;
    private ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor;
    private XtraTabPage xtraTabPage2;
    private GridControl gridControl;
    private DevExpress.XtraGrid.Views.Grid.GridView gridView;
    private RepositoryItemComboBox repositoryItemComboBox;
    private SqlDynamicSyntaxLanguage sqlDynamicSyntaxLanguage1;
    private XtraTabPage xtraTabPage3;
    private GridControl gridControlGlobal;
    private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
    private RepositoryItemComboBox repositoryItemComboBox1;

    public bool AllowEditorClose
    {
      get
      {
        if (((object) this.originalProperties).Equals((object) this.EditorSettings))
          return true;
        return XtraMessageBox.Show((System.Windows.Forms.IWin32Window) this.ParentForm, "Your changes have not been saved. Do you want to close without saving?", "Unsaved Changes", MessageBoxButtons.YesNo) == DialogResult.Yes;
      }
    }

    public bool AllowEditorSave
    {
      get
      {
        return true;
      }
    }

    public IEnumerable<EditorActionButton> EditorRibbonButtons
    {
      get
      {
        return (IEnumerable<EditorActionButton>) null;
      }
    }

    private OdbcReaderProperties EditorSettings
    {
      get
      {
        OdbcReaderProperties readerProperties = new OdbcReaderProperties()
        {
          Timeout = int.Parse(((BaseEdit) this.timoutEdit).get_EditValue().ToString()),
          Query = ((Control) this.syntaxEditor).Text
        };
        foreach (DataRow row in (InternalDataCollectionBase) this.dataTable.Rows)
          readerProperties.QueryVariables.Add(row[0].ToString(), row[1].ToString());
        return readerProperties;
      }
      set
      {
        this.loading = true;
        ((Control) this.timoutEdit).Text = value.Timeout < 300 ? "300" : value.Timeout.ToString();
        ((Control) this.syntaxEditor).Text = value.Query;
        this.dataTable.Clear();
        foreach (KeyValuePair<string, string> queryVariable in value.QueryVariables)
          this.dataTable.Rows.Add((object) queryVariable.Key, (object) queryVariable.Value);
        this.loading = false;
      }
    }

    public WorkbenchJob Job
    {
      get
      {
        return this.job;
      }
      set
      {
        this.job = value;
        OdbcReaderProperties readerProperties = (OdbcReaderProperties) value.GetDataReaderProperties(typeof (OdbcReaderProperties));
        this.originalProperties = (OdbcReaderProperties) ((DataReaderProperties) readerProperties).Clone();
        this.EditorSettings = readerProperties;
      }
    }

    public OdbcEditor()
    {
      this.InitializeComponent();
      this.dataTable.Columns.Add("Column");
      this.dataTable.Columns.Add("Value");
      this.dataTable.RowChanged += new DataRowChangeEventHandler(this.dataTable_RowChanged);
      this.dataTable.RowDeleted += new DataRowChangeEventHandler(this.dataTable_RowDeleted);
      this.gridControl.set_DataSource((object) this.dataTable);
      ((ColumnView) this.gridView).get_Columns().get_Item(0).set_ColumnEdit((RepositoryItem) this.repositoryItemComboBox);
      this.globalDataTable.Columns.Add("Column");
      this.globalDataTable.Columns.Add("Value");
      this.globalDataTable.RowChanged += new DataRowChangeEventHandler(this.globalDataTable_RowChanged);
      this.globalDataTable.RowDeleted += new DataRowChangeEventHandler(this.globalDataTable_RowDeleted);
      this.gridControlGlobal.set_DataSource((object) this.globalDataTable);
      this.LoadGlobalVariables();
    }

    private void globalDataTable_RowDeleted(object sender, DataRowChangeEventArgs e)
    {
      this.SaveGlobalsVariables();
    }

    private void SaveGlobalsVariables()
    {
      Dictionary<string, string> values = new Dictionary<string, string>();
      foreach (DataRow row in (InternalDataCollectionBase) this.globalDataTable.Rows)
        values.Add((string) row["Column"], (string) row["Value"]);
      GlobalReplacements.Instance.SetValues(values);
      GlobalReplacements.Instance.Save();
    }

    private void globalDataTable_RowChanged(object sender, DataRowChangeEventArgs e)
    {
      this.SaveGlobalsVariables();
    }

    private void LoadGlobalVariables()
    {
      this.globalDataTable.Clear();
      foreach (KeyValuePair<string, string> keyValuePair in GlobalReplacements.Instance.Values)
        this.globalDataTable.Rows.Add((object) keyValuePair.Key, (object) keyValuePair.Value);
    }

    public Task LoadReaderPropertiesAsync(CancellationToken cancellationToken)
    {
      return Task.Factory.StartNew((Action) (() =>
      {
        OdbcReaderProperties odbcProperties = (OdbcReaderProperties) this.Job.GetDataReaderProperties(typeof (OdbcReaderProperties));
        ((CollectionBase) this.repositoryItemComboBox.get_Items()).Clear();
        using (List<WorkbenchDataSchema.SchemaColumn>.Enumerator enumerator = this.Job.get_WorkbenchConfig().get_DataSchema().get_Columns().GetEnumerator())
        {
          while (enumerator.MoveNext())
            this.repositoryItemComboBox.get_Items().Add((object) enumerator.Current.get_SourceColumn());
        }
        this.Invoke((Delegate) (() => this.EditorSettings = odbcProperties));
      }));
    }

    public void ReaderPropertiesSaved()
    {
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      OdbcReaderProperties readerProperties = (OdbcReaderProperties) this.Job.GetDataReaderProperties(typeof (OdbcReaderProperties));
      this.originalProperties = (OdbcReaderProperties) ((DataReaderProperties) readerProperties).Clone();
      this.EditorSettings = readerProperties;
    }

    private void OdbcPropertiesChanged(object sender, EventArgs e)
    {
      if (this.loading)
        return;
      OdbcReaderProperties readerProperties = (OdbcReaderProperties) this.Job.GetDataReaderProperties(typeof (OdbcReaderProperties));
      readerProperties.Timeout = int.Parse(((BaseEdit) this.timoutEdit).get_EditValue().ToString());
      readerProperties.Query = ((Control) this.syntaxEditor).Text;
      readerProperties.QueryVariables.Clear();
      foreach (DataRow row in (InternalDataCollectionBase) this.dataTable.Rows)
        readerProperties.QueryVariables.Add(row[0].ToString(), row[1].ToString());
      this.Job.SetDataReaderProperties(typeof (DomoDataReaders.OdbcDataReader.OdbcDataReader), (DataReaderProperties) readerProperties);
    }

    private void PropertiesChanging(object sender, ChangingEventArgs e)
    {
      this.OdbcPropertiesChanged(sender, (EventArgs) e);
    }

    private void syntaxEditor_KeyTyped(object sender, KeyTypedEventArgs e)
    {
      this.OdbcPropertiesChanged(sender, (EventArgs) e);
    }

    private void dataTable_RowChanged(object sender, DataRowChangeEventArgs e)
    {
      this.OdbcPropertiesChanged(sender, (EventArgs) e);
    }

    private void dataTable_RowDeleted(object sender, DataRowChangeEventArgs e)
    {
      this.OdbcPropertiesChanged(sender, (EventArgs) e);
    }

    private void gridView_CellValueChanged(object sender, CellValueChangedEventArgs e)
    {
      ((BaseView) this.gridView).UpdateCurrentRow();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (OdbcEditor));
      SuperToolTip superToolTip1 = new SuperToolTip();
      ToolTipItem toolTipItem1 = new ToolTipItem();
      Document document = new Document();
      SuperToolTip superToolTip2 = new SuperToolTip();
      ToolTipItem toolTipItem2 = new ToolTipItem();
      this.sqlDynamicSyntaxLanguage1 = new SqlDynamicSyntaxLanguage();
      this.labelControl1 = new LabelControl();
      this.timoutEdit = new SpinEdit();
      this.xtraTabControl = new XtraTabControl();
      this.xtraTabPage1 = new XtraTabPage();
      this.syntaxEditor = new ActiproSoftware.SyntaxEditor.SyntaxEditor();
      this.xtraTabPage2 = new XtraTabPage();
      this.gridControl = new GridControl();
      this.gridView = new GridView();
      this.repositoryItemComboBox = new RepositoryItemComboBox();
      this.xtraTabPage3 = new XtraTabPage();
      this.gridControlGlobal = new GridControl();
      this.gridView1 = new GridView();
      this.repositoryItemComboBox1 = new RepositoryItemComboBox();
      ((ISupportInitialize) this.timoutEdit.get_Properties()).BeginInit();
      ((ISupportInitialize) this.xtraTabControl).BeginInit();
      ((Control) this.xtraTabControl).SuspendLayout();
      ((Control) this.xtraTabPage1).SuspendLayout();
      ((Control) this.xtraTabPage2).SuspendLayout();
      ((ISupportInitialize) this.gridControl).BeginInit();
      ((ISupportInitialize) this.gridView).BeginInit();
      ((ISupportInitialize) this.repositoryItemComboBox).BeginInit();
      ((Control) this.xtraTabPage3).SuspendLayout();
      ((ISupportInitialize) this.gridControlGlobal).BeginInit();
      ((ISupportInitialize) this.gridView1).BeginInit();
      ((ISupportInitialize) this.repositoryItemComboBox1).BeginInit();
      this.SuspendLayout();
      ((DynamicSyntaxLanguage) this.sqlDynamicSyntaxLanguage1).set_DefinitionXml(componentResourceManager.GetString("sqlDynamicSyntaxLanguage1.DefinitionXml"));
      ((Control) this.labelControl1).Location = new Point(20, 14);
      ((Control) this.labelControl1).Name = "labelControl1";
      ((Control) this.labelControl1).Size = new Size(125, 13);
      ((Control) this.labelControl1).TabIndex = 1;
      ((Control) this.labelControl1).Text = "Query Execution Timeout:";
      ((BaseEdit) this.timoutEdit).set_EditValue((object) new Decimal(new int[4]
      {
        300,
        0,
        0,
        0
      }));
      ((Control) this.timoutEdit).Location = new Point(151, 11);
      ((Control) this.timoutEdit).Name = "timoutEdit";
      ((RepositoryItemButtonEdit) this.timoutEdit.get_Properties()).get_Buttons().AddRange(new EditorButton[1]
      {
        new EditorButton((ButtonPredefines) -5)
      });
      this.timoutEdit.get_Properties().set_IsFloatValue(false);
      ((RepositoryItemTextEdit) this.timoutEdit.get_Properties()).get_Mask().set_EditMask("N00");
      this.timoutEdit.get_Properties().set_MaxValue(new Decimal(new int[4]
      {
        999999999,
        0,
        0,
        0
      }));
      this.timoutEdit.get_Properties().set_MinValue(new Decimal(new int[4]
      {
        300,
        0,
        0,
        0
      }));
      ((Control) this.timoutEdit).Size = new Size(72, 20);
      ((Control) this.timoutEdit).TabIndex = 2;
      // ISSUE: method pointer
      ((BaseEdit) this.timoutEdit).add_EditValueChanging(new ChangingEventHandler((object) this, __methodptr(PropertiesChanging)));
      ((Control) this.xtraTabControl).Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      ((Control) this.xtraTabControl).Location = new Point(20, 48);
      ((Control) this.xtraTabControl).Name = "xtraTabControl";
      this.xtraTabControl.set_SelectedTabPage(this.xtraTabPage1);
      ((Control) this.xtraTabControl).Size = new Size(1129, 345);
      ((Control) this.xtraTabControl).TabIndex = 4;
      this.xtraTabControl.get_TabPages().AddRange(new XtraTabPage[3]
      {
        this.xtraTabPage1,
        this.xtraTabPage2,
        this.xtraTabPage3
      });
      ((Control) this.xtraTabPage1).Controls.Add((Control) this.syntaxEditor);
      ((Control) this.xtraTabPage1).Name = "xtraTabPage1";
      ((Control) this.xtraTabPage1).Padding = new Padding(10);
      this.xtraTabPage1.set_Size(new Size(1123, 317));
      toolTipItem1.set_Text("Enter a query for the data you want to retrieve.");
      superToolTip1.get_Items().Add((BaseToolTipItem) toolTipItem1);
      this.xtraTabPage1.set_SuperTip(superToolTip1);
      ((Control) this.xtraTabPage1).Text = "Query";
      ((Control) this.syntaxEditor).Dock = DockStyle.Fill;
      document.set_Language((SyntaxLanguage) this.sqlDynamicSyntaxLanguage1);
      this.syntaxEditor.set_Document(document);
      ((Control) this.syntaxEditor).Location = new Point(10, 10);
      ((Control) this.syntaxEditor).Margin = new Padding(2);
      ((Control) this.syntaxEditor).Name = "syntaxEditor";
      ((Control) this.syntaxEditor).Size = new Size(1103, 297);
      ((Control) this.syntaxEditor).TabIndex = 4;
      // ISSUE: method pointer
      this.syntaxEditor.add_KeyTyped(new KeyTypedEventHandler((object) this, __methodptr(syntaxEditor_KeyTyped)));
      ((Control) this.xtraTabPage2).Controls.Add((Control) this.gridControl);
      ((Control) this.xtraTabPage2).Name = "xtraTabPage2";
      ((Control) this.xtraTabPage2).Padding = new Padding(10);
      this.xtraTabPage2.set_Size(new Size(1123, 317));
      toolTipItem2.set_Text("Define replacement variables for use in your query. ");
      superToolTip2.get_Items().Add((BaseToolTipItem) toolTipItem2);
      this.xtraTabPage2.set_SuperTip(superToolTip2);
      ((Control) this.xtraTabPage2).Text = "Replacement Variables";
      ((Control) this.gridControl).Dock = DockStyle.Fill;
      ((Control) this.gridControl).Location = new Point(10, 10);
      this.gridControl.set_MainView((BaseView) this.gridView);
      ((Control) this.gridControl).Name = "gridControl";
      ((EditorContainer) this.gridControl).get_RepositoryItems().AddRange(new RepositoryItem[1]
      {
        (RepositoryItem) this.repositoryItemComboBox
      });
      ((Control) this.gridControl).Size = new Size(1103, 297);
      ((Control) this.gridControl).TabIndex = 0;
      this.gridControl.set_UseEmbeddedNavigator(true);
      this.gridControl.get_ViewCollection().AddRange(new BaseView[1]
      {
        (BaseView) this.gridView
      });
      ((BaseView) this.gridView).set_GridControl(this.gridControl);
      ((BaseView) this.gridView).set_Name("gridView");
      ((ColumnViewOptionsBehavior) this.gridView.get_OptionsBehavior()).set_AllowAddRows((DefaultBoolean) 0);
      ((ColumnViewOptionsBehavior) this.gridView.get_OptionsBehavior()).set_AllowDeleteRows((DefaultBoolean) 0);
      this.gridView.get_OptionsNavigation().set_AutoFocusNewRow(true);
      ((ColumnViewOptionsView) this.gridView.get_OptionsView()).set_AnimationType((GridAnimationType) 1);
      this.gridView.get_OptionsView().set_NewItemRowPosition((NewItemRowPosition) 1);
      this.gridView.get_OptionsView().set_ShowGroupPanel(false);
      // ISSUE: method pointer
      ((ColumnView) this.gridView).add_CellValueChanged(new CellValueChangedEventHandler((object) this, __methodptr(gridView_CellValueChanged)));
      ((RepositoryItem) this.repositoryItemComboBox).set_AutoHeight(false);
      ((RepositoryItemButtonEdit) this.repositoryItemComboBox).get_Buttons().AddRange(new EditorButton[1]
      {
        new EditorButton((ButtonPredefines) -5)
      });
      ((RepositoryItem) this.repositoryItemComboBox).set_Name("repositoryItemComboBox");
      ((Control) this.xtraTabPage3).Controls.Add((Control) this.gridControlGlobal);
      ((Control) this.xtraTabPage3).Name = "xtraTabPage3";
      ((Control) this.xtraTabPage3).Padding = new Padding(10);
      this.xtraTabPage3.set_Size(new Size(1123, 317));
      ((Control) this.xtraTabPage3).Text = "Global Replacement Variables";
      ((Control) this.gridControlGlobal).Dock = DockStyle.Fill;
      ((Control) this.gridControlGlobal).Location = new Point(10, 10);
      this.gridControlGlobal.set_MainView((BaseView) this.gridView1);
      ((Control) this.gridControlGlobal).Name = "gridControlGlobal";
      ((EditorContainer) this.gridControlGlobal).get_RepositoryItems().AddRange(new RepositoryItem[1]
      {
        (RepositoryItem) this.repositoryItemComboBox1
      });
      ((Control) this.gridControlGlobal).Size = new Size(1103, 297);
      ((Control) this.gridControlGlobal).TabIndex = 1;
      this.gridControlGlobal.set_UseEmbeddedNavigator(true);
      this.gridControlGlobal.get_ViewCollection().AddRange(new BaseView[1]
      {
        (BaseView) this.gridView1
      });
      ((BaseView) this.gridView1).set_GridControl(this.gridControlGlobal);
      ((BaseView) this.gridView1).set_Name("gridView1");
      ((ColumnViewOptionsBehavior) this.gridView1.get_OptionsBehavior()).set_AllowAddRows((DefaultBoolean) 0);
      ((ColumnViewOptionsBehavior) this.gridView1.get_OptionsBehavior()).set_AllowDeleteRows((DefaultBoolean) 0);
      this.gridView1.get_OptionsNavigation().set_AutoFocusNewRow(true);
      ((ColumnViewOptionsView) this.gridView1.get_OptionsView()).set_AnimationType((GridAnimationType) 1);
      this.gridView1.get_OptionsView().set_NewItemRowPosition((NewItemRowPosition) 1);
      this.gridView1.get_OptionsView().set_ShowGroupPanel(false);
      ((RepositoryItem) this.repositoryItemComboBox1).set_AutoHeight(false);
      ((RepositoryItemButtonEdit) this.repositoryItemComboBox1).get_Buttons().AddRange(new EditorButton[1]
      {
        new EditorButton((ButtonPredefines) -5)
      });
      ((RepositoryItem) this.repositoryItemComboBox1).set_Name("repositoryItemComboBox1");
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.xtraTabControl);
      this.Controls.Add((Control) this.timoutEdit);
      this.Controls.Add((Control) this.labelControl1);
      this.Name = "OdbcEditor";
      this.Padding = new Padding(10);
      this.Size = new Size(1169, 406);
      ((ISupportInitialize) this.timoutEdit.get_Properties()).EndInit();
      ((ISupportInitialize) this.xtraTabControl).EndInit();
      ((Control) this.xtraTabControl).ResumeLayout(false);
      ((Control) this.xtraTabPage1).ResumeLayout(false);
      ((Control) this.xtraTabPage2).ResumeLayout(false);
      ((ISupportInitialize) this.gridControl).EndInit();
      ((ISupportInitialize) this.gridView).EndInit();
      ((ISupportInitialize) this.repositoryItemComboBox).EndInit();
      ((Control) this.xtraTabPage3).ResumeLayout(false);
      ((ISupportInitialize) this.gridControlGlobal).EndInit();
      ((ISupportInitialize) this.gridView1).EndInit();
      ((ISupportInitialize) this.repositoryItemComboBox1).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}*/
