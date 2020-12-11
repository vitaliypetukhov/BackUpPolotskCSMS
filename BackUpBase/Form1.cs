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

using System.IO.Compression;

namespace BackUpBase
{
    

    public partial class Form1 : Form
    {
        private SqlConnection conn;
        private SqlCommand command;
        private SqlDataReader reader;  

        string sql = "";
        string connectionString = "";

        public string[] server2020arr = 
        { 
            "SERVER2020", 
            "server2020", 
            "Server2020", 
            "SeRvEr2020", 
            "sERVER2020",
            "sErVeR2020"
        };

        public string[] winserverarr =
        {
            "WINSERVER",
            "Winserver",
            "wINSERVER",
            "winserver",
            "WiNsErVeR",
            "wInSeRvEr"
        };

        public Form1()
        {
            InitializeComponent();

            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
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
            checkBox1.Visible = false;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                connectionString = "Data Source = " +textDataSource.Text + "; User Id = " +txtUserId.Text+ "; Password = " +txtPassword.Text+";";
                conn = new SqlConnection(connectionString);
                conn.Open();

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

                if(server2020arr.Contains(textDataSource.Text) || winserverarr.Contains(textDataSource.Text))
                {
                    checkBox1.Visible = true;
                    checkBox1.Enabled = true;
                }
                else
                {
                    checkBox1.Visible = false;
                    checkBox1.Enabled = false;
                }              

                //MessageBox.Show("Подключение к источнику данных произведено успешно.\nТеперь необходимо выбрать базу данных для резервного копирования!");
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

                if(server2020arr.Contains(textDataSource.Text))
                {
                    ProgressBarForm formprogress = new ProgressBarForm();
                    formprogress.Show();

                    this.Hide();

                    int value_to_form_progressbar = 0;
                    string nameBase;                    

                    //------ ALLORG
                    conn = new SqlConnection(connectionString);
                    conn.Open();
                    nameBase = "ALLORG";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + "-" + DateTime.Now.Ticks.ToString() + ".bak'";
                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //------ EX_BD_PRIM
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "EX_BD_PRIM";

                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" +  nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //------ Lab
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "Lab";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" +  nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //------ EX_BD
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "EX_BD";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" +  nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //----------- 1SBASE
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "1SBASE";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" +  nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();
                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //---------------EX_BD_USER
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "EX_BD_USER";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" +  nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //---------- FOND
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "FOND";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    MessageBox.Show("Резервное копирование всех баз SERVER2020 успешно произведено!");
                }
              

                if (winserverarr.Contains(textDataSource.Text))
                {
                    ProgressBarForm formprogress = new ProgressBarForm();
                    formprogress.Show();

                    this.Hide();

                    int value_to_form_progressbar = 0;
                    string nameBase;

                    //------ ALLORG
                    conn = new SqlConnection(connectionString);
                    conn.Open();
                    nameBase = "ALLORG";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + "-" + DateTime.Now.Ticks.ToString() + ".bak'";
                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //------ EX_BD_PRIM
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "EX_BD_PRIM";

                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //------ EX_BD_USER
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "EX_BD_USER";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //------ EX_BD
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "EX_BD";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //----------- 1SBASE
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "1SBASE";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();
                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //---------------SMBusiness
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "SMBusiness";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    //---------- FOND
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    nameBase = "FOND";
                    sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + "-"
                        + DateTime.Now.Ticks.ToString() + ".bak'";

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);

                    conn.Close();
                    conn.Dispose();

                    MessageBox.Show("Резервное копирование всех баз WINSERVER успешно произведено!");
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);                
            }
            
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.linkedin.com/in/vitaliy-petukhov-206a3a156/");
        }             
       
        
    }
}
