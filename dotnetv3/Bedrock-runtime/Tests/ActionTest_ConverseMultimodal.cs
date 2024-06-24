// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved. 
// SPDX-License-Identifier: Apache-2.0

namespace BedrockRuntimeTests;

public class ActionTest_ConverseMultimodal
{
    [Theory, Trait("Category", "Integration")]
    [InlineData(typeof(AnthropicClaude.Converse))]
    public void ConverseMultimodalDoesNotThrow(Type type)
    {
        var entryPoint = type.Assembly.EntryPoint!;
        var exception = Record.Exception(() => entryPoint.Invoke(null, [Array.Empty<string>()]));
        Assert.Null(exception);
    }
}