using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win.UI
{
    public class HashDisplayValueFactory : IDisplayValueProducer<FileHashResult>
    {
        private readonly IHashFormatter hashFormatter;

        public HashDisplayValueFactory(IHashFormatter hashFormatter)
        {
            this.hashFormatter = hashFormatter;
        }

        public string GetDisplayValue(FileHashResult result)
        {
            return hashFormatter.GetString(result.Hash);
        }
    }
}