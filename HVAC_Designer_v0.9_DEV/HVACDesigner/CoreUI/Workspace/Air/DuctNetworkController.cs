using System;
using System.IO;
using System.Linq;
using HVACDesigner.Calculations.Air;
using HVACDesigner.Data.Models.Duct;
using HVACDesigner.Data.Providers;

namespace HVACDesigner.CoreUI.Workspace.Air
{
    public class DuctNetworkController
    {
        private readonly IDuctDataProvider _dataProvider;
        public IDuctDataProvider DataProvider => _dataProvider;

        private readonly DuctCalculator _ductCalc;
        private readonly NetworkCalculator _netCalc;

        public DuctNetwork Network { get; private set; }

        // MÉRÖNKI GYORSÍTÓTÁR: Megjegyzi a legutóbb sikeresen hozzáadott elemet
        // MÉRÖNKI GYORSÍTÓTÁR: Megjegyzi a legutóbb sikeresen hozzáadott elemet
        public DuctElement? LastAddedElement { get; set; }

        // ÚJ: Megjegyzi az utoljára használt geometriát az új elemek automatikus méretezéséhez
        public DuctGeometry? LastUsedGeometry { get; set; }

        public DuctNetworkController()
        {
            // System.IO bevonásával vagy teljes elérési úttal szebb
            string xmlRelativePath = Path.Combine("Data", "Xml", "ductdata.xml");
            _dataProvider = new XmlDuctDataProvider(xmlRelativePath);
            _ductCalc = new DuctCalculator();
            _netCalc = new NetworkCalculator(_ductCalc);
            Network = new DuctNetwork();
        }

        public DuctBranch AddBranch(string name, double flowM3h)
        {
            var branch = Network.AddBranch(name, HVACDesigner.Data.Models.Duct.FlowDirection.Supply);
            branch.FlowM3h = flowM3h;
            return branch;
        }

        public void AddElement(DuctBranch branch, DuctElement element)
        {
            branch.AddElement(element);

            // Mentés a gyorsítótárba a méret- és formaöröklődéshez
            LastAddedElement = element;
        }

        public void Calculate()
        {
            double maxLoss = 0;
            foreach (var branch in Network.Branches)
            {
                // Ideiglenes teszt fázisú hívás, a kalkulátort később újraírjuk
                branch.PressureLossPa = _netCalc.ComputeBranchLoss(branch.Elements, branch.FlowM3h);
                if (branch.PressureLossPa > maxLoss)
                {
                    maxLoss = branch.PressureLossPa;
                }
            }
        }
    }
}
