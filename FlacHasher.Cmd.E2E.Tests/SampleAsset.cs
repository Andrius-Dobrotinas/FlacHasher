namespace Andy.FlacHash.Application.Cmd.E2E
{
    static class SampleAsset
    {
        public const string Directory = "TestAssets";

        public static class Flac1
        {
            public const string FileName = "sample.flac";

            // Pinned from the decoded output of TestAssets/sample.flac. See TestAssets/make-test-assets.ps1.
            public const string ExpectedMd5 = "770ec9cbf2ff85a82670e10d807d82d1";
        }
    }
}
