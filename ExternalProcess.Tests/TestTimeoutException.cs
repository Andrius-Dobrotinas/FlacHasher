using NUnit.Framework;
using System;

namespace Andy.ExternalProcess
{
    class TestTimeoutException : AssertionException
    {
        public TestTimeoutException() : base("Test timed out")
        {
        }
    }
}