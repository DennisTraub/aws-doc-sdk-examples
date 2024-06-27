// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

package com.example.bedrockruntime.models.anthropicClaude.scenarios;

import software.amazon.awssdk.core.SdkBytes;
import software.amazon.awssdk.regions.Region;
import software.amazon.awssdk.services.bedrockruntime.BedrockRuntimeClient;
import software.amazon.awssdk.services.bedrockruntime.model.ContentBlock;
import software.amazon.awssdk.services.bedrockruntime.model.ConversationRole;
import software.amazon.awssdk.services.bedrockruntime.model.DocumentFormat;
import software.amazon.awssdk.services.bedrockruntime.model.DocumentSource;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;

public class MultimodalWithDocument {
    public static void converse() throws IOException {

        var client = BedrockRuntimeClient.builder().region(Region.US_EAST_1).build();

        String filePath = "path/to/your/document.pdf";
        SdkBytes fileContent = SdkBytes.fromByteArray(Files.readAllBytes(Paths.get(filePath)));

        var textMessage = ContentBlock.fromText("");
        var document = ContentBlock.fromDocument(doc -> doc.name("document")
                .format(DocumentFormat.PDF)
                .source(DocumentSource.fromBytes(fileContent)));

        var response = client.converse(req -> req
                .modelId("anthropic.claude-3-haiku-20240307-v1:0")
                .messages(message -> message
                        .role(ConversationRole.USER)
                        .content(textMessage, document)));

        var responseText = response.output().message().content().get(0).text();
        System.out.println(responseText);
    }

    public static void main(String[] args) throws IOException {
        converse();
    }
}
