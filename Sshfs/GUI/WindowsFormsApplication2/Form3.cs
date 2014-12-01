using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            Loglevel.SelectedIndex = 0;// Debugging ist beim Start der Form ausgewählt
        }
       
        private void Loglevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void virtualdriveletter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Utilities.GetAvailableDrives() schaut nach den verfügbaren drive letters 
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
