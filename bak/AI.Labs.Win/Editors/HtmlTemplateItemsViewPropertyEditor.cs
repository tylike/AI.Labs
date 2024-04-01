using AI.Labs.Module;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Office.Win;
using DevExpress.ExpressApp.Win.Controls;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Items;
using DevExpress.XtraRichEdit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Labs.Win.Editors
{
    //[PropertyEditor(typeof(string), "HtmlPropertyEditor", false)]
    //public class HtmlPropertyEditor : WinPropertyEditor,IInplaceEditSupport
    //{
    //    private int rowCount = 10;

    //    public new RichTextBoxWithBinding Control => (RichTextBoxWithBinding)base.Control;

    //    public int RowCount
    //    {
    //        get
    //        {
    //            return rowCount;
    //        }
    //        set
    //        {
    //            rowCount = value;
    //        }
    //    }

    //    private void Editor_RtfTextChanged(object sender, EventArgs e)
    //    {
    //        if (!base.IsValueReading)
    //        {
    //            OnControlValueChanged();
    //        }
    //    }

    //    protected override object CreateControlCore()
    //    {
    //        RichTextBoxWithBinding richTextBoxWithBinding = new RichTextBoxWithBinding();
    //        if (rowCount != 0)
    //        {
    //            richTextBoxWithBinding.Height = rowCount * richTextBoxWithBinding.PreferredHeight + 2;
    //        }
    //        else
    //        {
    //            richTextBoxWithBinding.Height = 15 * richTextBoxWithBinding.PreferredHeight + 2;
    //        }

    //        if (base.MaxLength > 0)
    //        {
    //            richTextBoxWithBinding.MaxLength = base.MaxLength;
    //        }

    //        richTextBoxWithBinding.RtfTextChanged += Editor_RtfTextChanged;
    //        richTextBoxWithBinding.ScrollBars = RichTextBoxScrollBars.Both;
    //        richTextBoxWithBinding.WordWrap = false;
    //        return richTextBoxWithBinding;
    //    }

    //    protected override void UpdateControlEnabled(bool enabled)
    //    {
    //        if (Control != null)
    //        {
    //            Control.ReadOnly = !enabled;
    //        }
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        try
    //        {
    //            if (disposing && Control != null)
    //            {
    //                Control.RtfTextChanged -= Editor_RtfTextChanged;

    //            }
    //        }
    //        finally
    //        {
    //            base.Dispose(disposing);
    //        }
    //    }

    //    public HtmlPropertyEditor(Type objectType, IModelMemberViewItem model)
    //        : base(objectType, model)
    //    {
    //        base.ControlBindingProperty = "RtfText";
    //        rowCount = model.RowCount;
    //    }

    //    public RepositoryItem CreateRepositoryItem()
    //    {
    //        RepositoryItemRtfEditEx repositoryItemRtfEditEx = new RepositoryItemRtfEditEx();
    //        repositoryItemRtfEditEx.ReadOnly = !base.AllowEdit;
    //        return repositoryItemRtfEditEx;
    //    }
    //}

    [PropertyEditor(typeof(string), LabsModule.HtmlPropertyEditor, false)]
    public class HtmlContentPropertyEditor : WinPropertyEditor
    {
        public HtmlContentPropertyEditor(Type objectType, IModelMemberViewItem model) : base(objectType, model)
        {
        }
        protected override object CreateControlCore()
        {
            control = new RichEditControl();// new HtmlContentControl();
            control.ActiveViewType = RichEditViewType.Simple;
            control.HtmlTextChanged += Control_HtmlTextChanged;
            control.Dock = System.Windows.Forms.DockStyle.Fill;
            return control;            
        }

        private void Control_HtmlTextChanged(object sender, EventArgs e)
        {
            WriteValue();
        }

        private RichEditControl control = null;

        protected override void ReadValueCore()
        {
            if (control != null)
            {
                control.HtmlText = (string)PropertyValue;
            }
        }
        protected override void WriteValueCore()
        {
            //base.WriteValueCore();
            if (control != null)
            {
                PropertyValue = control.HtmlText;
            }
        }

        protected override void OnControlCreated()
        {
            base.OnControlCreated();
            ReadValue();
        }
    }

    [PropertyEditor(typeof(IEnumerable), LabsModule.HtmlTemplateItemsPropertyEditor, false)]
    public class HtmlTemplateItemsViewPropertyEditor : WinPropertyEditor
    {
        static string MessageTemplate;
        static string MessageCss;
        static HtmlTemplateItemsViewPropertyEditor()
        {
            var baseFile = typeof(HtmlTemplateItemsViewPropertyEditor).Assembly.Location;
            var fileInfo = new FileInfo(baseFile);
            var baseDir = fileInfo.Directory.FullName + @"\template\";
            MessageTemplate = File.ReadAllText(baseDir + @"message.html");
            MessageCss = File.ReadAllText(baseDir + @"message.css");

        }
        public HtmlTemplateItemsViewPropertyEditor(Type objectType, IModelMemberViewItem model) : base(objectType, model)
        {
        }
        //GridControl gridControl;
        //ItemsView itemsView;
        ChatItems items;

        protected override object CreateControlCore()
        {
            if (items == null)
            {
                items = new ChatItems();
                items.ItemsView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn() { FieldName = "Message", Name = "Message" });
                items.ItemsView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn() { FieldName = "User.Photo", Name = "Photo" });
                items.ItemsView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn() { FieldName = "User.NickName", Name = "NickName" });

                items.ItemsView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn() { FieldName = "DateTime", Name = "Time" });

                items.ItemsView.QueryItemTemplate += ItemsView_QueryItemTemplate;
            }
            return items;
        }

        private void ItemsView_QueryItemTemplate(object sender, QueryItemTemplateEventArgs e)
        {
            e.Template.Template = MessageTemplate;
            e.Template.Styles = MessageCss;
        }

        //private void ItemsView_QueryItemTemplate(object sender, QueryItemTemplateEventArgs e)
        //{

        //}

        protected override object GetControlValueCore()
        {
            return items.GridControl.DataSource;
        }

        protected override void ReadValueCore()
        {
            items.GridControl.DataSource = this.PropertyValue;
        }
    }
}
