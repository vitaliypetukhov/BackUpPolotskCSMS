using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
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
            "SERVER2020", "server2020", 
            "Server2020", "SeRvEr2020", 
            "sERVER2020", "sErVeR2020",
            "SERver2020", "serVER2020",
            "SErver2020", "seRVER200",
            "SERVer2020", "servER2020",
            "SERVEr2020", "serveR2020",
            "SeRVER2020", "sErver2020",
            "SErVER2020", "seRver2020",
            "SERvER2020", "serVer2020",
            "SERVeR2020", "servEr2020"
        };

        public string[] winserverarr =
        {
            "WINSERVER", "Winserver",
            "wINSERVER", "winserver",
            "WiNsErVeR", "wInSeRvEr",
            "wInserver", "WiNSERVER",
            "wiNserver", "WInSERVER",
            "winServer", "WINsERVER",
            "winsErver", "WINSeRVER",
            "winseRver", "WINSErVER",
            "winserVer", "WINSERvER",
            "winservEr", "WINSERVeR",
            "winserveR", "WINSERVEr",
            "WINserver", "winSERVER"

        };

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
            checkBox1.Visible = false;

            typeBackpupCB.Enabled = false;
            BtnBrowse.Enabled = false;
            BtnDBFileBrowse.Enabled = false;
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            conn = new SqlConnection(connectionString);
            conn.Close();
            e.Cancel = true;
            this.Close();
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (textDataSource.Text.CompareTo("") == 0 || txtPassword.Text.CompareTo("") == 0 || txtPassword.Text.CompareTo("") == 0)
                {
                    MessageBox.Show("Ошибка подключения к базе данных!\nПроверьте правильность ввода данных.");
                    return;
                }                

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
                typeBackpupCB.Enabled = true;

                BtnDBFileBrowse.Enabled = true;
                BtnBrowse.Enabled = true;

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

            typeBackpupCB.Enabled = false;

            BtnBackup.Enabled = false;
            BtnRestore.Enabled = false;
            BtnConnect.Enabled = true;

            checkBox1.Enabled = false;

            BtnBrowse.Enabled = false;
            BtnDBFileBrowse.Enabled = false;
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            conn = new SqlConnection(connectionString);

            try
            {
                if (cmdDatabases.Text.CompareTo("") == 0)
                {
                    MessageBox.Show("База Данных для резервной копии не выбрана!");
                    return;
                }

                if (typeBackpupCB.Text.CompareTo("") == 0)
                {
                    MessageBox.Show("Тип копии не выбран!");
                    return;
                }
                
                conn.Open();

                if (typeBackpupCB.SelectedIndex == 0)
                {                  
                    sql = "BACKUP DATABASE [" + cmdDatabases.Text + "] TO DISK = '" + txtBackupLoc.Text + "\\" + cmdDatabases.Text + ".bak'";
                }
                else
                {                
                    sql = "BACKUP DATABASE [" + cmdDatabases.Text + "] TO DISK = '" + txtBackupLoc.Text + "\\" + cmdDatabases.Text + ".bak' " + "WITH DIFFERENTIAL";
                }
                command = new SqlCommand(sql, conn);
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Резервное копирование произведено успешно !");
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
            conn = new SqlConnection(connectionString);
            try
            {
                if (cmdDatabases.Text.CompareTo("") == 0)
                {
                    MessageBox.Show("Резервная копия базы данных не выбрана!");
                    return;
                }              
                                
                conn.Open();
                sql = "Alter Database " + cmdDatabases.Text + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                sql += "Restore Database " + cmdDatabases.Text + " FROM Disk = '" + txtRestoreFileLoc.Text + "' WITH REPLACE;";
                command = new SqlCommand(sql, conn);
                command.ExecuteNonQuery();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Восстановление базы данных из резервной копии произведено успешно !");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender; 
            if (checkBox.Checked == true)
            {
                BtnBackupAll.Enabled = true;
                BtnBackupAll.Visible = true;
                BtnBackup.Enabled = false;
                BtnBackup.Visible = false;
            }
            else
            {
                BtnBackupAll.Enabled = false;
                BtnBackupAll.Visible = false;
                BtnBackup.Enabled = true;
                BtnBackup.Visible = true;
            }
        }

        private void BtnBackupAll_Click(object sender, EventArgs e)
        {
            conn = new SqlConnection(connectionString);

            ProgressBarForm formprogress = new ProgressBarForm();

            int value_to_form_progressbar = 0;
            string value_label_progessbar = "Пожалуйста подождите, идет резервное копирование баз данных";
            string nameBase = "";

            try 
            {
                conn.Open();

                if (txtBackupLoc.Text.CompareTo("") == 0)
                {
                    MessageBox.Show("Выберите местоположение для резервной копии!");
                    return;
                }

                if(typeBackpupCB.Text.CompareTo("") == 0)
                {
                    MessageBox.Show("Тип копии не выбран!");
                    return;
                }              
                
                if (server2020arr.Contains(textDataSource.Text))
                {
                    formprogress.Show();
                    formprogress.Refresh();
                    Thread.Sleep(1000);
                    this.Hide();                  

                    //------ ALLORG   
                        nameBase = "ALLORG";

                        if (typeBackpupCB.SelectedIndex == 0)
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                        }
                        else
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                        }

                        command = new SqlCommand(sql, conn);
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();

                        value_to_form_progressbar += 15;
                        formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                        value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                        formprogress.PeredachaLabelPB(value_label_progessbar);                   
                        Thread.Sleep(5000);
                    
                    
                    //------ EX_BD_PRIM

                        nameBase = "EX_BD_PRIM";

                        if (typeBackpupCB.SelectedIndex == 0)
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                        }
                        else
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                        }

                        command = new SqlCommand(sql, conn);
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();

                        value_to_form_progressbar += 15;
                        formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                        value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                        formprogress.PeredachaLabelPB(value_label_progessbar);                   
                        Thread.Sleep(5000);                  


                    //------ Lab
                   
                        nameBase = "Lab";

                        if (typeBackpupCB.SelectedIndex == 0)
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                        }
                        else
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                        }

                        command = new SqlCommand(sql, conn);
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();

                        value_to_form_progressbar += 15;
                        formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                        value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                        formprogress.PeredachaLabelPB(value_label_progessbar);
                        Thread.Sleep(5000);
                   

                    //------ EX_BD
                   
                    nameBase = "EX_BD";

                    if (typeBackpupCB.SelectedIndex == 0)
                    {
                        sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                    }
                    else
                    {
                        sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                    }

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                    value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                    formprogress.PeredachaLabelPB(value_label_progessbar);
                    Thread.Sleep(5000);

                    //----------- 1SBASE

                    nameBase = "1SBASE";

                    if (typeBackpupCB.SelectedIndex == 0)
                    {
                        sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                    }
                    else
                    {
                        sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                    }

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                    value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                    formprogress.PeredachaLabelPB(value_label_progessbar);
                    Thread.Sleep(5000);

                    //---------------EX_BD_USER

                    nameBase = "EX_BD_USER";

                    if (typeBackpupCB.SelectedIndex == 0)
                    {
                        sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                    }
                    else
                    {
                        sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                    }

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                    value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                    formprogress.PeredachaLabelPB(value_label_progessbar);
                    Thread.Sleep(5000);

                    //---------- FOND

                    nameBase = "FOND";

                    if (typeBackpupCB.SelectedIndex == 0)
                    {
                        sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                    }
                    else
                    {
                        sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                    }

                    command = new SqlCommand(sql, conn);
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();

                    value_to_form_progressbar += 15;
                    formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                    value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                    formprogress.PeredachaLabelPB(value_label_progessbar);

                    MessageBox.Show("Резервное копирование всех баз SERVER2020 успешно произведено!");
                }
              
                if (winserverarr.Contains(textDataSource.Text))
                {
                    formprogress.Show();
                    formprogress.Refresh();
                    Thread.Sleep(1000);

                    this.Hide();

                    //------ ALLORG

                        nameBase = "ALLORG";

                        if (typeBackpupCB.SelectedIndex == 0)
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                        }
                        else
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                        }
                        command = new SqlCommand(sql, conn);
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();

                        value_to_form_progressbar += 15;
                        formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                        value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                        formprogress.PeredachaLabelPB(value_label_progessbar);
                        Thread.Sleep(5000);

                    //------ EX_BD_PRIM

                        nameBase = "EX_BD_PRIM";

                        if (typeBackpupCB.SelectedIndex == 0)
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                        }
                        else
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                        }

                        command = new SqlCommand(sql, conn);
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();

                        value_to_form_progressbar += 15;
                        formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                        value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                        formprogress.PeredachaLabelPB(value_label_progessbar);
                        Thread.Sleep(5000);

                    //------ EX_BD_USER

                        nameBase = "EX_BD_USER";

                        if (typeBackpupCB.SelectedIndex == 0)
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                        }
                        else
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                        }

                        command = new SqlCommand(sql, conn);
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();

                        value_to_form_progressbar += 15;
                        formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                        value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                        formprogress.PeredachaLabelPB(value_label_progessbar);
                        Thread.Sleep(5000);

                    //------ EX_BD

                        nameBase = "EX_BD";

                        if (typeBackpupCB.SelectedIndex == 0)
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                        }
                        else
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                        }

                        command = new SqlCommand(sql, conn);
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();

                        value_to_form_progressbar += 15;
                        formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                        value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                        formprogress.PeredachaLabelPB(value_label_progessbar);
                        Thread.Sleep(5000);

                    //----------- 1SBASE

                        nameBase = "1SBASE";

                        if (typeBackpupCB.SelectedIndex == 0)
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                        }
                        else
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                        }

                        command = new SqlCommand(sql, conn);
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();

                        value_to_form_progressbar += 15;
                        formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                        value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                        formprogress.PeredachaLabelPB(value_label_progessbar);
                        Thread.Sleep(5000);

                    //---------------SMBusiness

                        nameBase = "SMBusiness";

                        if (typeBackpupCB.SelectedIndex == 0)
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                        }
                        else
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                        }

                        command = new SqlCommand(sql, conn);
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();

                        value_to_form_progressbar += 15;
                        formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                        value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                        formprogress.PeredachaLabelPB(value_label_progessbar);
                        Thread.Sleep(5000);

                    //---------- FOND

                        nameBase = "FOND";

                        if (typeBackpupCB.SelectedIndex == 0)
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak'";
                        }
                        else
                        {
                            sql = "BACKUP DATABASE [" + nameBase + "]" + " TO DISK = '" + txtBackupLoc.Text + "\\" + nameBase + ".bak' " + "WITH DIFFERENTIAL";
                        }

                        command = new SqlCommand(sql, conn);
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();

                        value_to_form_progressbar += 15;
                        formprogress.PeredachaLoadaPB(value_to_form_progressbar);
                        value_label_progessbar = "Пожалуйста подождите, идет резервное копирование базы данных [" + nameBase + "]";
                        formprogress.PeredachaLabelPB(value_label_progessbar);

                    MessageBox.Show("Резервное копирование всех баз WINSERVER успешно произведено!");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);                
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.linkedin.com/in/vitaliy-petukhov-206a3a156/");
        }

    }
}
