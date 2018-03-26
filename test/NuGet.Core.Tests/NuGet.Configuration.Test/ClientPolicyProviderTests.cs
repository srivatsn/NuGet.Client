// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.Configuration.Test
{
    public class ClientPolicyProviderTests
    {
        [Theory]
        [InlineData("accept", ClientPolicy.Accept)]
        [InlineData("require", ClientPolicy.Require)]
        public void LoadsClientPolicyWhenPresent(string policyName, ClientPolicy expectedPolicy)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""{policyName}"" />
  </config>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config);
                var settings = new Settings(mockBaseDirectory);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                var clientPolicy = clientPolicyProvider.LoadClientPolicy();

                // Assert
                clientPolicy.Should().Be(expectedPolicy);
            }
        }

        [Theory]
        [InlineData("ACCEPT", ClientPolicy.Accept)]
        [InlineData("REQUIRE", ClientPolicy.Require)]
        public void LoadsClientPolicyWhenPresent_IgnoresCase(string policyName, ClientPolicy expectedPolicy)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""{policyName}"" />
  </config>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config);
                var settings = new Settings(mockBaseDirectory);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                var clientPolicy = clientPolicyProvider.LoadClientPolicy();

                // Assert
                clientPolicy.Should().Be(expectedPolicy);
            }
        }

        [Fact]
        public void LoadsDefaultClientPolicyIfNone()
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
</configuration>";

            var expectedValue = ClientPolicy.Accept;

            using (var mockBaseDirectory = TestDirectory.Create())
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config);
                var settings = new Settings(mockBaseDirectory);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                var clientPolicy = clientPolicyProvider.LoadClientPolicy();

                // Assert
                clientPolicy.Should().Be(expectedValue);
            }
        }

        [Theory]
        [InlineData("accept", ClientPolicy.Accept)]
        [InlineData("require", ClientPolicy.Require)]
        public void LoadsClientPolicyFromNestedSettingsWhenPresent(string policyName, ClientPolicy expectedPolicy)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config1 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""{policyName}"" />
  </config>
</configuration>";

            var config2 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            using (var mockChildDirectory = TestDirectory.Create(mockBaseDirectory))
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config1);
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockChildDirectory, config2);

                var configPaths = new List<string> { Path.Combine(mockChildDirectory, nugetConfigPath), Path.Combine(mockBaseDirectory, nugetConfigPath) };
                var settings = Settings.LoadSettingsGivenConfigPaths(configPaths);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                var clientPolicy = clientPolicyProvider.LoadClientPolicy();

                // Assert
                clientPolicy.Should().Be(expectedPolicy);
            }
        }

        [Theory]
        [InlineData("accept", ClientPolicy.Accept)]
        [InlineData("require", ClientPolicy.Require)]
        public void LoadsAndDupesClientPolicyFromNestedSettingsWhenPresent(string policyName, ClientPolicy expectedPolicy)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config1 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""accept"" />
  </config>
</configuration>";

            var config2 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""{policyName}"" />
  </config>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            using (var mockChildDirectory = TestDirectory.Create(mockBaseDirectory))
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config1);
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockChildDirectory, config2);

                var configPaths = new List<string> { Path.Combine(mockChildDirectory, nugetConfigPath), Path.Combine(mockBaseDirectory, nugetConfigPath) };
                var settings = Settings.LoadSettingsGivenConfigPaths(configPaths);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                var clientPolicy = clientPolicyProvider.LoadClientPolicy();

                // Assert
                clientPolicy.Should().Be(expectedPolicy);
            }
        }

        [Fact]
        public void LoadsDefaultClientPolicyFromNestedSettingsIfNone()
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
</configuration>";

            var config2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
