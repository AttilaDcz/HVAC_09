using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Help;
using HVACDesigner.CoreUI.Components.Structural;
using HVACDesigner.CoreUI.Status;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Components.Engineering
{
    public sealed class EngineeringUnitSelector : UserControl, IThemeable
    {
        private readonly EngineeringToolTip toolTip;

        private readonly EngineeringComboBox airFlowBox;
        private readonly EngineeringComboBox airPressureBox;
        private readonly EngineeringComboBox airDimensionBox;

        private readonly EngineeringComboBox hydraulicFlowBox;
        private readonly EngineeringComboBox hydraulicPressureBox;
        private readonly EngineeringComboBox hydraulicDimensionBox;

        private readonly EngineeringComboBox sanitaryFlowBox;
        private readonly EngineeringComboBox sanitaryPressureBox;
        private readonly EngineeringComboBox sanitaryDimensionBox;

        private readonly EngineeringComboBox energeticThicknessBox;
        private readonly EngineeringComboBox energeticLengthBox;
        private readonly EngineeringComboBox energeticPowerBox;

        private ThemePalette palette = ThemeManager.CurrentPalette;

        public EngineeringUnitSelector()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            toolTip = new EngineeringToolTip();

            airFlowBox = CreateCombo("Térfogatáram");
            airPressureBox = CreateCombo("Nyomás");
            airDimensionBox = CreateCombo("Méret");

            hydraulicFlowBox = CreateCombo("Térfogatáram");
            hydraulicPressureBox = CreateCombo("Nyomás");
            hydraulicDimensionBox = CreateCombo("Méret");

            sanitaryFlowBox = CreateCombo("Térfogatáram");
            sanitaryPressureBox = CreateCombo("Nyomás");
            sanitaryDimensionBox = CreateCombo("Méret");

            energeticThicknessBox = CreateCombo("Vastagság");
            energeticLengthBox = CreateCombo("Hossz");
            energeticPowerBox = CreateCombo("Teljesítmény");

            Size = new Size(710, 360);
            BackColor = palette.Surface;

            BuildLayout();
            LoadOptions();
            LoadCurrentValues();
            ApplyTheme(palette);
        }

        public void ApplyChanges()
        {
            UnitContext.Air.Flow = GetSelectedValue<AirFlowUnit>(airFlowBox, UnitContext.Air.Flow);
            UnitContext.Air.Pressure = GetSelectedValue<AirPressureUnit>(airPressureBox, UnitContext.Air.Pressure);
            UnitContext.Air.Dimension = GetSelectedValue<LengthUnit>(airDimensionBox, UnitContext.Air.Dimension);

            UnitContext.Hydraulics.Flow = GetSelectedValue<FluidFlowUnit>(hydraulicFlowBox, UnitContext.Hydraulics.Flow);
            UnitContext.Hydraulics.Pressure = GetSelectedValue<FluidPressureUnit>(hydraulicPressureBox, UnitContext.Hydraulics.Pressure);
            UnitContext.Hydraulics.Dimension = GetSelectedValue<LengthUnit>(hydraulicDimensionBox, UnitContext.Hydraulics.Dimension);

            UnitContext.Sanitary.Flow = GetSelectedValue<FluidFlowUnit>(sanitaryFlowBox, UnitContext.Sanitary.Flow);
            UnitContext.Sanitary.Pressure = GetSelectedValue<FluidPressureUnit>(sanitaryPressureBox, UnitContext.Sanitary.Pressure);
            UnitContext.Sanitary.Dimension = GetSelectedValue<LengthUnit>(sanitaryDimensionBox, UnitContext.Sanitary.Dimension);

            UnitContext.Energetics.Thickness = GetSelectedValue<LengthUnit>(energeticThicknessBox, UnitContext.Energetics.Thickness);
            UnitContext.Energetics.Length = GetSelectedValue<LengthUnit>(energeticLengthBox, UnitContext.Energetics.Length);
            UnitContext.Energetics.HeatFlow = GetSelectedValue<PowerUnit>(energeticPowerBox, UnitContext.Energetics.HeatFlow);

            UnitContext.TriggerUnitChanged();
            EngineeringStatusMessages.SetReady("Mértékegység-beállítások frissítve");
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Surface;
            toolTip.ApplyTheme(palette);

            foreach (Control control in Controls)
                ApplyThemeRecursive(control);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                toolTip.Dispose();

            base.Dispose(disposing);
        }

        private void BuildLayout()
        {
            EngineeringCardPanel airCard = CreateCard(
                "Légtechnika",
                "Légcsatorna, füstgáz és légsebesség modulokhoz.",
                new Point(0, 0));
            AddComboRow(airCard.ContentPanel, airFlowBox, airPressureBox, airDimensionBox);

            EngineeringCardPanel hydraulicCard = CreateCard(
                "Hidraulika",
                "Fűtési, hűtési és szivattyús csőhálózatokhoz.",
                new Point(355, 0));
            AddComboRow(hydraulicCard.ContentPanel, hydraulicFlowBox, hydraulicPressureBox, hydraulicDimensionBox);

            EngineeringCardPanel sanitaryCard = CreateCard(
                "Víz-csatorna",
                "Ivóvíz, szennyvíz és tetővíz számítások kijelzéséhez.",
                new Point(0, 178));
            AddComboRow(sanitaryCard.ContentPanel, sanitaryFlowBox, sanitaryPressureBox, sanitaryDimensionBox);

            EngineeringCardPanel energeticCard = CreateCard(
                "Energetika",
                "Épületszerkezetek és hőtechnikai modulokhoz.",
                new Point(355, 178));
            AddComboRow(energeticCard.ContentPanel, energeticThicknessBox, energeticLengthBox, energeticPowerBox);

            Controls.AddRange(new Control[] { airCard, hydraulicCard, sanitaryCard, energeticCard });

            toolTip.SetHelpRecursive(
                this,
                "A választás csak a megjelenítést és adatbevitelt befolyásolja. A számítási modellek saját, egyértelmű belső egységeikkel dolgoznak.",
                "Mértékegység-preferencia",
                EngineeringToolTipKind.Info);
        }

        private EngineeringCardPanel CreateCard(string title, string subtitle, Point location)
        {
            EngineeringCardPanel card = new EngineeringCardPanel
            {
                Title = title,
                Subtitle = subtitle,
                ShowIcon = false,
                ShowStatusBadge = false,
                ShowAccentStrip = true,
                ShowSeparator = true,
                Location = location,
                Size = new Size(335, 158)
            };
            card.ContentPanel.Padding = new Padding(14, 12, 14, 10);
            card.ApplyTheme(palette);
            return card;
        }

        private static EngineeringComboBox CreateCombo(string label)
        {
            return new EngineeringComboBox
            {
                LabelText = label,
                DisplayMember = nameof(UnitOption.Display),
                ValueMember = nameof(UnitOption.Value),
                Size = new Size(96, 56)
            };
        }

        private void AddComboRow(
            Control parent,
            EngineeringComboBox first,
            EngineeringComboBox second,
            EngineeringComboBox third)
        {
            first.Location = new Point(0, 34);
            second.Location = new Point(108, 34);
            third.Location = new Point(216, 34);

            parent.Controls.AddRange(new Control[] { first, second, third });
        }

        private void LoadOptions()
        {
            AddOptions(airFlowBox,
                Option("m³/h", AirFlowUnit.CubicMeterPerHour),
                Option("l/s", AirFlowUnit.LitersPerSecond),
                Option("m³/s", AirFlowUnit.CubicMeterPerSecond));
            AddOptions(airPressureBox,
                Option("Pa", AirPressureUnit.Pascal),
                Option("kPa", AirPressureUnit.Kilopascal),
                Option("mbar", AirPressureUnit.Millibar),
                Option("mmH₂O", AirPressureUnit.MillimetersOfWater));
            AddLengthOptions(airDimensionBox);

            AddOptions(hydraulicFlowBox,
                Option("m³/h", FluidFlowUnit.CubicMeterPerHour),
                Option("l/s", FluidFlowUnit.LitersPerSecond),
                Option("l/h", FluidFlowUnit.LitersPerHour),
                Option("kg/h", FluidFlowUnit.KilogramPerHour));
            AddFluidPressureOptions(hydraulicPressureBox);
            AddLengthOptions(hydraulicDimensionBox);

            AddOptions(sanitaryFlowBox,
                Option("l/s", FluidFlowUnit.LitersPerSecond),
                Option("m³/h", FluidFlowUnit.CubicMeterPerHour),
                Option("l/h", FluidFlowUnit.LitersPerHour));
            AddFluidPressureOptions(sanitaryPressureBox);
            AddLengthOptions(sanitaryDimensionBox);

            AddOptions(energeticThicknessBox,
                Option("cm", LengthUnit.Centimeter),
                Option("mm", LengthUnit.Millimeter),
                Option("m", LengthUnit.Meter));
            AddOptions(energeticLengthBox,
                Option("m", LengthUnit.Meter),
                Option("cm", LengthUnit.Centimeter));
            AddOptions(energeticPowerBox,
                Option("W", PowerUnit.Watt),
                Option("kW", PowerUnit.Kilowatt));
        }

        private void LoadCurrentValues()
        {
            SelectValue(airFlowBox, UnitContext.Air.Flow);
            SelectValue(airPressureBox, UnitContext.Air.Pressure);
            SelectValue(airDimensionBox, UnitContext.Air.Dimension);

            SelectValue(hydraulicFlowBox, UnitContext.Hydraulics.Flow);
            SelectValue(hydraulicPressureBox, UnitContext.Hydraulics.Pressure);
            SelectValue(hydraulicDimensionBox, UnitContext.Hydraulics.Dimension);

            SelectValue(sanitaryFlowBox, UnitContext.Sanitary.Flow);
            SelectValue(sanitaryPressureBox, UnitContext.Sanitary.Pressure);
            SelectValue(sanitaryDimensionBox, UnitContext.Sanitary.Dimension);

            SelectValue(energeticThicknessBox, UnitContext.Energetics.Thickness);
            SelectValue(energeticLengthBox, UnitContext.Energetics.Length);
            SelectValue(energeticPowerBox, UnitContext.Energetics.HeatFlow);
        }

        private static void AddLengthOptions(EngineeringComboBox comboBox)
        {
            AddOptions(comboBox,
                Option("mm", LengthUnit.Millimeter),
                Option("cm", LengthUnit.Centimeter),
                Option("m", LengthUnit.Meter));
        }

        private static void AddFluidPressureOptions(EngineeringComboBox comboBox)
        {
            AddOptions(comboBox,
                Option("Pa", FluidPressureUnit.Pascal),
                Option("kPa", FluidPressureUnit.Kilopascal),
                Option("bar", FluidPressureUnit.Bar),
                Option("mH₂O", FluidPressureUnit.MeterOfWater));
        }

        private static void AddOptions(EngineeringComboBox comboBox, params UnitOption[] options)
        {
            comboBox.Items.Clear();
            comboBox.Items.AddRange(options);
            if (comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;
        }

        private static UnitOption Option<T>(string display, T value)
            where T : struct, Enum
        {
            return new UnitOption(display, value);
        }

        private static void SelectValue<T>(EngineeringComboBox comboBox, T value)
            where T : struct, Enum
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is UnitOption option &&
                    option.Value is T optionValue &&
                    EqualityComparer<T>.Default.Equals(optionValue, value))
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }
        }

        private static T GetSelectedValue<T>(EngineeringComboBox comboBox, T fallback)
            where T : struct, Enum
        {
            return comboBox.SelectedItem is UnitOption option &&
                option.Value is T value
                    ? value
                    : fallback;
        }

        private void ApplyThemeRecursive(Control control)
        {
            if (control is IThemeable themeable)
                themeable.ApplyTheme(palette);
            else
            {
                control.BackColor = palette.Surface;
                control.ForeColor = palette.TextPrimary;
            }

            foreach (Control child in control.Controls)
                ApplyThemeRecursive(child);
        }

        private sealed class UnitOption
        {
            public UnitOption(string display, object value)
            {
                Display = display;
                Value = value;
            }

            public string Display { get; }
            public object Value { get; }
        }
    }
}
