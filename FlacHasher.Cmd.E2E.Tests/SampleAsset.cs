namespace Andy.FlacHash.Application.Cmd.E2E
{
    static class SampleAsset
    {
        public const string Directory = "TestAssets";

        public static class Sample1
        {
            public static class Flac
            {
                public const string FileName = "sample.flac";
                public const string ExpectedMd5 = "50fbd2fb80a146bb23ae362c8aa40b8b";
            }

            public static class Ape
            {
                public const string FileName = "sample.ape";
                public const string ExpectedMd5 = "50fbd2fb80a146bb23ae362c8aa40b8b";
            }
        }

        public static class Sample2
        {
            public static class Flac
            {
                public const string FileName = "sample2-800.flac";
                public const string ExpectedMd5 = "0bead2fbd3459921c6138de5aab4efc8";
            }

            public static class Ape
            {
                public const string FileName = "sample2-800.ape";
                public const string ExpectedMd5 = "0bead2fbd3459921c6138de5aab4efc8";
            }
        }
    }
}
