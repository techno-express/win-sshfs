using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Sshfs.GuiBackend.Remoteable;
using System.Runtime.Remoting;
using System.ServiceModel;
using Sshfs.GuiBackend;


namespace GUI_WindowsForms
{
    public partial class FiSSHForm : Form
    {
        Boolean Expanded = true;
        Boolean gBox2Vis = false;
        int TimerCount = 0;
        Font font = new Font("Microsoft Sans Serif", (float)8, FontStyle.Regular);
        Font fontBold = new Font("Microsoft Sans Serif", (float)8, FontStyle.Bold);
        TreeNode contxMenuDragged = new TreeNode(null);
        int iLocation = 10;
        int contxMenuTargetIndex = 0;
        bool ServerOffline = false;
        bool ConnectionFailBoxFlag = false;     // This flag is true, when there is already a IPCConnectionfial Message

        
        //////////////////////////////////////////////
        // For connection with Backend

        // Server connection object
        List<ServerModel> datamodel;
        private System.Threading.Thread MountThread = null; // Thread for mounting
        private Queue<Tuple<Guid, Guid>> ToMount = new Queue<Tuple<Guid,Guid>>(); // Mailbox for the Thread
        private List<Tuple<Guid, Guid>> MountingIDs = new List<Tuple<Guid, Guid>>();



        public FiSSHForm()
        {
            InitializeComponent();
            CreateTreeView();
        }

        /// updated the TreeView and, draws the nodes and describes the nodes
        private void CreateTreeView(/* STUFF */)
        {
            GetDataFromServer();
            treeView1.Nodes.Clear();
            try
            {
                for (int i = 0; i < datamodel.Count; i++)
                {
                    ServerModel server = datamodel[i];
                    // Adding server node 
                    TreeNode ParentNode = MakeServerNode(server);
                    treeView1.Nodes.Add(ParentNode);

                    // Adding folder nodes
                    for (int j = 0; j < server.Folders.Count(); j++)
                    {
                        FolderModel folder = server.Folders[j];
                        TreeNode ChildNode = MakeFolderNode(folder);
                        ParentNode.Nodes.Add(ChildNode);

                    }

                    // Adding "new folder" node
                    CreateAddFolderNode(ParentNode);
                }
                // Adding "new server" node
                CreateAddServerNode();
            }
            catch
            {
                Application.Exit();
            }
            
           
       }


