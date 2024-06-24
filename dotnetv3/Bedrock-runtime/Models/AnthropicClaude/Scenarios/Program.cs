// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved. 
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scenarios;

internal class Program
{
    static async Task Main()
    {
        var methods = new List<Scenario>
        {
            new("Send an image using Converse", MultimodalWithImage.Converse),
            new("Send an image using ConverseStream", MultimodalWithImage.ConverseStream),
            new("Send a document using Converse", MultimodalWithDocument.Converse),
            new("Send a document using ConverseStream", MultimodalWithDocument.ConverseStream),
            new("Use native request and response fields (Converse)", AdditionalRequestAndResponseFields.Converse),
            new("Use native request and response fields (ConverseStream)", AdditionalRequestAndResponseFields.ConverseStream)
        };

        while (true)
        {
            DisplayMenu(methods);

            string input = Console.ReadLine()!.Trim().ToLower();

            if (input == "x")
            {
                Console.WriteLine("Exiting the application. Goodbye!");
                break;
            }

            if (int.TryParse(input, out int choice) && choice > 0 && choice <= methods.Count)
            {
                Console.WriteLine($"\nExecuting: {methods[choice - 1].Description}");
                await methods[choice - 1].Method();
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number or 'x' to exit.");
            }

            Console.WriteLine();

        }
    }

    static void DisplayMenu(List<Scenario> scenarios)
    {
        Console.WriteLine("Please choose a method to execute:");
        for (int i = 0; i < scenarios.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {scenarios[i].Description}");
        }
        Console.WriteLine("x. Exit");
        Console.Write("Enter your choice: ");
    }

    private record Scenario(string Description, Func<Task> Method);
}