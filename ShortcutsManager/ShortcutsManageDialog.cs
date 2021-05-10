using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Ambiesoft.ShortcutsManager
{

    public partial class ShortcutsManageDialog : Form
    {


        private string GetDisplayMenuString(string menustring)
        {
            if (string.IsNullOrEmpty(menustring))
                return string.Empty;

            if (menustring.IndexOf('&') == -1)
                return menustring;

            if (menustring.IndexOf("&&") == -1)
                return menustring.Replace("&","");

            string[] stringSeparators = new string[] {"&&"};
            string[] part = menustring.Split(stringSeparators, StringSplitOptions.None);
            for(int i=0 ; i < part.Length ; ++i)
            {
                part[i] = part[i].Replace("&", "");
            }
            string ret = string.Join("&", part);
            return ret;
        }

        private ListViewItem CreateListViewItem(ToolStripMenuItem mitem, TreeNode node)
        {
            ListViewItem lvitem = new ListViewItem();
            lvitem.Text = GetDisplayShortcutString(mitem.ShortcutKeys);
            if (-1 != _icons.Images.IndexOfKey(mitem.Name))
                lvitem.ImageIndex = _icons.Images.IndexOfKey(mitem.Name);
            else
                lvitem.ImageIndex = 0;
            lvitem.Tag = node;
            lvitem.SubItems.Add(GetDisplayMenuString(mitem.Text));
            return lvitem;
        }

        private Dictionary<ToolStripMenuItem, Keys> _origs = new Dictionary<ToolStripMenuItem, Keys>();
        private void KeepOriginalShortcut(ToolStripMenuItem mitem)
        {
            _origs.Add(mitem, mitem.ShortcutKeys);
        }

        private void AddTreeItem(TreeNode parent, ToolStripItem item)
        {
            if (item is ToolStripSeparator)
                return;

            TreeNode node = new TreeNode(GetDisplayMenuString(item.Text));
            parent.Nodes.Add(node);
            ToolStripMenuItem mitem = (ToolStripMenuItem)item;
            KeepOriginalShortcut(mitem);
            if (mitem.Image != null)
            {
                _icons.Images.Add(mitem.Name, mitem.Image);
                //node.SelectedImageIndex = node.ImageIndex = _icons.Images.IndexOfKey(mitem.Name);

            }
            else
            {
              //  node.SelectedImageIndex = node.ImageIndex = 0;
            }

            NodeTag nodetag = new NodeTag(mitem, null);
            if (mitem.ShortcutKeys != Keys.None)
            {
                ListViewItem lvitem = CreateListViewItem(mitem, node);
                lstKeys.Items.Add(lvitem);
                nodetag._lvitem = lvitem;
            }
            node.Tag = nodetag;

            if (mitem.HasDropDownItems)
            {
                foreach (ToolStripItem it in mitem.DropDownItems)
                {
                    AddTreeItem(node, it);
                }
            }
        }

        private MenuStrip _ms;
        private void InitializeTree()
        {
            TreeNode top = new TreeNode(_ms.Text);
            foreach (ToolStripItem item in _ms.Items)
            {
                if (item is ToolStripSeparator)
                    continue;

                AddTreeItem(top, item);
            }
            tvMenu.Nodes.Add(top);
            top.Expand();
        }

        public ShortcutsManageDialog(MenuStrip ms)
        {
            InitializeComponent();
            _ms = ms;
            InitializeTree();
        }

        private void lstKeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            NodeTag nt;
            if (lstKeys.SelectedItems.Count == 0)
            {


                return;
            }

            TreeNode node = (TreeNode)lstKeys.SelectedItems[0].Tag;
            tvMenu.SelectedNode = node;


            nt = (NodeTag)node.Tag;
            if (nt == null)
            {

                return;
            }



            return;

        }

        private void tvMenu_AfterSelect(object sender, TreeViewEventArgs e)
        {
            bool add;
            bool modify;
            bool remove;
            AfterSelectStuff(out add, out modify, out remove);
            btnAdd.Enabled = add;
            btnModify.Enabled = modify;
            btnRemove.Enabled = remove;
        }
        private void AfterSelectStuff(out bool add, out bool modify, out bool remove)
        {
            add = modify = remove = false;
            NodeTag nt = (NodeTag)tvMenu.SelectedNode.Tag;
            if (nt == null)
                return;

            if (nt._lvitem != null)
            {
                nt._lvitem.Selected = true;
                lstKeys.EnsureVisible(nt._lvitem.Index);
                add = false;
                modify = remove = true;
            }
            else
            {
                if (lstKeys.SelectedItems.Count != 0)
                {
                    lstKeys.SelectedItems[0].Selected = false;
                }
                add = !nt._tsitem.HasDropDownItems;
                modify = remove = false;
            }

        }

        public void SaveIni(string section, HashIni hashini)
        {
            foreach (TreeNode n in tvMenu.Nodes)
            {
                SaveStuff(section, hashini, n);
            }
        }

        public void SaveIni(string section, string filename)
        {
            HashIni hashini = Profile.ReadAll(filename);
            SaveIni(section, hashini);
            Profile.WriteAll(hashini, filename);
        }

    
        private void SaveStuff(string section, HashIni hashini, TreeNode node)
        {

            NodeTag nt = (NodeTag)node.Tag;
            if (nt != null && nt._tsitem != null && !nt._tsitem.HasDropDownItems)
            {
                if (!string.IsNullOrEmpty(nt._tsitem.Name))
                {
                    Profile.WriteInt(
                        section,
                        nt._tsitem.Name,
                        (int)nt._tsitem.ShortcutKeys,
                        hashini);
                }
            }

            foreach (TreeNode n in node.Nodes)
            {
                SaveStuff(section, hashini, n);
            }
        }

        public void LoadIni(string section, HashIni hashini)
        {
            foreach (TreeNode n in tvMenu.Nodes)
            {
                LoadStuff(section, hashini, n);
            }
        }
        public void LoadIni(string section, string filename)
        {
            HashIni hashini = Profile.ReadAll(filename);
            LoadIni(section, hashini);
        }
        private static KeysConverter keyConverter = new KeysConverter();
        private string GetDisplayShortcutString(Keys k)
        {
            return keyConverter.ConvertToString(k);
        }

        private void LoadStuff(string section, HashIni hashini, TreeNode node)
        {

            NodeTag nt = (NodeTag)node.Tag;
            if (nt != null && nt._tsitem != null && !nt._tsitem.HasDropDownItems)
            {
                if (!string.IsNullOrEmpty(nt._tsitem.Name))
                {
                    int val ;
                    if (Profile.GetInt(
                        section,
                        nt._tsitem.Name,
                        -1,
                        out val,
                        hashini))
                    {
                        nt._tsitem.ShortcutKeys = (Keys)val;
                        if (nt._lvitem != null)
                        {
                            nt._lvitem.SubItems[0].Text = GetDisplayShortcutString(nt._tsitem.ShortcutKeys);
                        }
                        else if (nt._tsitem.ShortcutKeys != Keys.None)
                        {
                            ListViewItem lvitem = CreateListViewItem(nt._tsitem, node);
                            lstKeys.Items.Add(lvitem);
                            nt._lvitem = lvitem;
                        }
                    }
                }
            }

            foreach (TreeNode n in node.Nodes)
            {
                LoadStuff(section, hashini, n);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            TreeNode node = tvMenu.SelectedNode;
            if (node == null)
                return;

            NodeTag nt = (NodeTag)node.Tag;
            if (nt == null)
                return;

            if (nt._tsitem == null)
                return;

            if (nt._tsitem.HasDropDownItems)
                return;

            ChooseShortcutDialog dlg = new ChooseShortcutDialog();
            dlg.InitialKey= nt._tsitem.ShortcutKeys;
            if (System.Windows.Forms.DialogResult.OK != dlg.ShowDialog())
                return;

            if (dlg.ShortcutKeys == Keys.None)
                return;


            if (nt._tsitem.ShortcutKeys != dlg.ShortcutKeys)
            {
                if (_cakh != null)
                {
                    if (!_cakh(nt._tsitem, dlg.ShortcutKeys))
                    {
                        return;
                    }
                }

                nt._tsitem.ShortcutKeys = dlg.ShortcutKeys;
                ShortcutChanged(nt._tsitem, EventArgs.Empty);
            }


            ListViewItem newitem = CreateListViewItem(nt._tsitem, node);
            lstKeys.Items.Add(newitem);
            nt._lvitem = newitem;

            tvMenu_AfterSelect(tvMenu, null);
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            NodeTag nt = (NodeTag)tvMenu.SelectedNode.Tag;
            if (nt == null)
                return;

            if (lstKeys.SelectedItems.Count == 0)
                return;

            if (nt._lvitem == null)
                return;

            if (nt._tsitem == null)
                return;

            if (nt._tsitem.HasDropDownItems)
                return;


            ChooseShortcutDialog dlg = new ChooseShortcutDialog();
            dlg.InitialKey= nt._tsitem.ShortcutKeys;
            if (System.Windows.Forms.DialogResult.OK != dlg.ShowDialog())
                return;


            if (nt._tsitem.ShortcutKeys != dlg.ShortcutKeys)
            {
                if (_cakh != null)
                {
                    if (!_cakh(nt._tsitem, dlg.ShortcutKeys))
                    {
                        return;
                    }
                }
                nt._tsitem.ShortcutKeys = dlg.ShortcutKeys;
                ShortcutChanged(nt._tsitem, EventArgs.Empty);
            }




            nt._lvitem.SubItems[0].Text = GetDisplayShortcutString(nt._tsitem.ShortcutKeys);
            lstKeys.Sort();
            tvMenu_AfterSelect(tvMenu, null);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem mitem in _origs.Keys)
            {
                if (mitem.ShortcutKeys != _origs[mitem])
                {
                    mitem.ShortcutKeys = _origs[mitem];
                    ShortcutChanged(mitem, EventArgs.Empty);
                }
            }
        }

        private void ShortcutsManageDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            _origs = new Dictionary<ToolStripMenuItem, Keys>();

        }

        public delegate bool RemoveConfirmMessageBoxHandler(ToolStripMenuItem item);
        private RemoveConfirmMessageBoxHandler _rcmbh;
        public RemoveConfirmMessageBoxHandler RemoveConfirmMessageBox
        {
            get
            {
                return _rcmbh;
            }
            set
            {
                _rcmbh = value;
            }
        }


        public delegate void UnassignableAlertHandler(Keys k, Exception e);
        public UnassignableAlertHandler UnassignableAlertBox
        {
            get
            {
                return ChooseShortcutDialog._uaa;
            }
            set
            {
                ChooseShortcutDialog._uaa = value;
            }
        }
        private void btnRemove_Click(object sender, EventArgs e)
        {
            NodeTag nt = (NodeTag)tvMenu.SelectedNode.Tag;
            if (nt == null)
                return;

            if (lstKeys.SelectedItems.Count == 0)
                return;

            if (nt._lvitem == null)
                return;

            if (nt._tsitem == null)
                return;

            if (nt._tsitem.HasDropDownItems)
                return;

            if (_rcmbh != null)
            {
                if(!_rcmbh(nt._tsitem))
                {
                    return;
                }
            }

            if (nt._tsitem.ShortcutKeys != Keys.None)
            {
                nt._tsitem.ShortcutKeys = Keys.None;
                ShortcutChanged(nt._tsitem, EventArgs.Empty);

            }
            lstKeys.Items.Remove(nt._lvitem);
            nt._lvitem = null;

            tvMenu_AfterSelect(tvMenu, null);
        }

        private ImageList _icons = new ImageList();
        private void ShortcutsManageDialog_Load(object sender, EventArgs e)
        {
            ChooseShortcutDialog.CreateStatic();

            tvMenu.Nodes.Clear();
            lstKeys.Items.Clear();
            _origs = new Dictionary<ToolStripMenuItem, Keys>();
            _icons = new ImageList();
            _icons.Images.Add(Properties.Resources.Image1, Color.White);
            //tvMenu.ImageList = _icons;
            lstKeys.SmallImageList = _icons;
          
            InitializeTree();
        }
        public delegate ToolStripMenuItem DuplicateConfirmMessageBoxHandler(List<List<ToolStripMenuItem>> dupitemslist);
        private DuplicateConfirmMessageBoxHandler _dcmbh;
        public DuplicateConfirmMessageBoxHandler DuplicateConfirmMessageBox
        {
            get
            {
                return _dcmbh;
            }
            set
            {
                _dcmbh = value;
            }
        }
        private bool _cancelClose = false;
        private ToolStripMenuItem GetMenuItemFromLvItem(ListViewItem lvitem)
        {
            TreeNode tn = (TreeNode)lvitem.Tag;
            NodeTag nt = (NodeTag)tn.Tag;
            return nt._tsitem;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_dcmbh == null)
                return;

            string prevname = string.Empty;
            int count = lstKeys.Items.Count;
            List<List<ToolStripMenuItem>> dupitemslist = new List<List<ToolStripMenuItem>>();
            List<ToolStripMenuItem> dupitems = new List<ToolStripMenuItem>();

            for (int i = 0; i < count; ++i)
            {
                if (lstKeys.Items[i].Text == prevname)
                {
                    dupitems.Add(GetMenuItemFromLvItem(lstKeys.Items[i-1]));
                    dupitems.Add(GetMenuItemFromLvItem(lstKeys.Items[i]));
                    try
                    {
                        ++i;
                        for (; i < count; ++i)
                        {
                            if (lstKeys.Items[i].Text == prevname)
                            {
                                dupitems.Add(GetMenuItemFromLvItem(lstKeys.Items[i]));
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }

                    dupitemslist.Add(dupitems);
                    dupitems = new List<ToolStripMenuItem>();
                    --i;                    
                }
                else { prevname = lstKeys.Items[i].Text; 
                }
            }

            if (dupitemslist.Count != 0)
            {
                ToolStripMenuItem item = _dcmbh(dupitemslist);
                if ( item != null)
                {
                    foreach (ListViewItem lvitem in lstKeys.Items)
                    {
                        if (GetMenuItemFromLvItem(lvitem) == item)
                        {
                            lvitem.Selected = true;
                        }
                    }
                    _cancelClose = true;
                    return;
                }
            }
        }

        private void ShortcutsManageDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cancelClose)
            {
                _cancelClose = false;
                e.Cancel = true;
            }
        }

        private void lstKeys_ItemActivate(object sender, EventArgs e)
        {
            btnModify.PerformClick();
        }

        //public delegate void ShortcuChangedHander(Object sender, EventArgs e);
        public event EventHandler ShortcutChanged = new EventHandler(dummyfunc);
        private static void dummyfunc(Object sender, EventArgs e)
        {
        }


        public delegate bool CanAssignKeyHandler(ToolStripMenuItem item, Keys key);
        private CanAssignKeyHandler _cakh;
        public CanAssignKeyHandler CanAssignKey
        {
            get
            {
                return _cakh;
            }
            set
            {
                _cakh = value;
            }
        }

    }
}