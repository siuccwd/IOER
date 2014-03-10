using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web;
using Obout.Ajax.UI;
using Obout.Ajax.UI.HTMLEditor;
using Obout.Ajax.UI.HTMLEditor.ToolbarButton;


//using CustomToolbarButton = ILPathways.HTMLEditor.CustomToolbarButton;

namespace CustomEditor
{
    public class TestEditor : Editor
    {
        public override EditorTopToolbar TopToolbar
        {
            get
            {
                if (base.TopToolbar.GetType() != typeof(TestTopToolbar))
                {
                    base.TopToolbar = new TestTopToolbar();
                }
                return base.TopToolbar;
            }
        }

        public override EditorBottomToolbar BottomToolbar
        {
            get
            {
                if (base.BottomToolbar.GetType() != typeof(TestBottomToolbar))
                {
                    base.BottomToolbar = new TestBottomToolbar();
                }
                return base.BottomToolbar;
            }
        }

        public override EditorEditPanel EditPanel
        {
            get
            {
                if (base.EditPanel.GetType() != typeof(TestEditPanel))
                {
                    base.EditPanel = new TestEditPanel();
                }
                return base.EditPanel;
            }
        }

        public class TestTopToolbar : EditorTopToolbar
        {
            public TestTopToolbar()
            {
            }

            public override AppearanceType Appearance
            {
                get { return AppearanceType.Full; }
            }

            protected override void FullSet(Collection<CommonButton> col)
            {
                base.FullSet(col);
                col.Add(new Obout.Ajax.UI.HTMLEditor.ToolbarButton.HorizontalSeparator());
                col.Add( new CustomToolbarButton.InsertDate() );
            }
        }

        public class TestBottomToolbar : EditorBottomToolbar
        {
            public TestBottomToolbar()
            {
            }
        }

        public class TestEditPanel : EditorEditPanel
        {
            public override string HtmlPanelCssClass
            {
                get { return "MyHtmlPanel"; }
            }
        }
    }
}