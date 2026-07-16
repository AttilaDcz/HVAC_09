using System.Collections.Generic;

namespace HVACDesigner.EngineeringData.BuildingThermal.Materials
{
    /// <summary>
    /// A materialdata.xml homogén Material rekordjának nyers DTO-ja.
    /// </summary>
    public sealed class BuildingMaterialDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public string Lambda { get; set; } = string.Empty;
        public string LambdaCorrection { get; set; } = string.Empty;
        public string Density { get; set; } = string.Empty;

        /// <summary>
        /// A jelenlegi XML-ben kJ/(kg*K) értelmezésű érték.
        /// </summary>
        public string SpecificHeatKilojoules { get; set; } =
            string.Empty;

        public string Mu { get; set; } = string.Empty;
        public string ValueSource { get; set; } = string.Empty;
        public string MoistureCondition { get; set; } = string.Empty;
        public string StandardReference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int SourceRecordIndex { get; set; }

        public IDictionary<string, string> Metadata { get; set; } =
            new Dictionary<string, string>();
    }
}
