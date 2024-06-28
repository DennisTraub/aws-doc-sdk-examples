// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved. 
// SPDX-License-Identifier: Apache-2.0

using Xunit;

namespace BedrockRuntimeTests;

[Trait("Category", "Integration")]
public class ScenarioTests_Multimodal
{
    // [Fact]
    // public async Task ConverseImageScenarioReturnsSingleTextBlock()
    // {
    //     var response = await MultimodalWithImage.Converse();
    //     response.Output.Message.Role.Should().Be("assistant");
    //     response.Output.Message.Content.Count.Should().Be(1);
    //     response.Output.Message.Content.First().Text.Should().NotBeEmpty();
    // }
    //
    // [Fact]
    // public async Task ConverseStreamImageScenarioReturnsHttpOk()
    // {
    //     var response = await MultimodalWithImage.ConverseStream();
    //     response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
    // }
}