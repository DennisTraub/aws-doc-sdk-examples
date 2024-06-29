// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

package scenarios;

import actions.IntegrationTestBase;
import com.example.bedrockruntime.scenarios.DocumentUpload;
import org.junit.jupiter.api.Test;

public class TestDocumentUpload extends IntegrationTestBase {

    @Test
    void documentUploadReturnsResponse() {
        String response = DocumentUpload.documentUpload();
        assertNotNullOrEmpty(response);
    }

}
