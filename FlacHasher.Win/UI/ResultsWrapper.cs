using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public class ResultsWrapper<TValue, TListItem> where TListItem : IListItem<TValue>, new()
    {
        private readonly ListBox list_results;
        private readonly IFaceValueFactory<TValue> faceValueFactory;

        public ResultsWrapper(ListBox list_results, IFaceValueFactory<TValue> faceValueFactory)
        {
            this.list_results = list_results;
            this.list_results.DisplayMember = nameof(IListItem<TValue>.FaceValue);

            this.faceValueFactory = faceValueFactory;
        }

        public void AddResult(TValue result)
        {
            list_results.Items.Add(
                new TListItem
                {
                    Value = result,
                    FaceValue = faceValueFactory.GetFaceValue(result)
                });
        }

        public void Clear()
        {
            list_results.Items.Clear();
        }
    }
}