using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public static class ResultListContextMenuSetup
    {
        public static void WireUp(
            ListBox resultList,
            ContextMenuStrip contextMenu,
            Action<IEnumerable<ListItem<FileHashResult>>> saveHashesAction)
        {
            void resultList_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Right) return;

                contextMenu.Show(resultList, new Point(e.X, e.Y));
            }

            resultList.MouseDown += new MouseEventHandler(resultList_MouseDown);

            BuildResultsCtxMenu(resultList, contextMenu, saveHashesAction);
        }

        private static void BuildResultsCtxMenu(
            ListBox resultList,
            ContextMenuStrip contextMenu,
            Action<IEnumerable<ListItem<FileHashResult>>> saveHashesAction)
        {
            var saveEventHandler = new EventHandler(
                (sender, e) => saveHashesAction(resultList.Items.Cast<ListItem<FileHashResult>>()));

            contextMenu.Items.Add(
                "Save to a File...",
                null,
                saveEventHandler);
        }
    }
}