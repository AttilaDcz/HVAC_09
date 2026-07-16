using System.Collections.Generic;
using HVACDesigner.Data.Models.Duct;

namespace HVACDesigner.Data.Providers
{
    public interface IDuctDataProvider
    {
        //========================================================
        // Anyagok
        //========================================================

        IReadOnlyList<DuctMaterial> GetMaterials();


        //========================================================
        // Szabványos méretek
        //========================================================

        IReadOnlyList<CircularDuctSize> GetCircularDuctSizes();

        IReadOnlyList<RectangularDuctSize> GetRectangularDuctSizes();


        //========================================================
        // Egyenes légcsatornák
        //========================================================

        IReadOnlyList<DuctDefinition> GetStraightDucts();

        IReadOnlyList<DuctDefinition> GetFlexibleDucts();


        //========================================================
        // Kör keresztmetszetű idomok
        //========================================================

        IReadOnlyList<CircularFittingDefinition> GetCircularFittings();


        //========================================================
        // Négyszög keresztmetszetű idomok
        //========================================================

        IReadOnlyList<RectangularFittingDefinition> GetRectangularFittings();


        //========================================================
        // Átmeneti idomok
        //========================================================

        IReadOnlyList<TransitionFittingDefinition> GetTransitionFittings();


        //========================================================
        // Elágazó idomok
        //========================================================

        IReadOnlyList<BranchFittingDefinition> GetBranchFittings();


        //========================================================
        // Zsaluk, szűrők, hangtompítók, végponti elemek
        // 
        // Damper
        // Filter
        // Silencer
        // VAV
        // CAV
        // Grille
        // Diffuser
        // Louver
        // RoofCap
        // Hood
        //========================================================

        IReadOnlyList<DuctAccessoryDefinition> GetDuctAccessories();


        //========================================================
        // Egyedi felhasználói elemek
        //========================================================

        IReadOnlyList<CustomElementDefinition> GetCustomElements();
    }
}