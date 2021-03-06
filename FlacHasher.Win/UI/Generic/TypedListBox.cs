﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public interface ITypedListBox<TValue, TListItem> where TListItem : IListItem<TValue>, new()
    {
        void AddItem(TValue result);
        void ClearList();
    }

    /// <summary>
    /// A type-safe implementation of <see cref="ListBox"/> which takes items of
    /// <see cref="IListItem{TValue}"/> type as a way to control item display values.
    /// A <see cref="IDisplayValueProducer{T}"/> must be provided via <see cref="TypedListBox.FaceValueFactory"/>
    /// setter in order for the component to be able derive an item display values.
    /// The reason it can't be injected via a constructor is so it's WinForms designer-friendly
    /// (generated code doesn't have to be amended with the injection of a dependency).
    /// </summary>
    public class TypedListBox<TValue, TListItem> : ListBox, ITypedListBox<TValue, TListItem>
         where TListItem : IListItem<TValue>, new()
    {
        public TypedListBox()
        {
            this.DisplayMember = nameof(IListItem<TValue>.DisplayValue);
        }

        public IDisplayValueProducer<TValue> DisplayValueProducer { get; set; }

        public void AddItem(TValue result)
        {
            this.Items.Add(
                new TListItem
                {
                    Value = result,
                    DisplayValue = DisplayValueProducer?.GetDisplayValue(result)
                });
        }

        public void ClearList()
        {
            this.Items.Clear();
        }
    }
}