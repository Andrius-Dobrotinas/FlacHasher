namespace Andy.FlacHash.Application.Cmd.E2E
{
    static class SampleAsset
    {
        public const string Directory = "TestAssets";

        // A sample's FLAC and APE renditions encode the same audio, so they share one expected hash.
        public static class Sample1
        {
            public const string ExpectedMd5 = "50fbd2fb80a146bb23ae362c8aa40b8b";
            public const string ExpectedSha256 = "59aefcf88c06c9b1a08ee553ed4235a29c73a2e72ffb3ecf2ded8731cb1a5e8d";

            public static class Flac
            {
                public const string FileName = "sample.flac";
            }

            public static class Ape
            {
                public const string FileName = "sample.ape";
            }
        }

        public static class Sample2
        {
            public const string ExpectedMd5 = "0bead2fbd3459921c6138de5aab4efc8";
            public const string ExpectedSha256 = "ca254fb44648dfd355aecbd96b007e987564331d4849722ce57dd4634b0ccd96";

            public static class Flac
            {
                public const string FileName = "sample2-800.flac";
            }

            public static class Ape
            {
                public const string FileName = "sample2-800.ape";
            }
        }
    }
}
