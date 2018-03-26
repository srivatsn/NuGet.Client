// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            var config = $@"<?xml version='1.0' encoding='utf-8'?>
<configuration>
    <packageSources>
        <add key='nuget.org' value='https://nuget.org' />
        <add key='test.org' value='Packages' />
    </packageSources>
    <trustedSources>
        <nuget.org>
            <add key='HASH' value='SUBJECT_NAME' fingerprintAlgorithm='SHA256' />
        </nuget.org>
    </trustedSources>
    <config>
        <add key='signatureValidationMode' value='{policyName}' />
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
            var config = $@"<?xml version='1.0' encoding='utf-8'?>
<configuration>
    <packageSources>
        <add key='nuget.org' value='https://nuget.org' />
        <add key='test.org' value='Packages' />
    </packageSources>
    <trustedSources>
        <nuget.org>
            <add key='HASH' value='SUBJECT_NAME' fingerprintAlgorithm='SHA256' />
        </nuget.org>
    </trustedSources>
    <config>
        <add key='signatureValidationMode' value='{policyName}' />
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
            var config = @"<?xml version='1.0' encoding='utf-8'?>
<configuration>
    <packageSources>
        <add key='nuget.org' value='https://nuget.org' />
        <add key='test.org' value='Packages' />
    </packageSources>
    <trustedSources>
        <nuget.org>
            <add key='HASH' value='SUBJECT_NAME' fingerprintAlgorithm='SHA256' />
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
    }
}
