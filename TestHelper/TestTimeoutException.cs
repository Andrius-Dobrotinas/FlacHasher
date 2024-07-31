using NUnit.Framework;
using System;

namespace Andy
{
    class TestTimeoutException : AssertionException
    {
        public TestTimeoutException() : base("Test timed out")
        {
        }
    }
}