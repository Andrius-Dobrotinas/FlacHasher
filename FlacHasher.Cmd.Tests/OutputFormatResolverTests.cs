using NUnit.Framework;

namespace Andy.FlacHash.Cmd
{
    public class OutputFormatResolverTests
    {
        [TestCase("Cmdline value", "Settings value")]
        [TestCase("Cmdline value", "")]
        [TestCase("Cmdline value", null)]
        public void CmdlineValue_MustTakePrecedenceOverSettingsValue(string cmdLineArgValue, string settingsValue)
        {
            var settings = new Settings { OutputFormat = settingsValue };
            var cmdLineArguments = new Parameters { OutputFormat = cmdLineArgValue };

            var result = OutputFormatResolver.GetOutputFormat(settings, cmdLineArguments);

            Assert.AreEqual(cmdLineArgValue, result);
        }

        [TestCase("", "Settings value")]
        [TestCase("", "")]
        [TestCase("", null)]
        public void When_CmdlineValue_IsEmptyString__Must_Return_Null__AsThatMeansTheUserWantsNoFormatting(
            string cmdLineArgValue,
            string settingsValue)
        {
            var settings = new Settings { OutputFormat = settingsValue };
            var cmdLineArguments = new Parameters { OutputFormat = cmdLineArgValue };

            var result = OutputFormatResolver.GetOutputFormat(settings, cmdLineArguments);

            Assert.AreEqual(null, result);
        }

        [Test]
        public void When_CmdlineValue_IsNull__Must_Return_SettingsValue()
        {
            var settingsValue = "Settings value";
            var settings = new Settings { OutputFormat = settingsValue };
            var cmdLineArguments = new Parameters { OutputFormat = null };

            var result = OutputFormatResolver.GetOutputFormat(settings, cmdLineArguments);

            Assert.AreEqual(settingsValue, result);
        }

        [Test]
        public void When_SettingsValue_IsEmptyString__Must_Return_Null__AsThatMeansTheUserWantsNoFormatting()
        {
            var settings = new Settings { OutputFormat = null };
            var cmdLineArguments = new Parameters { OutputFormat = "" };

            var result = OutputFormatResolver.GetOutputFormat(settings, cmdLineArguments);

            Assert.AreEqual(null, result);
        }
    }
}