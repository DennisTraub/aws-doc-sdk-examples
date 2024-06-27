<?php

include __DIR__ . '/../../vendor/autoload.php';

use Aws\BedrockRuntime\BedrockRuntimeClient;

$client = new BedrockRuntimeClient(['region' => 'us-east-1']);

$document = file_get_contents('C:\Users\traubd\Downloads\minimal-document.pdf');

$message = [
    'role' => 'user',
    'content' => [
        ['text' => 'Summarize this document.'],
        ['document' => [
            'format' => 'txt',
            'name' => 'document',
            'source' => ['bytes' => $document]
        ]]
    ]
];

$response = $client->converse([
    'modelId' => 'anthropic.claude-3-haiku-20240307-v1:0',
    'messages' => [$message]
]);

echo $response->search('output.message.content[0].text');
