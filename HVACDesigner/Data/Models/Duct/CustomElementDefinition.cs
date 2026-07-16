using System;

namespace HVACDesigner.Data.Models.Duct
{
    /// <summary>
    /// Egyedi elemek (hangcsillapítók, csappantyúk, szűrők, VAV-ok) konfigurációs definíciója.
    /// Tárolja a speciális működési és eltömődési nyomásértékeket is.
    /// </summary>
    public record CustomElementDefinition(
        string Name,
        double DefaultZeta,
        FlowDirection FlowDirection,
        GeometryType GeometryType,
        double MinOperatingPressure, // VAV / CAV dobozokhoz (Pa)
        double InitialPressureDrop,  // Szűrők kezdeti nyomásesése (Pa)
        double FinalPressureDrop     // Szűrők végső, eltömődött nyomásesése (Pa)
    );
}