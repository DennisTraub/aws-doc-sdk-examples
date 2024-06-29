<?php

// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

include __DIR__ . '/../vendor/autoload.php';

// snippet-start:[php.example_code.bedrock-runtime.converse_stream.document_upload.header]
// This script demonstrates how to use the Amazon Bedrock Converse API to upload
// a document for summarization and analysis with the AWS SDK for PHP.

use Aws\BedrockRuntime\BedrockRuntimeClient;
use Aws\Exception\AwsException;

// snippet-end:[php.example_code.bedrock-runtime.converse_stream.document_upload.header]

// Set the model ID. For the list of supported models visit: https://go.aws/4buaEqu
$modelId = 'anthropic.claude-3-haiku-20240307-v1:0';

// snippet-start:[php.example_code.bedrock-runtime.converse_stream.document_upload.main]

// Prepare your document for the upload
$documentPath = 'resources/document.pdf';
$document = [
    'document' => [
        'format' => 'pdf',
        'name' => 'document',
        'source' => ['bytes' => file_get_contents($documentPath)]
    ]
];

// Define the prompt or question you want to ask
$prompt = ['text' => 'Summarize this document in 10 paragraphs.'];

// Set up the Amazon Bedrock Runtime client
$client = new BedrockRuntimeClient(['region' => 'us-east-1']);

try {
    // Send the document and your prompt to the model
    $response = $client->converseStream([
        'modelId' => $modelId,
        'messages' => [[
            'role' => 'user',
            'content' => [$prompt, $document]
        ]]
    ]);

    // Display the response stream in real time
    foreach ($response['stream'] as $event) {
        if (isset($event['contentBlockDelta'])) {
            $delta = $event['contentBlockDelta']['delta'];
            if (isset($delta['text'])) {
                echo $delta['text'];
            }
        }
    }
} catch (AwsException $e) {
    echo "ERROR: Unable to invoke $modelId. Reason: {$e->getAwsErrorMessage()}";
}

// snippet-end:[php.example_code.bedrock-runtime.converse_stream.document_upload.main]
