using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win.UI
{
    public class HashFaceValueFactory : IFaceValueFactory<FileHashResult>
    {
        private readonly string format;

        public HashFaceValueFactory(string format)
        {
            if (string.IsNullOrEmpty(format)) throw new ArgumentNullException(nameof(format));

            this.format = format;
        }

        public string GetFaceValue(FileHashResult result)
        {
            return Cmd.OutputFormatter.GetFormattedString(format, result.Hash, result.File);
        }
    }
}