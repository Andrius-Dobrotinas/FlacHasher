using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    public class ResultsWrapper<T>
    {
        private readonly ListBox list_results;
        private readonly IFaceValueFactory<T> faceValueFactory;

        public ResultsWrapper(ListBox list_results, IFaceValueFactory<T> faceValueFactory)
        {
            this.list_results = list_results;
            this.list_results.DisplayMember = nameof(IListItem.FaceValue);

            this.faceValueFactory = faceValueFactory;
        }

        public void AddResult(T result)
        {
            list_results.Items.Add(
                new FileHashResultListItem<T>
                {
                    Result = result,
                    FaceValue = faceValueFactory.GetFaceValue(result)
                });
        }

        public void Clear()
        {
            list_results.Items.Clear();
        }

        public IEnumerable<string> GetFaceValues()
        {
            return list_results.Items
                .Cast<FileHashResultListItem<T>>()
                .Select(x => x.FaceValue);
        }
    }
}