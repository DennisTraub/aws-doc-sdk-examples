<?php
# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

#snippet-start:[php.example_code.bedrock.service]
namespace Bedrock;

use Aws\Bedrock\BedrockClient;
use AwsUtilities\AWSServiceClass;

class BedrockService extends AWSServiceClass
{
    protected BedrockClient $bedrockClient;

    public function getBedrockClient(): BedrockClient
    {
        return $this->bedrockClient;
    }

    public function __construct($region = 'us-west-2', $profile = 'default', $version = 'latest')
    {
        $this->bedrockClient = new BedrockClient([
            'region' => $region,
            'profile' => $profile,
            'version' => $version
        ]);
    }

    #snippet-start:[php.example_code.bedrock.service.listFoundationModels]
    public function listFoundationModels()
    {
        return $this->bedrockClient->listFoundationModels();
    }
    #snippet-end:[php.example_code.bedrock.service.listFoundationModels]
}
#snippet-end:[php.example_code.bedrock.service]
