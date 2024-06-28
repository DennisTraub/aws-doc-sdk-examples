// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// snippet-start:[BedrockRuntime.dotnetv3.ConverseStream_DocumentUpload.header]
// This script demonstrates how to use the Amazon Bedrock Converse API to upload
// a document for summarization and analysis.

using System;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
// snippet-end:[BedrockRuntime.dotnetv3.ConverseStream_DocumentUpload.header]

// Set the model ID, e.g., Claude 3 Haiku
// For a complete list of models supporting document upload, visit: https://go.aws/4buaEqu
const string modelId = "anthropic.claude-3-haiku-20240307-v1:0";

// snippet-start:[BedrockRuntime.dotnetv3.ConverseStream_DocumentUpload.code]
// Specify the path to the document
const string documentPath = "1997 Shareholder Letter.pdf";

// Load and prepare the document
using MemoryStream documentBytes = new MemoryStream(await File.ReadAllBytesAsync(documentPath));
var document = new ContentBlock
{
    Document = new DocumentBlock
    {
        Name = "document_name",
        Source = new DocumentSource { Bytes = documentBytes },
        Format = DocumentFormat.Pdf // Supports PDF, Word, Excel, CSV, HTML, MD, and plain text
    }
};

// Define the prompt or question you want to ask
var prompt = new ContentBlock { Text = "What is this document about?" };

// Set up the Amazon Bedrock Runtime client
var client = new AmazonBedrockRuntimeClient(RegionEndpoint.USEast1);

try
{
    // Send the document and your prompt to the model
    ConverseStreamResponse response = await client.ConverseStreamAsync(new ConverseStreamRequest()
    {
        ModelId = modelId,
        Messages = [ new Message
        {
            Role = ConversationRole.User,
            Content = [document, prompt]
        } ]
    });

    // Display the response stream in real time
    foreach (var chunk in response.Stream.AsEnumerable())
    {
        if (chunk is ContentBlockDeltaEvent @event)
        {
            Console.Write(@event.Delta.Text);
        }
    }
}
catch (AmazonBedrockRuntimeException e)
{
    Console.WriteLine($"ERROR: Unable to invoke '{modelId}'. Reason: {e.Message}");
}
// snippet-end:[BedrockRuntime.dotnetv3.ConverseStream_DocumentUpload.code]

// Create a partial class to make the top-level script testable.
namespace CrossModelScenarios { public class DocumentUpload; }
