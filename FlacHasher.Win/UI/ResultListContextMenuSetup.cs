using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win.UI
{
    public static class ResultListContextMenuSetup
    {
        public static void WireUp(
            FileHashResultList resultList,
            ContextMenuStrip contextMenu,
            Action<IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>>> saveHashesAction)
        {
            void resultList_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Right) return;

                var hasAnyData = resultList.BackingData.Any(x => x.Value?.HashString != null);
                contextMenu.Enabled = hasAnyData;

                contextMenu.Show(resultList, new Point(e.X, e.Y));
            }

            resultList.MouseDown += new MouseEventHandler(resultList_MouseDown);

            BuildResultsCtxMenu(resultList, contextMenu, saveHashesAction);
        }

        private static void BuildResultsCtxMenu(
            FileHashResultList resultList,
            ContextMenuStrip contextMenu,
            Action<IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>>> saveHashesAction)
        {
            var saveEventHandler = new EventHandler(
                (sender, e) => saveHashesAction(resultList.BackingData));

            contextMenu.Items.Add(
                "Save to a File...",
                null,
                saveEventHandler);
        }
    }
}