namespace HVACDesigner.CoreUI.Workspace.Water
{
    internal sealed class WaterFixtureGridRow
    {
        public string FixtureId { get; }
        public string DisplayName { get; }
        public int Quantity { get; }
        public double? LoadingUnit { get; }
        public double? DischargeUnit { get; }
        public int? MinimumDn { get; }

        public double TotalLoadingUnits =>
            Quantity * (LoadingUnit ?? 0.0);

        public double TotalDischargeUnits =>
            Quantity * (DischargeUnit ?? 0.0);

        public WaterFixtureGridRow(
            string fixtureId,
            string displayName,
            int quantity,
            double? loadingUnit,
            double? dischargeUnit,
            int? minimumDn)
        {
            FixtureId = fixtureId;
            DisplayName = displayName;
            Quantity = quantity;
            LoadingUnit = loadingUnit;
            DischargeUnit = dischargeUnit;
            MinimumDn = minimumDn;
        }
    }
}

