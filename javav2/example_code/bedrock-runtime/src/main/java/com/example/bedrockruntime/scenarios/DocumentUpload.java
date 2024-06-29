// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

package com.example.bedrockruntime.scenarios;

import software.amazon.awssdk.core.SdkBytes;
import software.amazon.awssdk.core.exception.SdkClientException;
import software.amazon.awssdk.regions.Region;
import software.amazon.awssdk.services.bedrockruntime.BedrockRuntimeClient;
import software.amazon.awssdk.services.bedrockruntime.model.ContentBlock;
import software.amazon.awssdk.services.bedrockruntime.model.ConversationRole;
import software.amazon.awssdk.services.bedrockruntime.model.DocumentFormat;
import software.amazon.awssdk.services.bedrockruntime.model.DocumentSource;

import java.io.IOException;
import java.io.InputStream;

public class DocumentUpload {
    public static String documentUpload() {

        // Set the model ID. For the list of supported models visit: https://go.aws/4buaEqu
        String modelId = "anthropic.claude-3-haiku-20240307-v1:0";

        // Load the document from the resources folder
        var path = "./document.pdf";
        InputStream stream = DocumentUpload.class.getClassLoader().getResourceAsStream(path);
        SdkBytes document = SdkBytes.fromInputStream(stream);

        // Prepare the document for the upload
        var documentBlock = ContentBlock.fromDocument(doc -> doc
                .name("document_name")
                .source(DocumentSource.fromBytes(document))
                .format(DocumentFormat.PDF) // Supports PDF, Word, Excel, CSV, HTML, MD, and plain text
        );

        // Define the prompt or question you want to ask
        var textBlock = ContentBlock.fromText("Summarize this document.");

        // Set up the Amazon Bedrock Runtime client
        var client = BedrockRuntimeClient.builder().region(Region.US_EAST_1).build();

        try {
            // Send the document and prompt to the model
            var response = client.converse(request -> request
                    .modelId(modelId)
                    .messages(message -> message
                            .role(ConversationRole.USER)
                            .content(documentBlock, textBlock)
                    )
            );

            // Display the model's response
            var responseText = response.output().message().content().get(0).text();
            System.out.println(responseText);

            return responseText;

        } catch (SdkClientException e) {
            System.err.printf("ERROR: Can't invoke '%s'. Reason: %s", modelId, e.getMessage());
            throw new RuntimeException(e);
        }
    }

    public static void main(String[] args) throws IOException {
        documentUpload();
    }
}
