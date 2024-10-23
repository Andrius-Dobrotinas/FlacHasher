using Andy.Configuration.Ini;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Application
{
    public class SettingsFile_Tests
    {
        Mock<IIniFileReader> iniReader;

        public SettingsFile_Tests()
        {
            iniReader = new Mock<IIniFileReader>();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("asd")]
        public void When_IniFile_IsEmpty__Must__Return_Empty(string profileName)
        {
            var ini = new Dictionary<string, IDictionary<string, string>>();

            var result = SettingsFile.GetSettingsProfile(ini, profileName);

            Assert.IsEmpty(result);
        }


        [TestCaseSource(nameof(GetCases_ReturnRootSection))]
        public void When_ContainsRootSection_RepresentedByEmptyStringKey_And_TargetProfile_NotSpecified__Must__Return_RootSection(IDictionary<string, IDictionary<string, string>> ini, IDictionary<string, string> expectedSection)
        {
            var result = SettingsFile.GetSettingsProfile(ini, null);

            AssertThat.CollectionsMatchExactly(result.OrderBy(x => x.Key), expectedSection.OrderBy(x => x.Key));
        }

        [TestCaseSource(nameof(GetCases_ReturnRootSection))]
        public void When_ContainsRootSection_RepresentedByEmptyStringKey_And_TargetProfile_IsEmptyString__Must__Return_RootSection(IDictionary<string, IDictionary<string, string>> ini, IDictionary<string, string> expectedSection)
        {
            var result = SettingsFile.GetSettingsProfile(ini, "");

            AssertThat.CollectionsMatchExactly(result.OrderBy(x => x.Key), expectedSection.OrderBy(x => x.Key));
        }

        [Test]
        public void When_ContainsRootSection_And_NonExistent_TargetProfile_IsSpecified__Must__Reject()
        {
            var section = new Dictionary<string, string>
            {
                { "key0", "value2" },
                { "key 3", "value 3" },
            };

            var ini = new Dictionary<string, IDictionary<string, string>>
            {
                { "", section }
            };

            Assert.Throws<ConfigurationException>(() => SettingsFile.GetSettingsProfile(ini, "whatevs"));
        }

        [TestCaseSource(nameof(GetCases_Override))]
        public void When_ContainsRootSection_And_TargetProfile_IsSpecified__Must__Return_RootSection_With_ValuesOverriddenByTargetProfile(IDictionary<string, IDictionary<string, string>> ini, string targetProfile, IDictionary<string, string> expectedSection)
        {
            var result = SettingsFile.GetSettingsProfile(ini, targetProfile);

            AssertThat.CollectionsMatchExactly(result.OrderBy(x => x.Key), expectedSection.OrderBy(x => x.Key));
        }

        [TestCaseSource(nameof(GetCases_Override_ProfileSpecifiedInConfig))]
        public void When_ContainsRootSection_And_TargetProfile_NotSpecified_And_SectionSpecifiesProfile__Must__Return_RootSection_With_ValuesOverriddenByTargetProfile(IDictionary<string, IDictionary<string, string>> ini, IDictionary<string, string> expectedSection)
        {
            var result = SettingsFile.GetSettingsProfile(ini, null);

            AssertThat.CollectionsMatchExactly(result.OrderBy(x => x.Key), expectedSection.OrderBy(x => x.Key));
        }

        [TestCaseSource(nameof(GetCases_Override_ProfileSpecifiedInConfigAndFunctionParam))]
        public void When_ContainsRootSection_And_TargetProfile_IsSpecified_And_SectionSpecifiesProfile__Must__UseFunctionParameterProfile(IDictionary<string, IDictionary<string, string>> ini, string profile, IDictionary<string, string> expectedSection)
        {
            var result = SettingsFile.GetSettingsProfile(ini, profile);

            AssertThat.CollectionsMatchExactly(result.OrderBy(x => x.Key), expectedSection.OrderBy(x => x.Key));
        }

        [Test]
        public void When_ContainsRootSection_And_TargetProfile_IsEmptyString_And_SectionSpecifiesProfile__Must__Reset_To_RootSection()
        {
            var section1 = new Dictionary<string, string>
            {
                { "key0", "D new" }
            };
            var section2 = new Dictionary<string, string>
            {
                { "key3", "value changed" },
                { "key-new", "another value" }
            };

            var ini = new Dictionary<string, IDictionary<string, string>>
                {
                    { "", new Dictionary<string, string>
                        {
                            { "Profile", "first" },
                            { "key0", "initial D" },
                            { "key3", "value 3" },
                        }
                    },
                    { "first", section1 },
                    { "second", section2 },
                };

            var expected = new Dictionary<string, string>
                {
                    { "Profile", "first" },
                    { "key0", "initial D" },
                    { "key3", "value 3" },
                };
            var result = SettingsFile.GetSettingsProfile(ini, "");

            AssertThat.CollectionsMatchExactly(result.OrderBy(x => x.Key), expected.OrderBy(x => x.Key));
        }

        [TestCase(null)]
        [TestCase("")]
        public void When_HasNoRootSection_And_TargetProfile_NotSpecified__Must__Reject(string profileName)
        {
            var section = new Dictionary<string, string>
            {
                { "key0", "value2" },
                { "key 3", "value 3" },
            };

            var ini = new Dictionary<string, IDictionary<string, string>>
            {
                { "first", section }
            };

            Assert.Throws<ConfigurationException>(() => SettingsFile.GetSettingsProfile(ini, profileName));
        }

        [TestCase("profile1")]
        [TestCase("")]
        public void When_HasNoRootSection_And_NonExistent_TargetProfile_IsSpecified__Must__Reject(string profileName)
        {
            var section = new Dictionary<string, string>
            {
                { "key0", "value2" },
                { "key 3", "value 3" },
            };

            var ini = new Dictionary<string, IDictionary<string, string>>
            {
                { "first", section }
            };

            Assert.Throws<ConfigurationException>(() => SettingsFile.GetSettingsProfile(ini, profileName));
        }

        [TestCase("first")]
        [TestCase("second")]
        public void When_HasNoRootSection_And_TargetProfile_IsSpecified__Must__Return_TargetProfileSection(string profileName)
        {
            var section1 = new Dictionary<string, string>
            {
                { "key0", "profile1 val0" },
                { "key3", "profile1 val3" },
            };

            var section2 = new Dictionary<string, string>
            {
                { "key0", "profile2 val0" },
                { "key2", "profile2 val2" },
            };

            var ini = new Dictionary<string, IDictionary<string, string>>
            {
                { "first", section1 },
                { "second", section2 }
            };

            var expected = ini[profileName];
            var result = SettingsFile.GetSettingsProfile(ini, profileName);
            AssertThat.CollectionsMatchExactly(result.OrderBy(x => x.Key), expected.OrderBy(x => x.Key));
        }

        static IEnumerable<TestCaseData> GetCases_ReturnRootSection()
        {
            var section1 = new Dictionary<string, string>
            {
                { "key1", "value1" }
            };
            var section2 = new Dictionary<string, string>
            {
                { "key0", "value2" }
            };
            var section3 = new Dictionary<string, string>
            {
                { "key0", "value2" },
                { "key 3", "value 3" },
            };

            yield return new TestCaseData(
                new Dictionary<string, IDictionary<string, string>>
                {
                    { "", section1 },
                    { "first", section2 },
                },
                section1);

            yield return new TestCaseData(
                new Dictionary<string, IDictionary<string, string>>
                {
                    { "first", section1 },
                    { "", section2 },
                },
                section2);

            yield return new TestCaseData(
                new Dictionary<string, IDictionary<string, string>>
                {
                    { "first", section1 },
                    { "", section2 },
                    { "0",  section3 }
                },
                section2);
        }

        static IEnumerable<TestCaseData> GetCases_Override()
        {
            var sectionRoot = new Dictionary<string, string>
            {
                { "key0", "initial D" },
                { "key3", "value 3" },
            };
            var section1 = new Dictionary<string, string>
            {
                { "key0", "D new" }
            };
            var section2 = new Dictionary<string, string>
            {
                { "key3", "value changed" },
                { "key-new", "another value" }
            };

            yield return new TestCaseData(
                new Dictionary<string, IDictionary<string, string>>
                {
                    { "", sectionRoot.ToDictionary(x => x.Key, x => x.Value) },
                    { "first", section1 },
                    { "second", section2 },
                },
                "first",
                new Dictionary<string, string>
                {
                    { "key0", "D new" },
                    { "key3", "value 3" },
                });

            yield return new TestCaseData(
                new Dictionary<string, IDictionary<string, string>>
                {
                    { "", sectionRoot.ToDictionary(x => x.Key, x => x.Value) },
                    { "first", section1 },
                    { "second", section2 },
                },
                "second",
                new Dictionary<string, string>
                {
                    { "key0", "initial D" },
                    { "key3", "value changed" },
                    { "key-new", "another value" }
                });
        }

        static IEnumerable<TestCaseData> GetCases_Override_ProfileSpecifiedInConfig()
        {
            var section1 = new Dictionary<string, string>
            {
                { "key0", "D new" }
            };
            var section2 = new Dictionary<string, string>
            {
                { "key3", "value changed" },
                { "key-new", "another value" }
            };

            yield return new TestCaseData(
                new Dictionary<string, IDictionary<string, string>>
                {
                    { "", new Dictionary<string, string>
                        {
                            { "Profile", "first" },
                            { "key0", "initial D" },
                            { "key3", "value 3" },
                        }
                    },
                    { "first", section1 },
                    { "second", section2 },
                },
                new Dictionary<string, string>
                {
                    { "Profile", "first" },
                    { "key0", "D new" },
                    { "key3", "value 3" },
                });

            yield return new TestCaseData(
                new Dictionary<string, IDictionary<string, string>>
                {
                    { "", new Dictionary<string, string>
                        {
                            { "Profile", "second" },
                            { "key0", "initial D" },
                            { "key3", "value 3" },
                        }
                    },
                    { "first", section1 },
                    { "second", section2 },
                },
                new Dictionary<string, string>
                {
                    { "Profile", "second" },
                    { "key0", "initial D" },
                    { "key3", "value changed" },
                    { "key-new", "another value" }
                });
        }

        static IEnumerable<TestCaseData> GetCases_Override_ProfileSpecifiedInConfigAndFunctionParam()
        {
            var section1 = new Dictionary<string, string>
            {
                { "key0", "D new" }
            };
            var section2 = new Dictionary<string, string>
            {
                { "key3", "value changed" },
                { "key-new", "another value" }
            };

            yield return new TestCaseData(
                new Dictionary<string, IDictionary<string, string>>
                {
                    { "", new Dictionary<string, string>
                        {
                            { "Profile", "first" },
                            { "key0", "initial D" },
                            { "key3", "value 3" },
                        }
                    },
                    { "first", section1 },
                    { "second", section2 },
                },
                "second",
                new Dictionary<string, string>
                {
                    { "Profile", "first" },
                    { "key0", "initial D" },
                    { "key3", "value changed" },
                    { "key-new", "another value" }
                });

            yield return new TestCaseData(
                new Dictionary<string, IDictionary<string, string>>
                {
                    { "", new Dictionary<string, string>
                        {
                            { "Profile", "second" },
                            { "key0", "initial D" },
                            { "key3", "value 3" },
                        }
                    },
                    { "first", section1 },
                    { "second", section2 },
                },
                "first",
                new Dictionary<string, string>
                {
                    { "Profile", "second" },
                    { "key0", "D new" },
                    { "key3", "value 3" },
                });
        }
    }
}