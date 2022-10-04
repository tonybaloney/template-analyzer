﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Microsoft.Azure.Templates.Analyzer.Reports
{
    /// <summary>
    /// An <see cref="IReportWriter"/> for writing results to the console.
    /// </summary>
    public class ConsoleReportWriter : IReportWriter
    {
        internal static string IndentedNewLine = Environment.NewLine + "\t";
        internal static string TwiceIndentedNewLine = Environment.NewLine + "\t\t";

        private readonly List<string> filesAlreadyOutput = new List<string>();

        /// <inheritdoc/>
        public void WriteResults(IEnumerable<Types.IEvaluation> evaluations, IFileInfo templateFile, IFileInfo parametersFile = null)
        {
            var resultsByFile = ReportsHelper.GetResultsByFile(evaluations, filesAlreadyOutput, out int passedEvaluations);

            // output files in sorted order, but always output root first and regardless if no results
            var filesWithResults = resultsByFile.Keys.ToList();
            filesWithResults.Remove(templateFile.FullName);
            filesWithResults.Sort();
            filesWithResults.Insert(0, templateFile.FullName);

            foreach(var fileWithResults in filesWithResults)
            {
                string fileMetadata;
                fileMetadata = Environment.NewLine + Environment.NewLine + $"File: {fileWithResults}";

                if (fileWithResults == templateFile.FullName)
                {
                    if (parametersFile != null)
                    {
                        fileMetadata += Environment.NewLine + $"Parameters File: {parametersFile}";
                    }
                }
                else
                {
                    fileMetadata += Environment.NewLine + $"Root Template: {templateFile}";
                }

                Console.WriteLine(fileMetadata);

                // needed for all passing results case
                if (!resultsByFile.ContainsKey(fileWithResults))
                {
                    continue;
                }

                foreach ((var evaluation, var failedResults) in resultsByFile[fileWithResults])
                {
                    string resultString = string.Concat(failedResults.Select(result => $"{TwiceIndentedNewLine}Line: {result.SourceLocation.LineNumber}"));
                    var output = $"{IndentedNewLine}{(evaluation.RuleId != "" ? $"{evaluation.RuleId}: " : "")}{evaluation.RuleDescription}" +
                        $"{TwiceIndentedNewLine}Severity: {evaluation.Severity}" +
                        (!string.IsNullOrWhiteSpace(evaluation.Recommendation) ? $"{TwiceIndentedNewLine}Recommendation: {evaluation.Recommendation}" : "") +
                        $"{TwiceIndentedNewLine}More information: {evaluation.HelpUri}" +
                        $"{TwiceIndentedNewLine}Result: {(evaluation.Passed ? "Passed" : "Failed")} {resultString}";
                    Console.WriteLine(output);
                }
            }

            filesAlreadyOutput.AddRange(filesWithResults);

            Console.WriteLine($"{IndentedNewLine}Rules passed: {passedEvaluations}");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
