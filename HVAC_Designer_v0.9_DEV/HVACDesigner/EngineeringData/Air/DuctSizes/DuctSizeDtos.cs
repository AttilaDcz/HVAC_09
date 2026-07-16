namespace HVACDesigner.EngineeringData.Air.DuctSizes
{
    public sealed class CircularDuctSizeDto
    {
        public string DiameterMillimeters { get; set; } = string.Empty;
        public int SourceRecordIndex { get; set; }
    }

    public sealed class RectangularDuctSizeDto
    {
        public string WidthMillimeters { get; set; } = string.Empty;
        public string HeightMillimeters { get; set; } = string.Empty;
        public int SourceRecordIndex { get; set; }
    }
}
