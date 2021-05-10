using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Ambiesoft.ShortcutsManager
{
    partial class ChooseShortcutDialog : Form
    {
        private static List<ComboItem> _availables = new List<ComboItem>();
        //private static bool _performed=false;
        //private static void onclick(object s, EventArgs e)
        //{
        //    _performed = true;
        //}
        //private static Object _m;
        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    _m = msg;
        //    return base.ProcessCmdKey(ref msg, keyData);
        //}
        //private static bool _threadstarted = false;
        private static System.Threading.Thread _thread;
        private static long _constructed = 0;
        static ChooseShortcutDialog()
        {
                //_threadstarted = true;
                _thread = new System.Threading.Thread(new System.Threading.ThreadStart(CreateComboItemsThread));
                _thread.Start();
        }
        public static void CreateStatic() { }

        private static void CreateComboItemsThread()
        {
            try
            {
                //tsTester. Click += new EventHandler(onclick);
                int max = System.Math.Min((int)Keys.Control, (int)Keys.Shift);
                max = System.Math.Min(max, (int)Keys.Alt);
                max--;

                ToolStripMenuItem tsTester = new ToolStripMenuItem("TEST");
                int i = 0;
                for (; ; )
                {
                    try
                    {
                        for (; i < max; ++i)
                        {
                            tsTester.ShortcutKeys = (Keys)i | Keys.Control;
                            if (ComboItem.IsAvailable((Keys)i))
                            {
                                //_performed = false;
                                //tsTester.PerformClick();
                                //if (!_performed)
                                //    continue;

                                //_performed = false;
                                //Message m = new Message();
                                //m.HWnd = ((IWin32Window)this).Handle;
                                //m.Msg = WM_KEYDOWN;
                                //m.

                                //if(this.ProcessCmdKey(ref m, tsTester.ShortcutKeys) && _performed)
                                //    _availables.Add(new ComboItem((Keys)i));

                                _availables.Add(new ComboItem((Keys)i));
                            }
                        }
                        return;
                    }
                    catch (Exception)
                    {
                        ++i;
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                System.Threading.Interlocked.Exchange(ref _constructed, 1);
            }
        }

        public ChooseShortcutDialog()
        {
            InitializeComponent();
        }

        private Keys _initialkey = Keys.None;
        public Keys InitialKey
        {
            get
            {
                return _initialkey;
            }
            set
            {
                _initialkey = value;
            }
        }

        private void ChooseShortcutDialog_Load(object sender, EventArgs e)
        {
            Keys shortCut = InitialKey;
            chkCtrl.Checked = (shortCut & Keys.Control) == Keys.Control;
            chkAlt.Checked = (shortCut & Keys.Alt) == Keys.Alt;
            chkShift.Checked = (shortCut & Keys.Shift) == Keys.Shift;

            // To get the letter we have to eliminate the modifyers. This
            // is done by combining all set modifyers and then by removing
            // them from the shortcut with XOR.
            // Example:	Ctrl	 = 00100000
            //			Alt		 = 01000000 OR
            //			-------------------
            //			modifyer = 01100000
            //
            //			shortcut = 01100010
            //			modifyer = 01100000 XOR
            //			-------------------
            //			Key		 = 00000010
            Keys modifiers = Keys.None;
            if (chkCtrl.Checked) modifiers |= Keys.Control;
            if (chkAlt.Checked) modifiers |= Keys.Alt;
            if (chkShift.Checked) modifiers |= Keys.Shift;
            Keys buchstabe = shortCut ^ modifiers;

            if (0 == System.Threading.Interlocked.Read(ref _constructed))
            {
                _thread.Join();
            }

            cmbKeys.Items.AddRange(_availables.ToArray());
            cmbKeys.SelectedValue = buchstabe;
            cmbKeys.SelectedItem = new ComboItem(buchstabe);
        }

        public Keys ShortcutKeys
        {
            get
            {
                if (cmbKeys.SelectedItem == null)
                    return Keys.None;

                Keys ret= ((ComboItem)cmbKeys.SelectedItem).KeyCode;
                if (chkCtrl.Checked) ret |= Keys.Control;
                if (chkAlt.Checked) ret |= Keys.Alt;
                if (chkShift.Checked) ret |= Keys.Shift;
                return ret;
            }
        }

        private bool _closecancel = false;
        ToolStripMenuItem tsTester_ = new ToolStripMenuItem("TEST");
        private void btnOK_Click(object sender, EventArgs e)
        {
            
            _closecancel = false;
            try
            {
                tsTester_.ShortcutKeys = this.ShortcutKeys;
            }
            catch (Exception ex)
            {
                if (_uaa != null)
                {
                    _uaa(this.ShortcutKeys, ex);
                }
                else
                {
                    MessageBox.Show(ex.Message,
                        Application.ProductName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                _closecancel = true;
            }
        }

        private void ChooseShortcutDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_closecancel)
            {
                _closecancel = false;
                e.Cancel = true;
            }
        }

        
        internal static ShortcutsManageDialog.UnassignableAlertHandler _uaa;
        internal static ShortcutsManageDialog.UnassignableAlertHandler UnassignableAlert
        {
            get
            {
                return _uaa;
            }
            set
            {
                _uaa = value;
            }
        }

        private void cmbKeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = cmbKeys.SelectedItem != null;
        }

    }
}