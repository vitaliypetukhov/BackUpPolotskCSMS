using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackUpBase
{
    public partial class ProgressBarForm : Form
    {
        public ProgressBarForm()
        {
            InitializeComponent();
        }

        private void ProgressBarForm_Load(object sender, EventArgs e)
        {

        }

        public void PeredachaLoadaPB(int value)
        {
            progressBar.Value = value;

            if (progressBar.Value == progressBar.Maximum)
            {
                this.Close();

                Form1 mainform = new Form1();
                mainform.Show();
            }
        }
    }
}
