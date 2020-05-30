using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    public class ResultsWrapper
    {
        private readonly ListBox list_results;

        public ResultsWrapper(ListBox list_results)
        {
            this.list_results = list_results;
            this.list_results.DisplayMember = nameof(IListItem.FaceValue);
        }

        public void AddResult(FileHashResult result, string faceValue)
        {
            list_results.Items.Add(
                new FormX.FileHashResultListItem
                {
                    Result = result,
                    FaceValue = faceValue
                });
        }

        public IEnumerable<string> GetFaceValues()
        {
            return list_results.Items
                .Cast<FormX.FileHashResultListItem>()
                .Select(x => x.FaceValue);
        }
    }
}