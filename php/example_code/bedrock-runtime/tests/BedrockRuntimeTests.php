<?php
# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

#
# Integration tests for the Amazon Bedrock Runtime service.
#

namespace bedrockruntime\tests;

use BedrockRuntime\BedrockRuntimeService;
use PHPUnit\Framework\TestCase;

/**
 * @group integ
 */
class BedrockRuntimeTests extends TestCase
{
    protected BedrockRuntimeService $bedrockRuntimeService;

    private string $prompt = 'A test prompt';

    public function test_constructor_uses_defaults()
    {
        $this->bedrockRuntimeService = new BedrockRuntimeService();
        $client = $this->bedrockRuntimeService->getBedrockRuntimeClient();
        self::assertEquals('us-west-2', $client->getRegion());
    }

    public function test_constructor_uses_provided_arguments()
    {
        $this->bedrockRuntimeService = new BedrockRuntimeService('test-region');
        $client = $this->bedrockRuntimeService->getBedrockRuntimeClient();
        self::assertEquals('test-region', $client->getRegion());
    }

    public function test_claude_can_be_invoked()
    {
        $this->bedrockRuntimeService = new BedrockRuntimeService('us-east-1');
        $completion = $this->bedrockRuntimeService->invokeClaude($this->prompt);
        self::assertNotEmpty($completion);
    }

    public function test_jurassic2_can_be_invoked()
    {
        $this->bedrockRuntimeService = new BedrockRuntimeService('us-east-1');
        $completion = $this->bedrockRuntimeService->invokeJurassic2($this->prompt);
        self::assertNotEmpty($completion);
    }

    public function test_llama2_can_be_invoked()
    {
        $this->bedrockRuntimeService = new BedrockRuntimeService('us-east-1');
        $completion = $this->bedrockRuntimeService->invokeLlama2($this->prompt);
        self::assertNotEmpty($completion);
    }

    public function test_stable_diffusion_can_be_invoked()
    {
        $this->bedrockRuntimeService = new BedrockRuntimeService('us-east-1');
        $seed = 0;
        $style_preset = "photographic";
        $base64_image_data = $this->bedrockRuntimeService->invokeStableDiffusion($this->prompt, $seed, $style_preset);
        self::assertNotEmpty($base64_image_data);
    }

    public function test_titan_image_can_be_invoked()
    {
        $this->bedrockRuntimeService = new BedrockRuntimeService('us-east-1');
        $seed = 0;
        $base64_image_data = $this->bedrockRuntimeService->invokeTitanImage($this->prompt, $seed);
        self::assertNotEmpty($base64_image_data);
    }
}
