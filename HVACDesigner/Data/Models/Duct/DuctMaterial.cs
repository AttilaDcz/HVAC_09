namespace HVACDesigner.Data.Models.Duct
{
    /// <summary>
    /// Légcsatorna anyagok fizikai és áramlástani tulajdonságait (abszolút érdesség) tároló modell.
    /// </summary>
    public class DuctMaterial
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Az anyag abszolút érdessége (k) milliméterben (mm). (Pl. Spiro = 0.15, PIR = 0.05).
        /// </summary>
        public double Roughness { get; set; }

        /// <summary>
        /// Jelzi, ha az anyag hajlékony, flexibilis jellegű csővezeték.
        /// </summary>
        public bool IsFlexible { get; set; }
    }
}
