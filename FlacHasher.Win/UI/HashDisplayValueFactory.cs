using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win.UI
{
    public class HashDisplayValueFactory : IDisplayValueProducer<FileHashResult>
    {
        private readonly string format;

        public HashDisplayValueFactory(string format)
        {
            if (string.IsNullOrEmpty(format)) throw new ArgumentNullException(nameof(format));

            this.format = format;
        }

        public string GetDisplayValue(FileHashResult result)
        {
            return Cmd.OutputFormatter.GetFormattedString(format, result.Hash, result.File);
        }
    }
}