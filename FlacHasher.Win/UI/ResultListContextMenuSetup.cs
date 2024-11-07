using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win.UI
{
    public static class ResultListContextMenuSetup
    {
        public static void WireUp(
            FileHashResultList resultList,
            ContextMenuStrip contextMenu,
            Action saveHashesAction,
            Action<bool> copyResultsAction)
        {
            void resultList_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Right) return;

                var hasAnyData = resultList.BackingData.Any(x => x.Value?.HashString != null);
                contextMenu.Enabled = hasAnyData;

                contextMenu.Show(resultList, new Point(e.X, e.Y));
            }

            resultList.MouseDown += new MouseEventHandler(resultList_MouseDown);

            BuildResultsCtxMenu(contextMenu, saveHashesAction, copyResultsAction);
        }

        private static void BuildResultsCtxMenu(
            ContextMenuStrip contextMenu,
            Action saveHashesAction,
            Action<bool> copyResultsAction)
        {
            var saveEventHandler = new EventHandler(
                (sender, e) => saveHashesAction());

            contextMenu.Items.Add(
                "Save to a File...",
                null,
                saveEventHandler);

            contextMenu.Items.Add(
                "Copy",
                null,
                new EventHandler(
                    (sender, e) => copyResultsAction(false)));

            contextMenu.Items.Add(
                "Copy (Using Save formatting)",
                null,
                new EventHandler(
                    (sender, e) => copyResultsAction(true)));
        }

        
    }
}