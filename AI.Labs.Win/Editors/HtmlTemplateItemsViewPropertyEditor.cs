using AI.Labs.Module;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Labs.Win.Editors
{
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
