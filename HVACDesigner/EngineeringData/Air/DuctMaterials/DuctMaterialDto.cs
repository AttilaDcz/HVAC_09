namespace HVACDesigner.EngineeringData.Air.DuctMaterials
{
    /// <summary>
    /// Az XML/SQL/API forrásból érkező nyers légcsatorna-anyag rekord.
    /// A DTO nem tartalmaz mérnöki logikát.
    /// </summary>
    public sealed class DuctMaterialDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// A jelenlegi ductdata.xml-ben milliméterben tárolt érdesség.
        /// </summary>
        public string RoughnessMillimeters { get; set; } =
            string.Empty;

        public string Flexible { get; set; } =
            string.Empty;

        public int SourceRecordIndex { get; set; }
    }
}