</configuration>";

            var expectedValue = ClientPolicy.Accept;

            using (var mockBaseDirectory = TestDirectory.Create())
            using (var mockChildDirectory = TestDirectory.Create(mockBaseDirectory))
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config1);
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockChildDirectory, config2);

                var configPaths = new List<string> { Path.Combine(mockChildDirectory, nugetConfigPath), Path.Combine(mockBaseDirectory, nugetConfigPath) };
                var settings = Settings.LoadSettingsGivenConfigPaths(configPaths);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                var clientPolicy = clientPolicyProvider.LoadClientPolicy();

                // Assert
                clientPolicy.Should().Be(expectedValue);
            }
        }

        [Theory]
        [InlineData(ClientPolicy.Accept, "Accept")]
        [InlineData(ClientPolicy.Require, "Require")]
        public void WritesClientPolicyIfNonePresent(ClientPolicy policy, string policyName)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config);
                var settings = new Settings(mockBaseDirectory);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                clientPolicyProvider.SaveClientPolicy(policy);

                // Assert
                var result1 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""{policyName}"" />
  </config>
</configuration>";

                Assert.Equal(result1.Replace("\r\n", "\n"),
                    File.ReadAllText(Path.Combine(mockBaseDirectory, nugetConfigPath)).Replace("\r\n", "\n"));
            }
        }

        [Theory]
        [InlineData(ClientPolicy.Accept, "Accept")]
        [InlineData(ClientPolicy.Require, "Require")]
        public void WritesAndUpdatesClientPolicy(ClientPolicy policy, string policyName)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""require"" />
  </config>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config);
                var settings = new Settings(mockBaseDirectory);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                clientPolicyProvider.SaveClientPolicy(policy);

                // Assert
                var result1 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""{policyName}"" />
  </config>
</configuration>";

                Assert.Equal(result1.Replace("\r\n", "\n"),
                    File.ReadAllText(Path.Combine(mockBaseDirectory, nugetConfigPath)).Replace("\r\n", "\n"));
            }
        }

        [Theory]
        [InlineData(ClientPolicy.Accept, "Accept")]
        [InlineData(ClientPolicy.Require, "Require")]
        public void WritesClientPolicyIntoNestedSettings(ClientPolicy policy, string policyName)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
</configuration>";

            var config2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            using (var mockChildDirectory = TestDirectory.Create(mockBaseDirectory))
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config1);
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockChildDirectory, config2);

                var configPaths = new List<string> { Path.Combine(mockChildDirectory, nugetConfigPath), Path.Combine(mockBaseDirectory, nugetConfigPath) };
                var settings = Settings.LoadSettingsGivenConfigPaths(configPaths);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                clientPolicyProvider.SaveClientPolicy(policy);

                // Assert
                var result1 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""{policyName}"" />
  </config>
</configuration>";

                var result2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
</configuration>";

                Assert.Equal(result1.Replace("\r\n", "\n"),
                    File.ReadAllText(Path.Combine(mockBaseDirectory, nugetConfigPath)).Replace("\r\n", "\n"));
                Assert.Equal(result2.Replace("\r\n", "\n"),
                    File.ReadAllText(Path.Combine(mockChildDirectory, nugetConfigPath)).Replace("\r\n", "\n"));
            }
        }

        [Theory]
        [InlineData(ClientPolicy.Accept, "Accept")]
        [InlineData(ClientPolicy.Require, "Require")]
        public void WritesAndUpdatesClientPolicyIntoNestedSettings(ClientPolicy policy, string policyName)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""accept"" />
  </config>
</configuration>";

            var config2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            using (var mockChildDirectory = TestDirectory.Create(mockBaseDirectory))
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config1);
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockChildDirectory, config2);

                var configPaths = new List<string> { Path.Combine(mockChildDirectory, nugetConfigPath), Path.Combine(mockBaseDirectory, nugetConfigPath) };
                var settings = Settings.LoadSettingsGivenConfigPaths(configPaths);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                clientPolicyProvider.SaveClientPolicy(policy);

                // Assert
                var result1 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""{policyName}"" />
  </config>
