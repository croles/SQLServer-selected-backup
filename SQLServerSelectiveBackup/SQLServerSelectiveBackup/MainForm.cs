using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SQLServerSelectiveBackup
{
    public partial class MainForm : Form
    {
        private SqlConnection connection;

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                //Connection string
                string connectionString = "Data Source=" + txtServer.Text + ";Initial Catalog=" + txtCatalog.Text + ";"
                    + "User id=" + txtUser.Text + ";" + "Password=" + txtPassword.Text + ";";
                if (txtUser.Text == "") connectionString = "Data Source=" + txtServer.Text + ";Initial Catalog=" + txtCatalog.Text + ";" + "Integrated Security=true";

                //Create a new database connection and get tables
                connection = new SqlConnection(connectionString);
                connection.Open();
                DataTable Tables = connection.GetSchema("Tables");
                foreach (DataRow row in Tables.Rows) dgvTables.Rows.Add(false, row[2].ToString());

                ActivateBackupComponents();
            }

            catch (Exception ex)
            {
                MessageBox.Show("An error occurred due to the following exception: " + ex.ToString() + ".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvTables.Rows) row.Cells[0].Value = chkAll.Checked;
        }

        private void ActivateBackupComponents()
        {
            chkAll.Visible = true;
            dgvTables.Visible = true;
            btnBackup.Visible = true;
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            List<DataTable> lTables = new List<DataTable>();

            foreach (DataGridViewRow row in dgvTables.Rows)
            {
                if ((bool)row.Cells[0].Value)
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("SELECT * FROM " + row.Cells[1].Value.ToString(), connection);
                        DataTable dt = new DataTable();

                        dt.Load(cmd.ExecuteReader());
                        lTables.Add(dt);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Error retrieving table(" + row.Cells[1].Value.ToString() + "): " + ex.ToString() + "."
                            , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            if (lTables.Count > 0) SaveTablesData(lTables);
            else MessageBox.Show("No data has been stored.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Saves a list of DataTable into the backup file
        /// </summary>
        /// <param name="lTables">List of DataTable from de BD.</param>
        private void SaveTablesData(List<DataTable> lTables)
        {
            const string filename = "DataTables.sbak";

            try
            {
                if (File.Exists(filename)) File.Delete(filename);

                FileStream stream = File.Create(filename);
                var formatter = new BinaryFormatter();

                formatter.Serialize(stream, lTables);
                stream.Close();
            }catch(Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.ToString() + ".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show("Data has been saved.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
