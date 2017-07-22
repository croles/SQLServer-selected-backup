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
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace SQLServerSelectiveBackup
{
    public partial class MainForm : Form
    {
        private SqlConnection connection;
        private const string filename = "DataTables.sbak";

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Connects to the database and loads a list of tables to backup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Checks or unchecks the elements of the table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvTables.Rows) row.Cells[0].Value = chkAll.Checked;
        }

        /// <summary>
        /// After connection activations
        /// </summary>
        private void ActivateBackupComponents()
        {
            chkAll.Visible = true;
            dgvTables.Visible = true;
            btnBackup.Visible = true;
            btnRestore.Visible = true;
        }

        /// <summary>
        /// Creates a backup in a file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        dt.TableName = row.Cells[1].Value.ToString();
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
            try
            {
                if (File.Exists(filename)) File.Delete(filename);

                FileStream stream = File.Create(filename);
                var formatter = new BinaryFormatter();

                using (GZipStream compressionStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    formatter.Serialize(compressionStream, lTables);
                }
                stream.Close();
            }catch(Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.ToString() + ".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show("Data has been saved.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


        /// <summary>
        /// Restores backup information to DB.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    MessageBox.Show("No backup file found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                List<DataTable> lTables = LoadTablesFromFile();
                if (lTables.Count > 0)
                {
                    var bulkCopy = new SqlBulkCopy(connection);
                    foreach (DataTable table in lTables)
                    {
                        SqlCommand cmd = new SqlCommand("Delete FROM " + table.TableName, connection);
                        cmd.ExecuteNonQuery();
                        bulkCopy.DestinationTableName = table.TableName;
                        try
                        {
                            bulkCopy.WriteToServer(table);
                        }catch(Exception ex) { MessageBox.Show("Error restoring table: " + table.TableName + " -> " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    }
                    MessageBox.Show("Data has been restored.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error restoring backup data: " + ex.ToString() + ".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads the list of DataTable from file
        /// </summary>
        /// <returns>List of DB tables</returns>
        private List<DataTable> LoadTablesFromFile()
        {
            List<DataTable> lTables = new List<DataTable>();

            var formatter = new BinaryFormatter();
            FileStream stream = File.OpenRead(filename);
            using (GZipStream decompressionStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                lTables = (List<DataTable>)formatter.Deserialize(decompressionStream);
            }
            stream.Close();

            return lTables;
        }


    }
}
