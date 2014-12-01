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
    public partial class FiSSH : Form
    {
        Boolean Expanded = true;
        public FiSSH()
        {
            InitializeComponent();
        }

        private void ServerFolderEdit()
        {
            switch (treeView1.SelectedNode.Level)
            {
                case 0:
                    groupBox1.Enabled = true;
                    groupBox2.Visible = false;
                    break;
                case 1:
                    groupBox1.Enabled = false;
                    groupBox2.Visible = true;
                    break;
                default: break;
            }
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
                treeView1.Width = 814;
                Expanded = true;
                button3.Text = ">";
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listView1.Clear();
            if (treeView1.SelectedNode.Index == 0 && treeView1.SelectedNode.Level != 1)                                //Test für Serverinfo -> ListView
            {
                listView1.Items.Add("Nickname: TestServer");
                listView1.Items.Add("IP: 127.0.0.1");
                listView1.Items.Add("Notiz: TestServer zu TestZwecken");

            }
            if (treeView1.SelectedNode.Index == 0 && treeView1.SelectedNode.Level == 1)                                //Test für Serverinfo -> ListView
            {
                listView1.Clear();
                listView1.Items.Add("Nickname: TestFolder");
                listView1.Items.Add("Directory: root/linuxstuff/whatever");
                listView1.Items.Add("Notiz: TestServer zu TestZwecken");

            }
            ServerFolderEdit();
        }

    //------------------------------------------------------------------------------------------------------------------------    














    //------------------------------------------------------------------------------------------------------------------------
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


        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

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
            if (!Expanded) WindowExpand();
            ServerFolderEdit();
        }

        private void editToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!Expanded) WindowExpand();
            ServerFolderEdit();
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

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.
            Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            TreeNode targetNode = treeView1.GetNodeAt(targetPoint);

            // Retrieve the node that was dragged.
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // Confirm that the node at the drop location is not  
            // the dragged node or a descendant of the dragged node. 
            if (!draggedNode.Equals(targetNode) && !ContainsNode(draggedNode, targetNode))
            {
                // If it is a move operation, remove the node from its current  
                // location and add it to the node at the drop location. 
                if (e.Effect == DragDropEffects.Move)
                {
                    draggedNode.Remove();
                    if (targetNode != null) targetNode.Nodes.Add(draggedNode);
                    else treeView1.Nodes.Add(draggedNode);
                }

                // If it is a copy operation, clone the dragged node  
                // and add it to the node at the drop location. 
                else if (e.Effect == DragDropEffects.Copy)
                {
                    targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
                }

                // Expand the node at the location  
                // to show the dropped node.
                if (targetNode != null) targetNode.Expand();
                else { }
            }
        }

        private bool ContainsNode(TreeNode node1, TreeNode node2)
        {
            // Check the parent node of the second node. 
            if (node2 == null) return false;
            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            // If the parent node is not null or equal to the first node,  
            // call the ContainsNode method recursively using the parent of  
            // the second node. 
            return ContainsNode(node1, node2.Parent);
        } 

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void treeView1_DragLeave(object sender, EventArgs e)
        {

        }

        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.
            Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));

            // Select the node at the mouse position.
            treeView1.SelectedNode = treeView1.GetNodeAt(targetPoint);
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Move the dragged node when the left mouse button is used. 
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }

      
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Add feature: button2.Enabled = false; while no changes are made -> Later
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 Options = new Form3();
            Options.ShowDialog();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

    }
}