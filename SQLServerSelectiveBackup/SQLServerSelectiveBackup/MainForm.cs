using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SQLServerSelectiveBackup
{
    public partial class MainForm : Form
    {
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
                SqlConnection connection = new SqlConnection(connectionString);
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
    }
}
