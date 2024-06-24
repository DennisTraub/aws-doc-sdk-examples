// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved. 
// SPDX-License-Identifier: Apache-2.0

using FluentAssertions;
using Scenarios;
using Xunit;

namespace BedrockRuntimeTests;

[Trait("Category", "Integration")]
public class ScenarioTests_AdditionalRequestAndResponseFields
{
    [Fact]
    public async Task ConverseShouldContainAdditionalResponseFields()
    {
        var response = await AdditionalRequestAndResponseFields.Converse();

        response.Output.Message.Role.Should().Be("assistant");

        response.Output.Message.Content.Count.Should().Be(1);
        response.Output.Message.Content.First().Text.Should().NotBeEmpty();

        response.AdditionalModelResponseFields.AsDictionary()?["usage"].Should().NotBeNull();
    }

    [Fact]
    public async Task ConverseStreamShouldContainAdditionalResponseFields()
    {
        var response = await AdditionalRequestAndResponseFields.ConverseStream();

        response.Output.Message.Role.Should().Be("assistant");
        
        response.Output.Message.Content.Count.Should().Be(1);
        response.Output.Message.Content.First().Text.Should().NotBeEmpty();
        
        response.AdditionalModelResponseFields.AsDictionary()?["usage"].Should().NotBeNull();
    }
}