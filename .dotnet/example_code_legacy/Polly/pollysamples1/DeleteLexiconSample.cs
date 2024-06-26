﻿// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0
﻿using System;
using Amazon.Polly;
using Amazon.Polly.Model;

namespace PollySamples1
{
    class DeleteLexiconSample
    {
        public static void DeleteLexicon()
        {
            String LEXICON_NAME = "SampleLexicon";

            var client = new AmazonPollyClient();
            var deleteLexiconRequest = new DeleteLexiconRequest()
            {
                Name = LEXICON_NAME
            };

            try
            {
                client.DeleteLexicon(deleteLexiconRequest);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught: " + e.Message);
            }
        }
    }
}
