using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectCompiler
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            string username = UNBox.Text;
            string password = PWBox.Text;

            // Replace with your actual authentication logic
            if (username == "admin" && password == "0000")
            {
                // Authentication successful
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                // Authentication failed
                MessageBox.Show("Invalid username or password.");
            }
        }
    }
}
