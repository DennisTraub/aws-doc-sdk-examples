<?php
# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

#
# Integration test runner for Amazon Bedrock files.
#

namespace bedrock\tests;

use Bedrock\BedrockService;
use PHPUnit\Framework\TestCase;

/**
 * @group integ
 */
class BedrockBasicsTests extends TestCase
{
    protected BedrockService $bedrockService;

    public function test_constructor_uses_defaults()
    {
        $this->bedrockService = new BedrockService();
        $client = $this->bedrockService->getBedrockClient();
        self::assertEquals('us-west-2', $client->getRegion());
    }

    public function test_constructor_uses_provided_arguments()
    {
        $this->bedrockService = new BedrockService('test-region');
        $client = $this->bedrockService->getBedrockClient();
        self::assertEquals('test-region', $client->getRegion());
    }

    public function test_foundation_models_can_be_listed()
    {
        $this->bedrockService = new BedrockService('us-west-2', 'default', 'latest');
        $result = $this->bedrockService->listFoundationModels();
        self::assertNotEmpty($result['modelSummaries']);
    }
}
