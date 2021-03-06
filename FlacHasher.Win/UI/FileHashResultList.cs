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
        public FileHashResultList()
        {
            DisplayMember = nameof(FileHashResultListItem.HashString);
        }
    }
}