        /// updated the TreeView and, draws the nodes and describes the nodes
        private void UpdateTreeView(/* STUFF */)
        {
            GetDataFromServer();

            try
            {
                for (int i = 0; i < datamodel.Count(); i++)
                {
                    ServerModel server = datamodel[i];
                    TreeNode ParentNode = treeView1.Nodes[i];

                    if (ParentNode.Name == server.ID.ToString())
                    {
                        UpdateServerNode(server, ParentNode);

                        for (int j = 0; j < server.Folders.Count; j++)
                        {
                            FolderModel folder = server.Folders[j];
                            TreeNode ChildNode = ParentNode.Nodes[j];

                            if (ChildNode.Name == folder.ID.ToString())
                            {
                                UpdateFolderNode(folder, ChildNode);
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                    }
                    // if tree node does not fit to datamodel element
                    // add a new tree node and remove every other node with same id
                    else
                    {
                        throw new Exception();
                    }

                }
            }
            catch
            {
                CreateTreeView();
                return;
            }
       }

        private void UpdateMenuBar()
        {
             GetDataFromServer();

            if (treeView1.SelectedNode == null) { return; }

            switch (treeView1.SelectedNode.Level)
            {
                case 0:
                    #region server_node
                    mountToolStripMenuItem.Enabled = false;
                    unmountToolStripMenuItem.Enabled = false;
                    MountAnimatonStop();

                    break;
                    #endregion

                case 1:
                    #region folder_node
                    // get server which is presented by selected parent node
                    ServerModel server = GetSelectedServerNode();
                    // get folder which is presented by selected node
                    FolderModel folder = null;
                    try { folder = GetSelectedFolderNode(); }
                    catch { }

                    if (folder != null)
                    {
                        switch (folder.Status)
                        {
                            case Sshfs.DriveStatus.Mounted:
                                MountAnimatonStop();
                                mountToolStripMenuItem.Enabled = false;
                                unmountToolStripMenuItem.Enabled = true;
                                break;

                            case Sshfs.DriveStatus.Mounting:
                                MountAnimationStart();
                                mountToolStripMenuItem.Enabled = true;
                                unmountToolStripMenuItem.Enabled = false;
                                break;

                            default:
                                MountAnimatonStop();
                                mountToolStripMenuItem.Enabled = true;
                                unmountToolStripMenuItem.Enabled = false;
                                break;
                        }
                    }
                    else
                    {
                        mountToolStripMenuItem.Enabled = false;
                        unmountToolStripMenuItem.Enabled = false;
                    }

                    break;
                    #endregion

                default: break;
            }
        }

        private void GetDataFromServer()
        {
            IServiceFisshBone bone_server = IPCConnection.ClientConnect();
            List<ServerModel> tmp = new List<ServerModel>();

            if (datamodel != null)
                tmp = new List<ServerModel>(datamodel);

            try { datamodel = bone_server.listAll(); }
            catch
            {
                if (!ConnectionFailBoxFlag)
                {
                    ConnectionFailBoxFlag = true;
                    MessageBox.Show("Cannot connect with IPC-Server. Application will now close.");
                    Application.Exit();
                    
                    // ConnectionFailBoxFlag = false;
                }
                return;
            }
   }

        
        /// Updates menu strip and edit area
        private void ServerFolderEdit()
        {
            GetDataFromServer(); 

            if (treeView1.SelectedNode == null) { return; }

            switch (treeView1.SelectedNode.Level)
            {
                case 0:
                    if (Expanded)
                    {
                        groupBox1.Enabled = true;
                        groupBox2.Visible = false;
                        
                        // get server which is presented by selected node 
                        ServerModel server = GetSelectedServerNode();

                        if (server != null)
                        {
                            // write data in edit box
                            textBox_server_name.Text = server.Name;
                            textBox_server_ip.Text = server.Host;
                            numericUpDown_server_port.Value = server.Port;
                            richTextBox_server_notes.Text = server.Notes;
                            textbox_server_username.Text = server.Username;
                            textBox_server_privatkey.Text = server.PrivateKey;
                            textBox_server_password.Text = server.Password;


                            switch (server.Type)
                            {
                                case Sshfs.ConnectionType.Password:
                                    radioButton_server_password.Checked = true;
                                    break;

                                case Sshfs.ConnectionType.PrivateKey:
                                    radioButton_server_privatekey.Checked = true;
                                    break;

                                case Sshfs.ConnectionType.Pageant:
                                    radioButton_server_pageant.Checked = true;
                                    break;
                            }
                            
                            groupBox1.Enabled = true;
                            textBox_server_name.Enabled = true;
                            textBox_server_ip.Enabled = true;
                            numericUpDown_server_port.Enabled = true;
                            groupBox2.Enabled = false;
                            groupBox3.Enabled = false;

                            button_server_savechanges.Enabled = true;
                            button_folder_savechanges.Enabled = false;
                        }
                        else
                        {
                            // write data in edit box
                            textBox_server_name.Text = null;
                            textBox_server_ip.Text = null;
                            numericUpDown_server_port.Value = 22;
                            richTextBox_server_notes.Text = null;
                            textbox_server_username.Text = null;
                            textBox_server_privatkey.Text = null;
                            textBox_server_password.Text = null;
                            
                            groupBox1.Enabled = false;
                            groupBox2.Enabled = false;
                            groupBox3.Enabled = false;

                            button_server_savechanges.Enabled = false;
                            button_folder_savechanges.Enabled = false;
                        }
                    }
                    else gBox2Vis = false;
                    break;
                case 1:
                    if (Expanded)
                    {
                        groupBox2.Visible = true;

                        // get server which is presented by selected parent node
                        ServerModel server = GetSelectedServerNode();
                        // get folder which is presented by selected node
                        FolderModel folder = null;
                        try { folder = GetSelectedFolderNode(); }
                        catch { }

                        if (folder != null)
                        {
                            // write data in edit box
                            textBox_folder_entry.Text = folder.Name;
                            textBox_folder_password.Text = folder.Password;
                            textBox_folder_privat_key.Text = folder.PrivateKey;
                            textBox_folder_username.Text = folder.Username;
                            textBox9_folder_remotedirectory.Text = folder.Folder;
                            checkBox_folder_usedefaultaccound.Checked = folder.use_global_login;
                            writeAvailableDrivesInCombo();
                            comboBox_folder_driveletter.SelectedIndex = comboBox_folder_driveletter.Items.IndexOf(folder.Letter + ":");
                            radioButton_folder_virtualdrive.Checked = folder.use_virtual_drive;
                            radioButton_folder_usedrive.Checked = ! folder.use_virtual_drive;
                            textBox_folder_virtual_drive.Text = folder.VirtualDriveFolder;
                            checkBox_automount.Checked = folder.Automount;

                            //server properties 
                            textBox_server_name.Text = server.Name;
                            textBox_server_ip.Text = server.Host;
                            numericUpDown_server_port.Value = server.Port;

                            switch (folder.Type)
                            {
                                case Sshfs.ConnectionType.Password:
                                    radioButton_folder_password.Checked = true;
                                    break;

                                case Sshfs.ConnectionType.PrivateKey:
                                    radioButton_folder_privatekey.Checked = true;
                                    break;

                                case Sshfs.ConnectionType.Pageant:
                                    radioButton_folder_pageant.Checked = true;
                                    break;
                            }



                            groupBox1.Enabled = true;
                            textBox_server_name.Enabled = false;
                            textBox_server_ip.Enabled = false;
                            numericUpDown_server_port.Enabled = false;
                            groupBox2.Enabled = true;
                            groupBox3.Enabled = !checkBox_folder_usedefaultaccound.Checked;

                            button_server_savechanges.Enabled = false;
                            button_folder_savechanges.Enabled = true;
                        }
                        else
                        {
                            textBox_folder_entry.Text = null;
                            textBox_folder_password.Text = null;
                            textBox_folder_privat_key.Text = null;
                            textBox_folder_username.Text = null;
                            textBox9_folder_remotedirectory.Text = null;

                            groupBox1.Enabled = false;
                            groupBox2.Enabled = false;
                            groupBox3.Enabled = false;

                            button_server_savechanges.Enabled = false;
                            button_folder_savechanges.Enabled = false;
                        }
                        
                    }
                    else gBox2Vis = true;
                    break;
                default: break;
            }
        }

        /// scale up or scale down the treeView size 
        private void WindowExpand(){
            if (Expanded)
            {
                if (groupBox2.Visible)
                {
                    groupBox1.Visible = groupBox2.Visible = false;
                    gBox2Vis = true;
                }
                else groupBox1.Visible = false;
                treeView1.Width = groupBox1.Location.X + groupBox1.Size.Width - 15;
                Expanded = false;
                button_windowexpand.Text = "<";
            }
            else
            {
                if (gBox2Vis) groupBox1.Visible = groupBox2.Visible = true;
                else groupBox1.Visible = true;
                treeView1.Width = groupBox1.Location.X - 25;
                Expanded = true;
                button_windowexpand.Text = ">";
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            editToolStripMenuItem.Enabled = true;
            UpdateMenuBar();
            ServerFolderEdit();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
        }

        /// the treeView will be expanded by clicking the windowexpand-button 
        private void button3_Click(object sender, EventArgs e)
        {
            WindowExpand();
        }


        private void editToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!Expanded) WindowExpand();
            ServerFolderEdit();
            textBox_server_name.Focus();
        }

        private void editToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!Expanded) WindowExpand();
            ServerFolderEdit(); 
            textBox_folder_entry.Focus();
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)  //Use Default Account Checkbox
        {
            groupBox3.Enabled = !checkBox_folder_usedefaultaccound.Checked;
        }

