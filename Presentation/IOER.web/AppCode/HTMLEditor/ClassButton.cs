using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using Obout.Ajax.UI;
using Obout.Ajax.UI.HTMLEditor;
using Obout.Ajax.UI.HTMLEditor.ToolbarButton;
using Obout.Ajax.UI.HTMLEditor.QF;

//using CustomPopups = ILPathways.HTMLEditor.CustomPopups;

//namespace ILPathways.CustomToolbarButton
namespace CustomToolbarButton
{
    [ParseChildren(true)]
    [PersistChildren(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal class ClassNameSelector : Selector
    {
        #region [ Properties ]

        public override Obout.Ajax.UI.HTMLEditor.Popups.Popup RelatedPopup
        {
            get
            {
                if (base.RelatedPopup == null)
                {
                    base.RelatedPopup = new CustomPopupsHere.ClassNamesPopup();
                }
                return base.RelatedPopup;
            }
        }

        // what client-side type to initiate
        protected override string ClientControlType
        {
            get { return "CustomToolbarButton.ClassNameSelector"; }
        }

        // what file in the client-side type is located
        protected override string ScriptPath
        {
            get { return "~/App_Obout/HTMLEditor/Scripts/ClassNameButton.js"; }
        }

        // tooltip if not found in Localization
        public override string DefaultToolTip
        {
            get { return "Select class name"; }
        }

        // put up CSS file used by QuickFormatting
        public override string AdditionalCSS
        {
            get
            {
                return (RelatedPopup as CustomPopupsHere.ClassNamesPopup).InnerQuickFormatting.SourceFile;
            }
        }

        #endregion
    }

    [ParseChildren(true)]
    [PersistChildren(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal class ClassNameField : DesignModeBoxButton
    {
        #region [ Fields ]

        private TableCell cell = null;
        private string selectorID = "";

        #endregion

        #region [ Properties ]

        // what client-side type to initiate
        protected override string ClientControlType
        {
            get { return "CustomToolbarButton.ClassNameField"; }
        }

        // what file in the client-side type is located
        protected override string ScriptPath
        {
            get { return "~/App_Obout/HTMLEditor/Scripts/ClassNameButton.js"; }
        }

        public string SelectorID
        {
            get
            {
                return selectorID;
            }
            set
            {
                selectorID = value;
            }
        }

        #endregion

        #region [ Methods ]

        protected override void CreateChildControls()
        {
            Table table = new Table();
            table.Attributes.Add("border", "0");
            table.Attributes.Add("cellspacing", "0");
            table.Attributes.Add("cellpadding", "0");
            table.Style[HtmlTextWriterStyle.Overflow] = "hidden !important";
            table.Style[HtmlTextWriterStyle.Margin] = "0px";
            table.Style[HtmlTextWriterStyle.Padding] = "0px";
            table.Style[HtmlTextWriterStyle.Height] = "100%";
            table.Style[HtmlTextWriterStyle.Cursor] = "text";
            TableRow row = new TableRow();
            table.Rows.Add(row);
            cell = new TableCell();
            cell.Style[HtmlTextWriterStyle.VerticalAlign] = "middle !important";
            HtmlGenericControl span = new HtmlGenericControl("span");
            span.Controls.Add(new LiteralControl(this.GetFromResource("CssHeader", "CSS") + ":&nbsp;"));
            span.Style[HtmlTextWriterStyle.FontSize] = "12px";
            span.Style[HtmlTextWriterStyle.VerticalAlign] = "middle";
            span.Style[HtmlTextWriterStyle.FontFamily] = "verdana";
            cell.Controls.Add(span);
            cell.Style[HtmlTextWriterStyle.BackgroundColor] = "#E0E0E0";
            row.Cells.Add(cell);
            cell = new TableCell();
            row.Cells.Add(cell);
            cell.VerticalAlign = VerticalAlign.Middle;
            cell.HorizontalAlign = HorizontalAlign.Left;
            //cell.Style[HtmlTextWriterStyle.VerticalAlign] = "middle !important";
            cell.Style[HtmlTextWriterStyle.Width] = "100px";
            cell.Style[HtmlTextWriterStyle.Overflow] = "hidden";
            cell.Style[HtmlTextWriterStyle.BackgroundColor] = "white";
            cell.Controls.Add(new LiteralControl(""));

            Content.Add(table);

            base.CreateChildControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (cell.ClientID.Length > 0) cell.Attributes.Add("id", cell.ClientID);
            this.Style[HtmlTextWriterStyle.Padding] = "1px";
            base.OnPreRender(e);
        }

        protected override void DescribeComponent(ScriptComponentDescriptor descriptor)
        {
            descriptor.AddElementProperty("innerCell", cell.ClientID);
            descriptor.AddProperty("selectorID", selectorID);
            base.DescribeComponent(descriptor);
        }

        #endregion
    }

    [ButtonsList(true)]
    public class ClassName : ButtonsGroup
    {
        #region [ Fields ]

        ClassNameField classNameField;
        ClassNameSelector classNameSelector;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Initializes a new ClassName button
        /// </summary>
        public ClassName()
            : base()
        {
            classNameField = new ClassNameField();
            this.Buttons.Add(classNameField);
            classNameSelector = new ClassNameSelector();
            classNameSelector.ID = "Selector";
            this.Buttons.Add(classNameSelector);
        }

        protected override void OnPreRender(EventArgs e)
        {
            classNameField.SelectorID = classNameSelector.ClientID;
            base.OnPreRender(e);
        }

        #endregion
    }
}

namespace CustomPopupsHere
{
    //[ClientCssResource("Obout.Ajax.UI.HTMLEditor.QF.QuickFormatting.css")]
    [RequiredScript(typeof(Toolbar))]
    [RequiredScript(typeof(QuickFormatting))]
    [RequiredScript(typeof(StyleItem))]
    [RequiredScript(typeof(Obout.Ajax.UI.HTMLEditor.Popups.Popup))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public class ClassNamesPopup : Obout.Ajax.UI.HTMLEditor.Popups.Popup
    {
        #region [ Fields ]

        internal QuickFormattingInPopup _quickFormatting = null;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the inner QuickFormatting control.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [NotifyParentProperty(true)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Bindable(true)]
        public virtual QuickFormattingInPopup InnerQuickFormatting
        {
            get
            {
                if (_quickFormatting == null)
                {
                    _quickFormatting = new QuickFormattingInPopup();
                }
                return _quickFormatting;
            }
        }

        protected override string ScriptPath
        {
            get { return "~/App_Obout/HTMLEditor/Scripts/ClassNamesPopup.js"; }
        }

        protected override string ClientControlType
        {
            get { return "CustomPopup.ClassNamesPopup"; }
        }

        #endregion

        #region [ Methods ]

        protected override void FillContent()
        {
            Table table = new Table();
            TableRow row = null;
            TableCell cell;

            InnerQuickFormatting.EnsureChildControls();

            Collection<StyleItem> items = new Collection<StyleItem>();
            foreach (Control control in InnerQuickFormatting.Controls[1].Controls)
            {
                StyleItem item = control as StyleItem;
                items.Add(item);
            }

            int n=0;
            foreach (StyleItem item in items)
            {
                row = new TableRow();
                table.Rows.Add(row);
                cell = new TableCell();
                cell.Attributes["class"] = "oae_quickformatting";
                ButtonInPopup popupButton = new ButtonInPopup(item);
                RegisteredHandlers.Add(new Obout.Ajax.UI.HTMLEditor.Popups.RegisteredField("button_" + n.ToString(), popupButton));
                n++;
                cell.Controls.Add(popupButton);
                cell.Style[HtmlTextWriterStyle.BackgroundColor] = "Transparent";
                cell.Style[HtmlTextWriterStyle.FontSize] = "10pt";
                cell.Style[HtmlTextWriterStyle.Padding] = "0px";
                cell.Style[HtmlTextWriterStyle.FontFamily] = "Verdana";
                cell.Style[HtmlTextWriterStyle.Color] = "Blue";
                cell.Style[HtmlTextWriterStyle.Cursor] = "pointer";
                row.Cells.Add(cell);
            }

            table.Attributes.Add("border", "0");
            table.Attributes.Add("cellspacing", "0");
            table.Attributes.Add("cellpadding", "0");
            table.Style["background-color"] = "transparent";

            Content.Add(table);
        }
        protected override void OnPreRender(EventArgs e)
        {
            // no padding inside the popup
            this.Container.Style[HtmlTextWriterStyle.Padding] = "0px";
            // call base method
            base.OnPreRender(e);
        }

        protected override void DescribeComponent(ScriptComponentDescriptor descriptor)
        {
            // add to the popup link to CSS file used for the 'QuickFormatting control'
            descriptor.AddProperty("additionalCss", Page.ClientScript.GetWebResourceUrl(typeof(QuickFormatting), "Obout.Ajax.UI.HTMLEditor.QF.QuickFormatting.css"));
            // call base method
            base.DescribeComponent(descriptor);
        }

        #endregion
    
        #region [ QuickFormattingInPopup ]

        [ToolboxItem(false)]
        [RequiredScript(typeof(Toolbar))]
        public sealed class QuickFormattingInPopup : QuickFormatting
        {
            internal new void EnsureChildControls()
            {
                SourceFile = SourceFile;
                base.EnsureChildControls();
            }
        }
        #endregion

        #region [ Custom button for the popup ]

        [ParseChildren(true)]
        [PersistChildren(false)]
        [RequiredScript(typeof(CommonToolkitScripts))]
        [RequiredScript(typeof(StyleItem))]
        private class ButtonInPopup : Obout.Ajax.UI.HTMLEditor.Popups.PopupBoxButton
        {
            #region [ Fields ]

            private StyleItem _item;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Initializes a new ButtonInPopup
            /// </summary>
            public ButtonInPopup(StyleItem item)
                : base(HtmlTextWriterTag.Span)
            {
                _item = item;
                this.CssClass = item.CssClass;
            }

            #endregion

            #region [ Properties ]

            protected override string ScriptPath
            {
                get { return "~/App_Obout/HTMLEditor/Scripts/ClassNamesPopup.js"; }
            }

            protected override string ClientControlType
            {
                get { return "CustomPopup.ClassNameItem"; }
            }
            
            #endregion

            #region [ Methods ]

            protected override void CreateChildControls()
            {
                Content.Add(_item);
                base.CreateChildControls();
            }

            protected override void DescribeComponent(ScriptComponentDescriptor descriptor)
            {
                descriptor.AddProperty("styleItemID", _item.ClientID);
                base.DescribeComponent(descriptor);
            }

            #endregion
        }
        #endregion
    }
}