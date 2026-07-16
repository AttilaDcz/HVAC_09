namespace HVACDesigner.Data.Models.Duct
{
    public class ElementCreationParameters
    {
        public double Length { get; set; }

        public double Airflow { get; set; }


        public DuctGeometry Geometry { get; set; } = new();


        public double Zeta { get; set; }


        public double? FixedPressureDrop { get; set; }


        public DuctMaterial? MaterialOverride { get; set; }


        public double ShankLength1 { get; set; }

        public double ShankLength2 { get; set; }


        public bool HasLocalTerminal { get; set; }

        public double LocalTerminalAirflow { get; set; }


        public double FreeAreaPercent { get; set; } = 100;


        // ÚJ
        // XML vagy UI alapján kerül kitöltésre
        public string Subtype { get; set; } = "";


        // ÚJ
        // későbbi Factory döntésekhez
        public DuctElementType ElementType { get; set; }


        // ÚJ
        public PressureLossType PressureLossType { get; set; }
            = PressureLossType.Zeta;


        // ÚJ
        // szűrők, VAV-ok stb.
        public bool UseFixedPressureDrop { get; set; }


        public string Name { get; set; } = "";

        public string Description { get; set; } = "";


        public void ApplyGeometryDefaults()
        {
            if (Geometry == null)
                Geometry = new DuctGeometry();
        }
    }
}
