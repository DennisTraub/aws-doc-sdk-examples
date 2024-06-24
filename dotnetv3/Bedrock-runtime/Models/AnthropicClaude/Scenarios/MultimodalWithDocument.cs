// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using System.Linq;

namespace Scenarios;

public static class MultimodalWithDocument
{
    public static async Task<ConverseResponse?> Converse()
    {
        // The location of the document to send.
        const string filePath = "ScenarioResources/document.txt";

        // The model ID, e.g., Claude 3 Haiku.
        const string modelId = "anthropic.claude-3-haiku-20240307-v1:0";

        // Create a Bedrock Runtime client in the AWS Region you want to use.
        var client = new AmazonBedrockRuntimeClient(RegionEndpoint.USEast1);

        try
        {
            // Load the image file into a byte stream.
            using var documentBytes = new MemoryStream(await File.ReadAllBytesAsync(filePath));

            // Create a multimodal message with a text prompt and the document.
            var textPrompt = "Create a one-paragraph summary of this document.";
            var multimodalMessage = new Message
            {
                Role = ConversationRole.User,
                Content =
                [
                    new ContentBlock { Text = textPrompt },
                    new ContentBlock
                    {
                        Document = new DocumentBlock
                        {
                            Name = "Document",
                            Format = DocumentFormat.Txt,
                            Source = new DocumentSource { Bytes = documentBytes }
                        }
                    }
                ]
            };

            // Send the multimodal message to the model using the Converse API.
            var response = await client.ConverseAsync(new ConverseRequest
            {
                ModelId = modelId,
                Messages = [multimodalMessage]
            });

            // Extract and display the model's response.
            var responseText = response?.Output?.Message?.Content?[0]?.Text ?? "";
            Console.WriteLine(responseText);

            return response;

        }
        catch (IOException e)
        {
            Console.WriteLine($"Error loading image from {filePath}: {e.Message}");
            throw;
        }
        catch (AmazonBedrockRuntimeException e)
        {
            Console.WriteLine($"Error invoking model '{modelId}': {e.Message}");
            throw;
        }
    }

    public static async Task<ConverseStreamResponse> ConverseStream()
    {
        // The location of the document to send.
        const string filePath = "ScenarioResources/document.txt";

        // The model ID, e.g., Claude 3 Haiku.
        const string modelId = "anthropic.claude-3-haiku-20240307-v1:0";

        // Create a Bedrock Runtime client in the AWS Region you want to use.
        var client = new AmazonBedrockRuntimeClient(RegionEndpoint.USEast1);

        try
        {
            // Load the image file into a byte stream.
            using var documentBytes = new MemoryStream(await File.ReadAllBytesAsync(filePath));

            // Create a multimodal message with a text prompt and the document.
            var textPrompt = "Create a one-paragraph summary of this document.";
            var multimodalMessage = new Message
            {
                Role = ConversationRole.User,
                Content =
                [
                    new ContentBlock { Text = textPrompt },
                    new ContentBlock
                    {
                        Document = new DocumentBlock
                        {
                            Name = "Document",
                            Format = DocumentFormat.Txt,
                            Source = new DocumentSource { Bytes = documentBytes }
                        }
                    }
                ]
            };

            // Send the multimodal message to the model using the Converse API.
            var response = await client.ConverseStreamAsync(new ConverseStreamRequest
            {
                ModelId = modelId,
                Messages = [multimodalMessage]
            });

            // Extract and display the streamed response text in real-time.
            foreach (var chunk in response.Stream.AsEnumerable())
            {
                if (chunk is ContentBlockDeltaEvent streamEvent)
                {
                    Console.Write(streamEvent.Delta.Text);
                }
            }

            return response;

        }
        catch (IOException e)
        {
            Console.WriteLine($"Error loading image from {filePath}: {e.Message}");
            throw;
        }
        catch (AmazonBedrockRuntimeException e)
        {
            Console.WriteLine($"Error invoking model '{modelId}': {e.Message}");
            throw;
        }
    }
}