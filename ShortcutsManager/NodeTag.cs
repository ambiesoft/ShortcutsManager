using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Ambiesoft.ShortcutsManager
{
     class NodeTag
    {
        public NodeTag(ToolStripMenuItem tsi, ListViewItem lvi)
        {
            _tsitem = tsi;
            _lvitem = lvi;
        }
        public ToolStripMenuItem _tsitem;
        public ListViewItem _lvitem;
    };

}
