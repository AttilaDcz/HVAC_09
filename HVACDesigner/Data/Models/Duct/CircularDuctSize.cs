namespace HVACDesigner.Data.Models.Duct
{
    /// <summary>
    /// Szabványos kör keresztmetszetű átmérő konfigurációs modellje.
    /// </summary>
    public record CircularDuctSize(int Diameter)
    {
        /// <summary>
        /// Intelligens, gépész formátumú megnevezés a legördülő menükhöz (pl. Ø160 mm).
        /// </summary>
        public string DisplayName => $"Ø{Diameter} mm";
    }
}