using System;
using System.IO;
using System.Windows.Forms;
using Ring;
using Ring.Data;
//using Ring.Adapters.SQLite;
using Ring.Adapters.PostgreSQL;

namespace SchemaMgr
{

    public partial class frmMain : Form
    {
        private const string SQLITE_CONN_STRING = @"Data Source=c:\temp\meta.db;Version=3;UseUTF16Encoding=False;";

        private const string POSTGRE_CONN_STRING1 = "User ID=postgres; Password=Start$0123;" +
            "Host=127.0.0.1;Port=5432;Database=postgres; Pooling=false;";
	    private const string POSTGRE_CONN_STRING = "User ID=postgres; Password=sa;" +
	                                               "Host=127.0.0.1;Port=5432;Database=postgres; Pooling=false;";
		private const string POSTGRE_CONN_STRING2 =
            "Data Source=127.0.0.1;location=postgres;User ID=Root; password=sa;timeout=1000;";

        public frmMain()
        {
            InitializeComponent();
            var dbConnection = new DbConnectionAdapter(new Configuration("sqlite", POSTGRE_CONN_STRING));
            GlobalContext.Properties[PropertyType.MetaDataConnection] = dbConnection;
		}

        private void cmdSelect_Click(object sender, EventArgs e)
        {
            if (browseNewSchema.ShowDialog() == DialogResult.OK)
                txtSelectedFile.Text = browseNewSchema.FileName;
            cmdUpgrade.Enabled = true;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            txtSelectedFile.Width = Math.Max(this.Width -412 + 287, 280);
            btnSelect.Left = Math.Max(Width - 101,305);
            txtOutput.Width = Math.Max(Width - 412 + 372, 372);
            txtDriver.Width = Math.Max(Width - 412 + 295, 295);
        }
        
        /// <summary>
        /// connection string for ole db connection 
        /// </summary>
        private void txtDriver_TextChanged(object sender, EventArgs e)
        {
            cmdUpgrade.Enabled = (txtDriver.Text.Length > 0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadConfig();
            cmdUpgrade.Enabled = (txtDriver.Text.Length > 0);
        }

        /// <summary>
        /// !!! Upgrade  !!!!!
        /// </summary>
        private void cmdUpgrade_Click(object sender, EventArgs e)
        {
            SaveConfig();
            txtOutput.Clear();
            var startTime = DateTime.Now;
	        var feedback = string.Empty;
            AppendLine("Start validation at: " + startTime.ToString());
	        var schemaMgr = new Ring.Schema.SchemaMgr();
	        var fs = new FileStream(txtSelectedFile.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using (var sr = new StreamReader(fs))
            {
	            var jobId = schemaMgr.Upgrade(fs, DatabaseProvider.PostgreSql, out feedback);
            }
            TimeSpan tsp = DateTime.Now - startTime;
            AppendLine("Process time: " + tsp.ToString());
        }

        private void SaveConfig()
        {
            IniParser ini = new IniParser();
            ini.AddSetting("Application", "default_ole_db_source", txtDriver.Text);
            ini.AddSetting("Application", "last_schema", txtSelectedFile.Text);
            ini.SaveSettings();
        }

        private void LoadConfig()
        {
            IniParser ini = new IniParser();
            txtDriver.Text = ini.GetSetting("Application", "default_ole_db_source");
            txtSelectedFile.Text = ini.GetSetting("Application", "last_schema");
        }
        public void AppendLine( string value)
        {
            if (txtOutput.Text.Length == 0) txtOutput.Text = value;
            else txtOutput.AppendText("\r\n" + value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
	        //Test.TestPerf(null, SchemaLoadType.Full);

        }

		private void button2_Click(object sender, EventArgs e)
		{
			Ring.Schema.SchemaMgr.Export(@"c:\\temp\\text.xml", 9300);
		}

		private void btnImportData_Click(object sender, EventArgs e)
		{
			if (browseData.ShowDialog() == DialogResult.OK)
			{
				var import = new Import();
				import.ParseFile(browseData.FileName, @"RpgSheet");
			}
		}
	}
}
