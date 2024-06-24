// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// snippet-start:[BedrockRuntime.dotnetv3.Converse_Multimodal_AnthropicClaude]
// This example demonstrates how to use Amazon Bedrock's Converse API to send a
// multimodal message (text and image) to Anthropic Claude and process the response.

using System;
using System.IO;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;

// The location of the image to send.
const string filePath = "Resources/image.png";

// The model ID, e.g., Claude 3 Haiku.
const string modelId = "anthropic.claude-3-haiku-20240307-v1:0";

// Create a Bedrock Runtime client in the AWS Region you want to use.
var client = new AmazonBedrockRuntimeClient(RegionEndpoint.USEast1);

try
{
    // Load the image file into a byte stream.
    using var imageBytes = new MemoryStream(await File.ReadAllBytesAsync(filePath));

    // Create a multimodal message with a text prompt and the image.
    var textPrompt = "Describe this image.";
    var multimodalMessage = new Message
    {
        Role = ConversationRole.User,
        Content =
        [
            new ContentBlock { Text = textPrompt },
            new ContentBlock
            {
                Image = new ImageBlock
                {
                    Format = ImageFormat.Png,
                    Source = new ImageSource { Bytes = imageBytes }
                }
            }
        ]
    };

    // Send the multimodal message to the model using the Converse API.
    var response = await client.ConverseAsync(new ConverseRequest
    {
        ModelId = modelId, Messages = [multimodalMessage]
    });

    // Extract and display the model's response.
    string responseText = response?.Output?.Message?.Content?[0]?.Text ?? "";
    Console.WriteLine(responseText);
    
}
catch (IOException e)
{
    Console.WriteLine($"Error loading image from {filePath}: {e.Message}");
}
catch (AmazonBedrockRuntimeException e)
{
    Console.WriteLine($"Error invoking model '{modelId}': {e.Message}");
}

// snippet-end:[BedrockRuntime.dotnetv3.Converse_Multimodal_AnthropicClaude]

// Create a partial class to make the top-level script testable.
namespace AnthropicClaude { public class ConverseMultimodal; }
