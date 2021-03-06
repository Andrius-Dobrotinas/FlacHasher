using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win.UI
{
    public class FileHashResultListItem
    {
        public FileHashResult Value { get; set; }
        public string HashString { get; set; }
    }

    public class FileHashResultList : TypedListBox<FileHashResultListItem>
    {
        /// <summary>
        /// Since the object is created by the Winforms Designer, this method makes sure the desired default values are used
        /// </summary>
        public void Initialize()
        {
            DisplayMember = nameof(FileHashResultListItem.HashString);
        }
    }
}