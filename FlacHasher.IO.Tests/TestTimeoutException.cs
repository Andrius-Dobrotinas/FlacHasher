using NUnit.Framework;
using System;

namespace Andy.FlacHash.IO
{
    class TestTimeoutException : AssertionException
    {
        public TestTimeoutException() : base("Test timed out")
        {
        }
    }
}