using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AI.Labs.Win.Editors
{
    public partial class ChatItems : UserControl
    {
        public ChatItems()
        {
            InitializeComponent();
        }
        public GridControl GridControl { get => this.gridControl1; }
        public ItemsView ItemsView { get => this.itemsView1; }
    }
}
