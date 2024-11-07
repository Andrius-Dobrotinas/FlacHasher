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
            Action<IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>>> saveHashesAction,
            Action<IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>>, bool> copyResultsAction)
        {
            void resultList_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Right) return;

                var hasAnyData = resultList.BackingData.Any(x => x.Value?.HashString != null);
                contextMenu.Enabled = hasAnyData;

                contextMenu.Show(resultList, new Point(e.X, e.Y));
            }

            resultList.MouseDown += new MouseEventHandler(resultList_MouseDown);

            BuildResultsCtxMenu(resultList, contextMenu, saveHashesAction, copyResultsAction);
        }

        private static void BuildResultsCtxMenu(
            FileHashResultList resultList,
            ContextMenuStrip contextMenu,
            Action<IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>>> saveHashesAction,
            Action<IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>>, bool> copyResultsAction)
        {
            var saveEventHandler = new EventHandler(
                (sender, e) => saveHashesAction(resultList.BackingData));

            contextMenu.Items.Add(
                "Save to a File...",
                null,
                saveEventHandler);

            contextMenu.Items.Add(
                "Copy",
                null,
                new EventHandler(
                    (sender, e) => copyResultsAction(GetData(resultList), false)));

            contextMenu.Items.Add(
                "Copy (Using Save formatting)",
                null,
                new EventHandler(
                    (sender, e) => copyResultsAction(GetData(resultList), true)));
        }

        static IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>> GetData(FileHashResultList resultList)
        {
            var selectedItems = resultList.ListViewItems.Where(x => x.Selected);
            if (selectedItems.Any())
                return selectedItems.Select(x => new KeyValuePair<FileInfo, FileHashResultListItem>(x.Key, x.Data));
            else
                return resultList.BackingData;
        }
    }
}