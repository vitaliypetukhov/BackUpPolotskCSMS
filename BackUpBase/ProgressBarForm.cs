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

namespace BackUpBase
{
    public partial class ProgressBarForm : Form
    {
        public ProgressBarForm()
        {
            InitializeComponent();
        }
         

        public void PeredachaLoadaPB(int value)
        {
            progressBar.Value = value;

            if (progressBar.Value == progressBar.Maximum)
            {
                this.Close();

                Form1 mainform = new Form1();
                mainform.Show();

                mainform.Refresh();
                Thread.Sleep(1000);
            }
        }

        public void PeredachaLabelPB (string labelvaluestring)
        {
            label1.Text = labelvaluestring;

            label1.Refresh();
        }

    }
}
