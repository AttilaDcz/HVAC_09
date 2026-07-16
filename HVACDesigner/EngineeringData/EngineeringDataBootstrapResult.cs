using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HVACDesigner.EngineeringData.Registry;
using HVACDesigner.EngineeringData.Rules;
using HVACDesigner.EngineeringData.Rules.Common;

namespace HVACDesigner.EngineeringData
{
    public sealed class EngineeringDataBootstrapResult
    {
        public EngineeringDataRegistry DataRegistry { get; }
        public EngineeringRuleRegistry RuleRegistry { get; }
        public RulePackageBootstrapResult RuleResult { get; }
        public IReadOnlyList<string> Diagnostics { get; }

        public bool Succeeded
        {
            get
            {
                foreach (string diagnostic in Diagnostics)
                {
                    if (diagnostic.StartsWith(
                        "ERROR:",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return RuleResult.FailedFileCount == 0;
            }
        }

        public EngineeringDataBootstrapResult(
            EngineeringDataRegistry dataRegistry,
            EngineeringRuleRegistry ruleRegistry,
            RulePackageBootstrapResult ruleResult,
            IEnumerable<string> diagnostics)
        {
            DataRegistry =
                dataRegistry ??
                throw new ArgumentNullException(nameof(dataRegistry));

            RuleRegistry =
                ruleRegistry ??
                throw new ArgumentNullException(nameof(ruleRegistry));

            RuleResult =
                ruleResult ??
                throw new ArgumentNullException(nameof(ruleResult));

            Diagnostics = new ReadOnlyCollection<string>(
                new List<string>(
                    diagnostics ?? Array.Empty<string>()));
        }
    }
}
