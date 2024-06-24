// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime.Documents;

namespace Scenarios;

public class AdditionalRequestAndResponseFields
{
    public static async Task<ConverseResponse?> Converse()
    {
        // The model ID, e.g., Claude 3 Haiku.
        const string modelId = "anthropic.claude-3-haiku-20240307-v1:0";

        // Create a Bedrock Runtime client in the AWS Region you want to use.
        var client = new AmazonBedrockRuntimeClient(RegionEndpoint.USEast1);

        // Define the user message.
        var userMessage = "Describe the purpose of a 'hello world' program in one line";

        // Create a request with the model ID, the user message, and an inference configuration.
        var request = new ConverseRequest
        {
            ModelId = modelId,
            Messages =
            [
                new Message
                {
                    Role = ConversationRole.User,
                    Content = [new ContentBlock { Text = userMessage }]
                }
            ],

            // Basic inference parameters supported by the Converse API:
            InferenceConfig = new InferenceConfiguration
            {
                MaxTokens = 512,
                Temperature = 1.0F,
                TopP = 0.999F,
                StopSequences = [ "Human:", "User:" ]
            },

            // Additional inference parameters supported by the individual model:
            AdditionalModelRequestFields = Document.FromObject(new { top_k = 100 }),

            // Path(s) to specific response fields provided by the individual model:
            AdditionalModelResponseFieldPaths = [ "/usage" ]
        };

        try
        {
            // Send the request to the Bedrock Runtime and wait for the result.
            var response = await client.ConverseAsync(request);

            // Extract and print the response text.
            Console.WriteLine($"\nResponse text: \n{response?.Output?.Message?.Content?[0]?.Text}");

            // Extract and print the additional response fields.
            var usage = response?.AdditionalModelResponseFields.AsDictionary()?["usage"].AsDictionary();
            Console.WriteLine("\nAdditional response fields:");
            Console.WriteLine($"/usage/input_tokens: {usage?["input_tokens"].AsInt()}");
            Console.WriteLine($"/usage/output_tokens: {usage?["output_tokens"].AsInt()}");

            return response;

        }
        catch (AmazonBedrockRuntimeException e)
        {
            Console.WriteLine($"ERROR: Can't invoke '{modelId}'. Reason: {e.Message}");
            throw;
        }
    }

    public static async Task<ConverseResponse> ConverseStream()
    {
        // The model ID, e.g., Claude 3 Haiku.
        const string modelId = "anthropic.claude-3-haiku-20240307-v1:0";

        // Create a Bedrock Runtime client in the AWS Region you want to use.
        var client = new AmazonBedrockRuntimeClient(RegionEndpoint.USEast1);

        // Define the user message.
        var userMessage = "Describe the purpose of a 'hello world' program in one line";

        // Create a request with the model ID, the user message, and an inference configuration.
        var request = new ConverseStreamRequest()
        {
            ModelId = modelId,
            Messages =
            [
                new Message
                {
                    Role = ConversationRole.User,
                    Content = [new ContentBlock { Text = userMessage }]
                }
            ],

            // Basic inference parameters supported by the Converse API:
            InferenceConfig = new InferenceConfiguration
            {
                MaxTokens = 512,
                Temperature = 1.0F,
                TopP = 0.999F,
                StopSequences = ["Human:", "User:"]
            },

            // Additional inference parameters supported by the individual model:
            AdditionalModelRequestFields = Document.FromObject(new { top_k = 100 }),

            // Path(s) to additional response fields provided by the individual model:
            AdditionalModelResponseFieldPaths = ["/usage"]
        };

        try
        {
            // Send the request to the Bedrock Runtime and wait for the result.
            var response = await client.ConverseStreamAsync(request);

            // Prepare an empty response object to store the final results.
            var finalResponse = PrepareEmptyResponseObject();

            // Process the response stream and store the role, text and additional response fields.
            foreach (var chunk in response.Stream.AsEnumerable())
            {
                if (chunk is MessageStartEvent startEvent)
                {
                    finalResponse.Output.Message.Role = startEvent.Role;
                }
                else if (chunk is ContentBlockDeltaEvent deltaEvent)
                {
                    var delta = deltaEvent.Delta.Text;
                    finalResponse.Output.Message.Content[0].Text += delta;

                    Console.Write(delta);
                }
                else if (chunk is MessageStopEvent stopEvent)
                {
                    finalResponse.AdditionalModelResponseFields = stopEvent.AdditionalModelResponseFields;

                    var usage = stopEvent.AdditionalModelResponseFields.AsDictionary()?["usage"].AsDictionary();
                    // Console.WriteLine("\nAdditional response fields:");
                    // Console.WriteLine($"/usage/input_tokens: {usage?["input_tokens"].AsInt()}");
                    // Console.WriteLine($"/usage/output_tokens: {usage?["output_tokens"].AsInt()}");
                }
            }

            return finalResponse;

        }
        catch (AmazonBedrockRuntimeException e)
        {
            Console.WriteLine($"ERROR: Can't invoke '{modelId}'. Reason: {e.Message}");
            throw;
        }
    }

    private static ConverseResponse PrepareEmptyResponseObject()
    {
        return new ConverseResponse
        {
            Output = new ConverseOutput
            {
                Message = new Message
                {
                    Content = [new ContentBlock { Text = "" }]
                }
            }
        };
    }
}
