using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.SqlClient;

namespace BackUpBase
{
    public partial class Form1 : Form
    {
        private SqlConnection conn;
        private SqlCommand command;
        private SqlDataReader reader;  


        string sql = "";
        string connectionString = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BtnDisconnect.Enabled = false;
            cmdDatabases.Enabled = false;
            BtnBackup.Enabled = false;
            BtnRestore.Enabled = false;


            BtnBackupAll.Enabled = false;
            BtnBackupAll.Visible = false;
            checkBox1.Enabled = false;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                connectionString = "Data Source = " +textDataSource.Text + "; User Id = " +txtUserId.Text+ "; Password = " +txtPassword.Text+";";
                conn = new SqlConnection(connectionString);
                conn.Open();

                //sql = "EXEC sp_databases";

                // SERVER2020\SQLEXPRESS

                sql = "SELECT * FROM sys.databases d WHERE d.database_id > 4";

                command = new SqlCommand(sql, conn);
                reader = command.ExecuteReader();
                cmdDatabases.Items.Clear();

                while(reader.Read())
                {
                    cmdDatabases.Items.Add(reader[0].ToString());
                }

                reader.Dispose();
                conn.Close();
                conn.Dispose();

                textDataSource.Enabled = false;
                txtPassword.Enabled = false;
                txtUserId.Enabled = false;
                BtnConnect.Enabled = false;
                BtnDisconnect.Enabled = true;

                BtnBackup.Enabled = true;
                BtnRestore.Enabled = true;

                cmdDatabases.Enabled = true;

                checkBox1.Enabled = true;

                MessageBox.Show("Подключение к источнику данных произведено успешно.\nТеперь необходимо выбрать базу данных для резервного копирования!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            textDataSource.Enabled = true;
            txtPassword.Enabled = true;
            txtUserId.Enabled = true;
            cmdDatabases.Enabled = false;

            BtnBackup.Enabled = false;
            BtnRestore.Enabled = false;
            BtnConnect.Enabled = true;

            checkBox1.Enabled = false;
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmdDatabases.Text.CompareTo("") == 0)
                {
                    MessageBox.Show("База Данных для резервной копии не выбрана!");
                    return;
                }

                conn = new SqlConnection(connectionString);
                conn.Open();
                sql = "BACKUP DATABASE [" + cmdDatabases.Text + "] TO DISK = '" + txtBackupLoc.Text + "\\" + cmdDatabases.Text + "-" + DateTime.Now.Ticks.ToString() + ".bak'";

                command = new SqlCommand(sql, conn);
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
                
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Резервное копирование произведено успешно !");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            if(dlg.ShowDialog() == DialogResult.OK) 
            {
                txtBackupLoc.Text = dlg.SelectedPath;
            }
        }

        private void BtnDBFileBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Backup Files(*.bak)|*.bak|All Files(*.*)|*.*";
            dlg.FilterIndex = 0;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtRestoreFileLoc.Text = dlg.FileName;

            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            try
            {

                if (cmdDatabases.Text.CompareTo("") == 0)
                {
                    MessageBox.Show("Резервная копия базы данных не выбрана!");
                    return;
                }

                conn = new SqlConnection(connectionString);
                conn.Open();
                sql = "Alter Database " + cmdDatabases.Text + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                sql += "Restore Database " + cmdDatabases.Text + " FROM Disk = '" + txtRestoreFileLoc.Text + "' WITH REPLACE;";
                command = new SqlCommand(sql, conn);
                command.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Восстановление базы данных из резервной копии произведено успешно !");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender; // приводим отправителя к элементу типа CheckBox
            if (checkBox.Checked == true)
            {
                BtnBackupAll.Enabled = true;
                BtnBackupAll.Visible = true;
            }
            else
            {
                BtnBackupAll.Enabled = false;
                BtnBackupAll.Visible = false;
                //MessageBox.Show("Флажок " + checkBox.Text + "  теперь не отмечен");
            }
        }

        private void BtnBackupAll_Click(object sender, EventArgs e)
        {
                       
             try 
            {

                if (txtBackupLoc.Text.CompareTo("") == 0)
                {
                    MessageBox.Show("Выберите местоположение для резервной копии!");
                    return;
                }

                conn = new SqlConnection(connectionString);
                conn.Open();
                sql = "BACKUP DATABASE ALLORG" + " TO DISK = '" + txtBackupLoc.Text + "\\" + cmdDatabases.Text + "-" + DateTime.Now.Ticks.ToString() + ".bak'";
                command = new SqlCommand(sql, conn);
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Резервное копирование ALLORG успешно произведено !");

                conn = new SqlConnection(connectionString);
                conn.Open();
                sql = "BACKUP DATABASE EX_BD_PRIM" + " TO DISK = '" + txtBackupLoc.Text + "\\" + cmdDatabases.Text + "-" + DateTime.Now.Ticks.ToString() + ".bak'";
                command = new SqlCommand(sql, conn);
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Резервное копирование EX_BD_PRIM успешно произведено !");

                conn = new SqlConnection(connectionString);
                conn.Open();
                sql = "BACKUP DATABASE Lab" + " TO DISK = '" + txtBackupLoc.Text + "\\" + cmdDatabases.Text + "-" + DateTime.Now.Ticks.ToString() + ".bak'";
                command = new SqlCommand(sql, conn);
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Резервное копирование Lab успешно произведено !");

                conn = new SqlConnection(connectionString);
                conn.Open();
                sql = "BACKUP DATABASE EX_BD" + " TO DISK = '" + txtBackupLoc.Text + "\\" + cmdDatabases.Text + "-" + DateTime.Now.Ticks.ToString() + ".bak'";
                command = new SqlCommand(sql, conn);
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Резервное копирование EX_BD успешно произведено !");

                conn = new SqlConnection(connectionString);
                conn.Open();
                sql = "BACKUP DATABASE ["+ "1SBASE" + "] TO DISK = '" + txtBackupLoc.Text + "\\" + cmdDatabases.Text + "-" + DateTime.Now.Ticks.ToString() + ".bak'";
                command = new SqlCommand(sql, conn);
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Резервное копирование 1SBASE успешно произведено !");

                conn = new SqlConnection(connectionString);
                conn.Open();
                sql = "BACKUP DATABASE EX_BD_USER" + " TO DISK = '" + txtBackupLoc.Text + "\\" + cmdDatabases.Text + "-" + DateTime.Now.Ticks.ToString() + ".bak'";
                command = new SqlCommand(sql, conn);
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Резервное копирование EX_BD_USER успешно произведено !");

                conn = new SqlConnection(connectionString);
                conn.Open();
                sql = "BACKUP DATABASE FOND" + " TO DISK = '" + txtBackupLoc.Text + "\\" + cmdDatabases.Text + "-" + DateTime.Now.Ticks.ToString() + ".bak'";
                command = new SqlCommand(sql, conn);
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Резервное копирование FOND успешно произведено !");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("");
            }
            
        }
    }
}
