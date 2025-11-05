using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Application.Win
{
    [Serializable]
    public class DecoderProfileList
    {
        private DecoderProfile[] profiles;

        public DecoderProfile[] Profiles
        {
            get { return profiles; }
            set { profiles = value; }
        }
    }
}