</configuration>";

                var result2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
</configuration>";

                Assert.Equal(result1.Replace("\r\n", "\n"),
                    File.ReadAllText(Path.Combine(mockBaseDirectory, nugetConfigPath)).Replace("\r\n", "\n"));
                Assert.Equal(result2.Replace("\r\n", "\n"),
                    File.ReadAllText(Path.Combine(mockChildDirectory, nugetConfigPath)).Replace("\r\n", "\n"));
            }
        }

        [Theory]
        [InlineData(ClientPolicy.Accept)]
        [InlineData(ClientPolicy.Require)]
        public void SavesClientPolicyIfNonePresent(ClientPolicy expectedpolicy)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config);
                var settings = new Settings(mockBaseDirectory);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                clientPolicyProvider.SaveClientPolicy(expectedpolicy);

                // Assert
                clientPolicyProvider.LoadClientPolicy().Should().Be(expectedpolicy);
            }
        }

        [Theory]
        [InlineData(ClientPolicy.Accept)]
        [InlineData(ClientPolicy.Require)]
        public void SavesAndUpdatesClientPolicy(ClientPolicy expectedpolicy)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""require"" />
  </config>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config);
                var settings = new Settings(mockBaseDirectory);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                clientPolicyProvider.SaveClientPolicy(expectedpolicy);

                // Assert
                clientPolicyProvider.LoadClientPolicy().Should().Be(expectedpolicy);
            }
        }

        [Theory]
        [InlineData(ClientPolicy.Accept)]
        [InlineData(ClientPolicy.Require)]
        public void SavesClientPolicyIntoNestedSettings(ClientPolicy expectedpolicy)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
</configuration>";

            var config2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            using (var mockChildDirectory = TestDirectory.Create(mockBaseDirectory))
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config1);
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockChildDirectory, config2);

                var configPaths = new List<string> { Path.Combine(mockChildDirectory, nugetConfigPath), Path.Combine(mockBaseDirectory, nugetConfigPath) };
                var settings = Settings.LoadSettingsGivenConfigPaths(configPaths);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                clientPolicyProvider.SaveClientPolicy(expectedpolicy);

                // Assert
                clientPolicyProvider.LoadClientPolicy().Should().Be(expectedpolicy);
            }
        }

        [Theory]
        [InlineData(ClientPolicy.Accept)]
        [InlineData(ClientPolicy.Require)]
        public void SavesAndUpdatesClientPolicyIntoNestedSettings(ClientPolicy expectedpolicy)
        {
            // Arrange
            var nugetConfigPath = "NuGet.Config";
            var config1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://nuget.org"" />
    <add key=""test.org"" value=""Packages"" />
  </packageSources>
  <trustedSources>
    <nuget.org>
      <add key=""HASH"" value=""SUBJECT_NAME"" fingerprintAlgorithm=""SHA256"" />
    </nuget.org>
  </trustedSources>
  <config>
    <add key=""signatureValidationMode"" value=""accept"" />
  </config>
</configuration>";

            var config2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
</configuration>";

            using (var mockBaseDirectory = TestDirectory.Create())
            using (var mockChildDirectory = TestDirectory.Create(mockBaseDirectory))
            {
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockBaseDirectory, config1);
                ConfigurationFileTestUtility.CreateConfigurationFile(nugetConfigPath, mockChildDirectory, config2);

                var configPaths = new List<string> { Path.Combine(mockChildDirectory, nugetConfigPath), Path.Combine(mockBaseDirectory, nugetConfigPath) };
                var settings = Settings.LoadSettingsGivenConfigPaths(configPaths);
                var clientPolicyProvider = new ClientPolicyProvider(settings);

                // Act
                clientPolicyProvider.SaveClientPolicy(expectedpolicy);

                // Assert
                clientPolicyProvider.LoadClientPolicy().Should().Be(expectedpolicy);
            }
        }
    }
}