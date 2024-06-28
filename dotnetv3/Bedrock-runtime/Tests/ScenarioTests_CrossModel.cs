// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using CrossModelScenarios;
using Xunit;

namespace BedrockRuntimeTests;

public class ScenarioTests_CrossModel
{
    [Fact, Trait("Category", "Integration")]
    public void DocumentUploadDoesNotThrow()
    {
        Type type = typeof(DocumentUpload);
        var entryPoint = type.Assembly.EntryPoint!;
        var exception = Record.Exception(() => entryPoint.Invoke(null, [Array.Empty<string>()]));
        Assert.Null(exception);
    }
}