        private void mountToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)   //Opens Options + About Dialog
        {
            AboutForm About = new AboutForm();
            About.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateTreeView();
            // Balloon tip for the systemtray
            FiSSH.BalloonTipText = "Application Minimized.";
            FiSSH.BalloonTipTitle = "FiSSH";
        }


        /// allows to use drag&drop in the treeView
        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            IServiceFisshBone bone_server = IPCConnection.ClientConnect();
            // Retrieve the client coordinates of the drop location.
            Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            TreeNode targetNode = treeView1.GetNodeAt(targetPoint);

            // Retrieve the node that was dragged.
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // Confirm that the node at the drop location is not  
            // the dragged node or a descendant of the dragged node. 
            if (!draggedNode.Equals(targetNode) && 
                !ContainsNode(draggedNode, targetNode) && 
                !ServerOrFolderAddNode(draggedNode) &&
                !ServerOrFolderAddNode(targetNode))
            {
                // If it is a move operation, remove the node from its current  
                // location and add it to the node at the drop location. 
                if (e.Effect == DragDropEffects.Move)
                {

                        if (draggedNode.Level == 0)
                        {
                            if (targetNode != null && targetNode.Level == 0)
                            {
                                ServerModel draggedServer = datamodel.Find(x => x.ID.ToString() == draggedNode.Name);
                                ServerModel targedServer = datamodel.Find(x => x.ID.ToString() == targetNode.Name);
                                
                                draggedNode.Remove();
                                treeView1.Nodes.Insert(targetNode.Index + 1, draggedNode);

                                bone_server.MoveServerAfter(draggedServer.ID, targedServer.ID);
                            }
                        }
                        if (draggedNode.Level == 1)
                        {
                            if (targetNode != null && targetNode.Level == 1)
                            {
                                ServerModel draggedServer = datamodel.Find(x => x.ID.ToString() == draggedNode.Parent.Name);
                                ServerModel targedServer = datamodel.Find(x => x.ID.ToString() == targetNode.Parent.Name);
                                FolderModel draggedFolder = draggedServer.Folders.Find(x => x.ID.ToString() == draggedNode.Name);
                                FolderModel targedFolder = targedServer.Folders.Find(x => x.ID.ToString() == targetNode.Name);
                                
                                draggedNode.Remove();
                                targetNode.Parent.Nodes.Insert(targetNode.Index + 1, draggedNode);

                                bone_server.MoveFolderAfter(
                                    draggedServer.ID, targedServer.ID,
                                    draggedFolder.ID, targedFolder.ID);
                            }

                            if (targetNode != null && targetNode.Level == 0)
                            {
                                ServerModel draggedServer = datamodel.Find(x => x.ID.ToString() == draggedNode.Parent.Name);
                                ServerModel targetServer = datamodel.Find(x => x.ID.ToString() == targetNode.Name);
                                FolderModel draggedFolder = draggedServer.Folders.Find(x => x.ID.ToString() == draggedNode.Name);
                                
                                Guid targetFolderID = Guid.Empty;
                                if (targetServer.Folders.Count > 0)
                                {
                                    targetFolderID = targetServer.Folders.Last().ID;
                                }

                                draggedNode.Remove();
                                targetNode.Nodes.Insert(targetNode.Nodes.Count - 1, draggedNode); //inserted at end before "add folder" node

                                bone_server.MoveFolderAfter(
                                    draggedServer.ID, targetServer.ID,
                                    draggedFolder.ID, targetFolderID);
                            }
                        }
                    /*
                   if (targetNode != null && (draggedNode.Level > targetNode.Level) && !ServerOrFolderAddNode(targetNode)) 
                   { 
                      draggedNode.Remove(); targetNode.Nodes.Insert(targetNode.Nodes.Count - 1, draggedNode); // Send operation to backend here -> update TreeView
                    //  UpdateTreeView();
                   }
                   if (targetNode != null && (draggedNode.Level == targetNode.Level) && !ServerOrFolderAddNode(targetNode))
                   {
                       contxMenuDragged = draggedNode;
                       contxMenuTargetIndex = targetNode.Index;
                       TreeNode contxMenuDraggedParent = contxMenuDragged.Parent;
                       if (contxMenuDraggedParent != null)
                       {
                           contxMenuDragged.Remove();
                           contxMenuDraggedParent.Nodes.Insert(contxMenuTargetIndex, contxMenuDragged);
                       }
                       else
                       {
                           contxMenuDragged.Remove();
                           treeView1.Nodes.Insert(contxMenuTargetIndex, contxMenuDragged);
                       }

                       //  UpdateTreeView();
                   }

                   else 
                   {
                       if (targetNode == null && draggedNode.Level != 1) 
                       {
                          draggedNode.Remove(); treeView1.Nodes.Insert(treeView1.Nodes.Count - 1, draggedNode); //Same here
                       //   UpdateTreeView();
                       }
                   }*/
 
                   
                }

                // If it is a copy operation, clone the dragged node  
                // and add it to the node at the drop location. 
                else if (e.Effect == DragDropEffects.Copy)
                {
                 //   targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
                }

                // Expand the node at the location  
                // to show the dropped node.
                if (targetNode != null) targetNode.Expand();
                else { }
            }
        }

        private bool ServerOrFolderAddNode(TreeNode node1)
        {
            if (node1.Parent != null && node1.Parent.Nodes[node1.Parent.Nodes.Count - 1].Equals(node1)) return true;
            if (treeView1.Nodes[treeView1.Nodes.Count - 1].Equals(node1)) return true;
            else return false;
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
            
        }

        /// opens the option-Form by clicking the option-button
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm Options = new OptionsForm();
            Options.ShowDialog();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (!ServerOrFolderAddNode(e.Node))
            {
                float iHeight = e.Graphics.MeasureString("Name: ", fontBold).Height;
                e.Graphics.DrawString("Name: "+e.Node.Tag, fontBold, Brushes.Black, e.Bounds.X + 2, e.Bounds.Y + 5);
                e.Graphics.DrawString(e.Node.Text, font, Brushes.Black, e.Bounds.X +2, e.Bounds.Y + iHeight+5);
            }
            else
                e.Graphics.DrawString(e.Node.Text, fontBold, Brushes.Green, e.Bounds.X + 2, e.Bounds.Y + 5);
        }

        private void radioButton_folder_password_CheckedChanged(object sender, EventArgs e)
        {
            textBox_folder_privat_key.Enabled = button4.Enabled = false;
            textBox_folder_password.Enabled = true;
        }

        private void radioButton_folder_privatkey_CheckedChanged(object sender, EventArgs e)
        {
            textBox_folder_privat_key.Enabled = button4.Enabled = true;
            textBox_folder_password.Enabled = false;
        }

        private void radioButton_folder_pageant_CheckedChanged(object sender, EventArgs e)
        {
            textBox_folder_privat_key.Enabled = button4.Enabled = textBox_folder_password.Enabled = false;
        }

        private void radioButton_server_password_CheckedChanged(object sender, EventArgs e)
        {
            textBox_server_privatkey.Enabled = button1.Enabled = false;
            textBox_server_password.Enabled = true;
        }

        private void radioButton_folder_privatekey_CheckedChanged(object sender, EventArgs e)
        {
            textBox_server_privatkey.Enabled = button1.Enabled = true;
            textBox_server_password.Enabled = false;
        }

        private void radioButton_server_pageant_CheckedChanged(object sender, EventArgs e)
        {
            textBox_server_privatkey.Enabled = button1.Enabled = textBox_server_password.Enabled = false;
        }

        private void radioButton_folder_usedrive_CheckedChanged(object sender, EventArgs e)
        {
            comboBox_folder_driveletter.Enabled = true;
        }

        private void radioButton_folder_virtualdrive_CheckedChanged(object sender, EventArgs e)
        {
            comboBox_folder_driveletter.Enabled = false;
        }

        private void button_server_savechanges_Click(object sender, EventArgs e)
        {
            IServiceFisshBone bone_server = IPCConnection.ClientConnect();
            ServerModel server = GetSelectedServerNode();


            server.Name = textBox_server_name.Text;
            server.Notes = richTextBox_server_notes.Text;
            server.Username = textbox_server_username.Text;
            server.Password = textBox_server_password.Text;
            server.PrivateKey = textBox_server_privatkey.Text;
            server.Host = textBox_server_ip.Text;
            server.Port = (int) numericUpDown_server_port.Value;

            if (radioButton_server_pageant.Checked)
                server.Type = Sshfs.ConnectionType.Pageant;
            else if (radioButton_server_privatekey.Checked)
                server.Type = Sshfs.ConnectionType.PrivateKey;
            else
                server.Type = Sshfs.ConnectionType.Password;

            bone_server.editServer(server);
        }

        private void button_folder_savechanges_Click(object sender, EventArgs e)
        {
            IServiceFisshBone bone_server = IPCConnection.ClientConnect();
            // ausgewählten Ordners in die Variable "folder" schreiben und der zugehörige Server in "server"
            FolderModel folder = GetSelectedFolderNode();
            ServerModel server = GetSelectedServerNode();

            if (folder.Status == Sshfs.DriveStatus.Mounting || folder.Status == Sshfs.DriveStatus.Mounted)
            {
                MessageBox.Show("Error: Folder can only be edited in unmounted state.");
            }
            else
            {
                // Ordnereigenschaften ersetzen mit dem was in den Textboxen steht
                folder.Name = textBox_folder_entry.Text;
                folder.Note = richTextBox_folder_notes.Text;
                folder.Password = textBox_folder_password.Text;
                folder.PrivateKey = textBox_folder_privat_key.Text;
                folder.use_global_login = checkBox_folder_usedefaultaccound.Checked;
                folder.use_virtual_drive = radioButton_folder_virtualdrive.Checked;
                folder.Username = textBox_folder_username.Text;
                folder.VirtualDriveFolder = textBox_folder_virtual_drive.Text;
                folder.Folder = textBox9_folder_remotedirectory.Text;
                folder.Automount = checkBox_automount.Checked;

                try
                {
                    folder.Letter = comboBox_folder_driveletter.SelectedItem.ToString().ToCharArray()[0];
                }
                catch (NullReferenceException)
                {
                    folder.Letter = ' ';
                }


                if (radioButton_folder_privatekey.Checked == true)
                {
                    folder.Type = Sshfs.ConnectionType.PrivateKey;
                }
                else if (radioButton_folder_pageant.Checked == true)
                {
                    folder.Type = Sshfs.ConnectionType.Pageant;
                }
                else
                {
                    folder.Type = Sshfs.ConnectionType.Password;
                }

                bone_server.editFolder(server.ID, folder);
            }
        }

        /// focused the first text box on the right side by clicking the edit-button
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Expanded) WindowExpand();
            ServerFolderEdit();
            if (groupBox2.Visible) textBox_folder_entry.Focus();
            else textBox_server_name.Focus();
        }

        /// one frame in the loading animation when mount
        private void timer_animation_Tick(object sender, EventArgs e)
        {
            
            string[] Mounting = new string[4] {"Mounting","Mounting.","Mounting..","Mounting..."};
            mountToolStripMenuItem.Text = Mounting[TimerCount];
            mountToolStripMenuItem.Image = imageList2.Images[TimerCount];
            TimerCount++;
            if (TimerCount == 4)
            {
                TimerCount = 0;
            }
        }

        /// Updates frequently all icons
        private void time_viewupdate_Tick(object sender, EventArgs e)
        {
            UpdateMenuBar();
            UpdateTreeView();
        }



        private void mountToolStripMenuItem_Click_help()
        {
            IServiceFisshBone bone_server = IPCConnection.ClientConnect();
            Tuple<Guid, Guid> IDs = ToMount.Dequeue();
            MountingIDs.Add(IDs);
            //MountingFlagPipe.Enqueue(true);
            try
            {
                bone_server.Mount(IDs.Item1, IDs.Item2);
                ServerFolderEdit();
            }
            catch (FaultException<Fault> thrown_error)
            {
                //:::FIXME:::
                MessageBox.Show(thrown_error.Detail.Message);
            }
            catch (Exception thrown_error)
            {
                //:::FIXME:::
            }
            //MountingFlagPipe.Dequeue();
            MountingIDs.Remove(IDs);
        }

        /// the Folder will be mounted by clicking on the mount button
        /**
         * 
         */
       private void mountToolStripMenuItem_Click(object sender, EventArgs e)
        {// Only folders can be mounted

            ServerModel server = GetSelectedServerNode();
            FolderModel folder = GetSelectedFolderNode();

            if (server == null || folder == null)
            {
                //:::FIXME:::
                return;
            }

          
            if (0 < MountingIDs.IndexOf(new Tuple<Guid, Guid>(server.ID, folder.ID))
                   || ToMount.Contains(new Tuple<Guid,Guid>(server.ID, folder.ID) ))
            {
                return;
            }

            ToMount.Enqueue(new Tuple<Guid, Guid>(server.ID,folder.ID));
            folder.Status = Sshfs.DriveStatus.Mounting;
         //   MountAnimationStart();

           this.MountThread =
                new System.Threading.Thread(new System.Threading.ThreadStart(this.mountToolStripMenuItem_Click_help));
           MountThread.Start();         
    
         }

        private void unmountToolStripMenuItem_Click(object sender, EventArgs e)
        {// Only folders can be unmounted

            IServiceFisshBone bone_server = IPCConnection.ClientConnect();
            ServerModel server = GetSelectedServerNode();
            FolderModel folder = GetSelectedFolderNode();

            if (server == null || folder == null)
            {
                //:::FIXME:::
                return;
            }

            try
            {
                bone_server.UMount(server.ID, folder.ID);
                ServerFolderEdit();
            }
            catch (FaultException<Fault> thrown_error)
            {
                //:::FIXME:::
                MessageBox.Show(thrown_error.Detail.Message);
            }
            catch (Exception thrown_error)
            {
                //:::FIXME:::
            }

            // :::FIXME::: die animation muss parallel laufen 
            //loads the animation for mounting
            /*
            if (treeView1.SelectedNode.Index == 0 && treeView1.SelectedNode.Level != 0 && treeView1.SelectedNode.Level != 2 && timer1.Enabled == false)
            {
                deleteToolStripMenuItem.Enabled = false;
                editToolStripMenuItem.Enabled = false;
                optionsToolStripMenuItem.Enabled = false;
                timer1.Enabled =true;
            }
         //stops the animation and loads the mount image 
            else { timer1.Enabled = false;
                   deleteToolStripMenuItem.Enabled = true;
                   optionsToolStripMenuItem.Enabled = true;
                   mountToolStripMenuItem.Image = imageList1.Images[0];
                   mountToolStripMenuItem.Text = "Mount";

                 }*/
        }


        private void FiSSHForm_Resize(object sender, EventArgs e)
        {// Determine if the cursor is in the window
            bool cursorNotInBar = Screen.GetWorkingArea(this).Contains(Cursor.Position);
            if (this.WindowState == FormWindowState.Minimized && cursorNotInBar)
            {
               this.ShowInTaskbar = false;
               FiSSH.ShowBalloonTip(1000);
            }
        }

        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void closeApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void restoreToolStripMenuItem_Click(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void FiSSHForm_Closing(object sender, FormClosingEventArgs e)
        { // The user has requested the form be closed so mimimize to the system tray instead
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }


        #region TOOLS
        private TreeNode MakeServerNode (/*int index,*/ ServerModel server)
        {
            //TreeNode ParentNode = treeView1.Nodes.Insert(index, String.Format(
            TreeNode ParentNode = new TreeNode();
            UpdateServerNode(server, ParentNode);
            ParentNode.SelectedImageIndex = 6;
            ParentNode.ImageIndex = 6;
            ParentNode.ContextMenuStrip = this.contextMenuStrip1;
            ParentNode.Name = server.ID.ToString();

            return ParentNode;
        }

        private TreeNode MakeFolderNode (/*int index, TreeNode ParentNode,*/ FolderModel folder)
        {
            //TreeNode ChildNode = ParentNode.Nodes.Insert(index, String.Format(
            TreeNode ChildNode = new TreeNode();
            UpdateFolderNode(folder, ChildNode);
            ChildNode.SelectedImageIndex = 4;
            ChildNode.ImageIndex = 4;
            ChildNode.ContextMenuStrip = this.contextMenuStrip2;
            ChildNode.Name = folder.ID.ToString();
            return ChildNode;
        }

        private void CreateAddServerNode()
        {
            TreeNode Node2 = treeView1.Nodes.Add(String.Format(
                                        "\n" +
                                        "Add new Server" + Environment.NewLine));
            Node2.SelectedImageIndex = 5;
            Node2.ImageIndex = 5;
            Node2.Name = Guid.Empty.ToString();
        }

        private void CreateAddFolderNode(TreeNode ParentNode)
        {
            TreeNode Node = ParentNode.Nodes.Add(String.Format(
                                      "\n" +
                                      "Add new Folder" + Environment.NewLine));
            Node.SelectedImageIndex = 3;
            Node.ImageIndex = 3;
            Node.Name = Guid.Empty.ToString();
        }

        private void UpdateServerNode(ServerModel server, TreeNode node)
        {

            string text = String.Format(
                                  //  "Name: " + server.Name + Environment.NewLine +
                                    "IP: " + server.Host + Environment.NewLine);
            if (server.Notes != "" && server.Notes != null)
            {
                text += "Notes: " + server.Notes;
            }


            if (text != node.Text || (string) node.Tag != server.Name)
            {
                node.Tag = server.Name;
                node.Text = text;
            }
        }

        private void UpdateFolderNode(FolderModel folder, TreeNode node)
        {
            string text = String.Format(String.Format(
                                  //  "Name: " + folder.Name + Environment.NewLine +
                                    "Path: " + folder.Folder + Environment.NewLine));
            if (folder.Note != "" && folder.Note != null)
            {
                text += "Notes: " + folder.Note;
            }

            if (text != node.Text || (string)node.Tag != folder.Name)
            {
                node.Tag = folder.Name;
                node.Text = text;
            }
        }

        private ServerModel GetSelectedServerNode()
        {
            switch (treeView1.SelectedNode.Level)
            {
                case 0: //return datamodel.ElementAt(treeView1.SelectedNode.Index);
                    return datamodel.Find(x => treeView1.SelectedNode.Name == x.ID.ToString());

                case 1: //return datamodel.ElementAt(treeView1.SelectedNode.Parent.Index);
                    return datamodel.Find(x => treeView1.SelectedNode.Parent.Name == x.ID.ToString());

                default: return null;
            }
        }


        private FolderModel GetSelectedFolderNode()
        {
            switch (treeView1.SelectedNode.Level)
            {
                case 0: return null;

                case 1: 
                    ServerModel server = datamodel.Find(x => treeView1.SelectedNode.Parent.Name == x.ID.ToString());
                    return server.Folders.Find(x => treeView1.SelectedNode.Name == x.ID.ToString());

                default: return null;
            }
        }

        private void MountAnimationStart()
        {
            timer_animation.Enabled = true;
            deleteToolStripMenuItem.Enabled = false;
        }

        private void MountAnimatonStop()
        {
            timer_animation.Enabled = false;
            mountToolStripMenuItem.Image = imageList1.Images[0];
            mountToolStripMenuItem.Text = "Mount";
            deleteToolStripMenuItem.Enabled = true;
        }

        private void addServer()
        {
            IServiceFisshBone bone_server = IPCConnection.ClientConnect();
            ServerModel server = new ServerModel();
            bone_server.addServer(server);
            server.Name = "new Server";
        }

        private void addFolder()
        {
            IServiceFisshBone bone_server = IPCConnection.ClientConnect();
            ServerModel server = GetSelectedServerNode();
            FolderModel folder = new FolderModel();
            folder.Name = "New Folder";
            TreeNode newNode = MakeFolderNode(folder);
            newNode.Name = bone_server.addFolder(server.ID, folder).ToString();

            if (treeView1.SelectedNode.Level == 1)
            {
                treeView1.SelectedNode.Parent.Nodes.Insert(treeView1.SelectedNode.Parent.Nodes.Count - 1, newNode);
            }
            else if (treeView1.SelectedNode.Level == 0)
            {
                treeView1.SelectedNode.Nodes.Insert(treeView1.SelectedNode.Nodes.Count - 1, newNode);

            }
        }

        private static bool IsDriveAvailable(char letter)
        {
            List<char> not_available = new List<char>();
            not_available.Add('a');
            not_available.Add('A');
            not_available.Add('b');
            not_available.Add('B');

            if (not_available.Contains(letter) ||
                Directory.GetLogicalDrives().Contains(((letter).ToString() + @":\")))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void writeAvailableDrivesInCombo()
        {
            this.comboBox_folder_driveletter.Items.Clear();
            for (int i = 'Z'; i >= 'A'; i--)
            {
                if (IsDriveAvailable((char)i) == true) this.comboBox_folder_driveletter.Items.Add((char)i + ":");
            }
        }
        #endregion

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IServiceFisshBone bone_server = IPCConnection.ClientConnect();
            ServerModel server = GetSelectedServerNode();
            FolderModel folder = GetSelectedFolderNode();

            if (treeView1.SelectedNode.Level == 1 && folder != null)
            {
                bone_server.removeFolder(server.ID, folder.ID);
                treeView1.SelectedNode.Remove();
            }
            else if(treeView1.SelectedNode.Level == 0 && server != null)
            {
                bone_server.removeServer(server.ID);
                treeView1.SelectedNode.Remove();
            }
        }

        private void duplicateToolStripMenuItem_all_Click(object sender, EventArgs e)
        {
            IServiceFisshBone bone_server = IPCConnection.ClientConnect();
            ServerModel server = GetSelectedServerNode();
            FolderModel folder = GetSelectedFolderNode();
            TreeNode node = new TreeNode();

            if (treeView1.SelectedNode.Level == 1)
            {
                node = MakeFolderNode(new FolderModel());
                node.Name = bone_server.duplicateFolder(server.ID, folder.ID).ToString();
                treeView1.SelectedNode.Parent.Nodes.Insert(
                    treeView1.SelectedNode.Index + 1,
                    node);
            }
            else if (treeView1.SelectedNode.Level == 0)
            {
                node = MakeServerNode(new ServerModel());
                node.Name = bone_server.duplicateServer(server.ID).ToString();
                treeView1.Nodes.Insert(
                    treeView1.SelectedNode.Index + 1,
                    node);
            }

        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            if (ServerOrFolderAddNode(treeView1.SelectedNode))
            {
                if (treeView1.SelectedNode.Level == 1)
                {
                    addFolder();
                }
                else if (treeView1.SelectedNode.Level == 0)
                {
                    IServiceFisshBone bone_server = IPCConnection.ClientConnect();
                    ServerModel server = new ServerModel();
                    server.Name = "New Server";
                    TreeNode newNode = MakeServerNode(server);
                    CreateAddFolderNode(newNode);
                    newNode.Name = bone_server.addServer(server).ToString();
                    treeView1.Nodes.Insert(treeView1.Nodes.Count - 1, newNode);
                }
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right) treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);
        }

        private void mountAllFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i = 0;
            TreeNode CountNode = treeView1.SelectedNode;
            while (CountNode.Nodes.Count - 1 != i)
            {
                treeView1.SelectedNode = CountNode.Nodes[i];

                ServerModel server = GetSelectedServerNode();
                FolderModel folder = GetSelectedFolderNode();
                if (folder.Status == Sshfs.DriveStatus.Mounted || folder.Status == Sshfs.DriveStatus.Mounting){
                    
                }
                else
                {
                    if (server == null || folder == null)
                    {
                        //:::FIXME:::
                        return;
                    }


                    if (0 < MountingIDs.IndexOf(new Tuple<Guid, Guid>(server.ID, folder.ID))
                           || ToMount.Contains(new Tuple<Guid, Guid>(server.ID, folder.ID)))
                    {
                        return;
                    }

                    ToMount.Enqueue(new Tuple<Guid, Guid>(server.ID, folder.ID));
                    folder.Status = Sshfs.DriveStatus.Mounting;
                    //   MountAnimationStart();

                    this.MountThread =
                         new System.Threading.Thread(new System.Threading.ThreadStart(this.mountToolStripMenuItem_Click_help));
                    MountThread.Start();
                }                        


                i++;
            }
        }

        private void addNewFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addFolder();
        }
    }
}