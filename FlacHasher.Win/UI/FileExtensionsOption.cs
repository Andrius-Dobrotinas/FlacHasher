using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application.Win.UI
{
    class FileExtensionsOption
    {
        public FileExtensionsOption(string Text, string[] Extensions)
        {
            this.Name = Text;
            this.Extensions = Extensions;
        }
        public string Name { get; }
        public string[] Extensions { get; }
    }
}