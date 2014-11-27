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
    public partial class Form1 : Form
    {
        Boolean Expanded = true;
        public Form1()
        {
            InitializeComponent();
        }

        private void WindowExpand(){
            if (Expanded)
            {
                groupBox1.Visible = groupBox2.Visible = false;
                treeView1.Width = 1225;
                Expanded = false;
                button3.Text = "<";
            }
            else
            {
                groupBox1.Visible = true;
                treeView1.Width = 820;
                Expanded = true;
                button3.Text = ">";
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
        }

        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {

        }

        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void treeView1_AfterSelect_2(object sender, TreeViewEventArgs e)
        {
            listView1.Clear();
            if (treeView1.SelectedNode.Index == 0)                                //Test für Serverinfo -> ListView
            {
                treeView1.SelectedNode.Text = ("Nickname: TestServer" + Environment.NewLine
                    + "IP: 127.0.0.1" + '\n'
                    + "Notiz: TestServer zu TestZwecken");
                listView1.Items.Add("Nickname: TestServer");
                listView1.Items.Add("IP: 127.0.0.1");
                listView1.Items.Add("Notiz: TestServer zu TestZwecken");

            }
            if (treeView1.SelectedNode.Index == 0 && treeView1.SelectedNode.Level==1)                                //Test für Serverinfo -> ListView
            {
                listView1.Clear();
                listView1.Items.Add("Nickname: TestFolder");
                listView1.Items.Add("Directory: root/linuxstuff/whatever");
                listView1.Items.Add("Notiz: TestServer zu TestZwecken");

            }
        }   

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
          
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WindowExpand();
        }

        private void editToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!Expanded)
            {
                WindowExpand();
            }
            groupBox1.Visible = true;
            groupBox2.Visible = false;
        }

        private void editToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!Expanded)
            {
                WindowExpand();
            }
            groupBox1.Visible = false;
            groupBox2.Visible = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            groupBox3.Enabled = !checkBox1.Checked;
        }

        private void mountToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 About = new Form2();
            About.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}