using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public class VerificationResultsListWrapper
    {
        private ListView list_verification_results;

        public VerificationResultsListWrapper(ListView list_verification_results)
        {
            this.list_verification_results = list_verification_results;
        }

        public void Add(FileInfo file, bool isMatch)
        {
            var item = new ListViewItem
            {
                Text = file.Name,
            };

            item.SubItems.Add(isMatch.ToString());

            this.list_verification_results.Items.Add(item);
        }
    }
}