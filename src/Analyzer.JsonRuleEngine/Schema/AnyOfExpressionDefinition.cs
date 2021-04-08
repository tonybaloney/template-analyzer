﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.Azure.Templates.Analyzer.RuleEngines.JsonEngine.Expressions;
using Microsoft.Azure.Templates.Analyzer.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Azure.Templates.Analyzer.RuleEngines.JsonEngine.Schemas
{
    /// <summary>
    /// The schema for anyOf expressions in JSON rules.
    /// </summary>
    internal class AnyOfExpressionDefinition : ExpressionDefinition
    {
        /// <summary>
        /// Gets or sets the expressions found in AnyOf.
        /// </summary>
        [JsonProperty]
        public ExpressionDefinition[] AnyOf { get; set; }

        /// <summary>
        /// Creates an <see cref="AnyOfExpression"/> capable of evaluating JSON using the expressions specified in the JSON rule.
        /// </summary>
        /// <param name="jsonLineNumberResolver">An <see cref="ILineNumberResolver"/> to
        /// pass to the created <see cref="Expression"/>.</param>
        /// <returns>The AnyOfExpression.</returns>
        public override Expression ToExpression(ILineNumberResolver jsonLineNumberResolver) =>
            new AnyOfExpression(this.AnyOf.Select(e => e.ToExpression(jsonLineNumberResolver)).ToArray(), GetCommonProperties(jsonLineNumberResolver));

        /// <summary>
        /// Validates the <see cref="AnyOfExpressionDefinition"/> for valid syntax
        /// </summary>
        internal override void Validate()
        {
            if (!(this.AnyOf?.Count() > 0))
            {
                throw new JsonException("No expressions were specified in the anyOf expression");
            }

            int nullCount = this.AnyOf.Count(e => e == null);

            if (nullCount > 0)
            {
                throw new JsonException($"Null expressions are not valid. {nullCount} expressions are null in anyOf expression");
            }
        }
    }
}
