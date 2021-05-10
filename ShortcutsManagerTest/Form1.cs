using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Ambiesoft.ShortcutsManager;
namespace Ambiesoft.ShortcutsManagerTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private bool ConfirmSCRemove(ToolStripMenuItem item)
        {
            return MessageBox.Show(string.Format("Are you sure to remove {0}", item.Text),
                Application.ProductName,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes;
        }
        private void UnassignableAlert(Keys k, Exception e)
        {
            MessageBox.Show("This key is not available.",
                Application.ProductName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }
        private ToolStripMenuItem dupcheck(List<List<ToolStripMenuItem>> dupitemslist)
        {
            DialogResult r= MessageBox.Show(string.Format("Shortcut Key \"{0}\" is assigned on multiple items. Are you sure to continue?",dupitemslist[0][0].ShortcutKeys.ToString()),
                Application.ProductName,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if(r==DialogResult.Yes)
                return null;

            return dupitemslist[0][0];
            
        }
        private void ShortcutChanged(object sender, EventArgs e)
        {
        }
        private bool CanAssignKey(ToolStripMenuItem item, Keys key)
        {
            return true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string savefilename = Application.ExecutablePath + ".wini";

            ShortcutsManageDialog dlg =  new ShortcutsManageDialog(menuStrip1);
            dlg.CanAssignKey = CanAssignKey;
            dlg.RemoveConfirmMessageBox = new ShortcutsManageDialog.RemoveConfirmMessageBoxHandler(this.ConfirmSCRemove);
            dlg.UnassignableAlertBox = new ShortcutsManageDialog.UnassignableAlertHandler(this.UnassignableAlert);
            dlg.DuplicateConfirmMessageBox = new ShortcutsManageDialog.DuplicateConfirmMessageBoxHandler(this.dupcheck);
            dlg.ShortcutChanged += new EventHandler(ShortcutChanged);
            if (System.IO.File.Exists(savefilename))
                dlg.LoadIni("myshortcuts", savefilename);
            
            if (System.Windows.Forms.DialogResult.OK != dlg.ShowDialog())
                return;

            dlg.SaveIni("myshortcuts", savefilename);
        }

        private void zenbu_Click(object sender, EventArgs e)
        {
            MessageBox.Show(((ToolStripItem)sender).Text + " Clicked");
        }
    }
}