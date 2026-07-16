using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Data;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Components.Structural;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.EngineeringData;
using HVACDesigner.EngineeringData.SimpleCatalogs;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Workspace.EngineeringData
{
    public sealed class EngineeringDataSandboxControl : UserControl, IThemeable
    {
        private readonly HVACScrollableContainer scrollContainer;
        private ThemePalette palette = ThemeManager.CurrentPalette;

        public EngineeringDataSandboxControl()
        {
            Dock = DockStyle.Fill;

            scrollContainer = new HVACScrollableContainer
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(scrollContainer);
            ApplyTheme(palette);
            BuildContent();
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Window;
            scrollContainer.ApplyTheme(palette);
            ThemeApplicator.ApplyTheme(scrollContainer, palette);
            Invalidate(true);
        }

        private void BuildContent()
        {
            scrollContainer.ContentControls.Clear();

            int y = 18;
            AddOverviewCards(ref y);
            AddRegistryTable(ref y);
            AddFixtureTable(ref y);
            AddMaterialTable(ref y);
            AddDiagnosticsTable(ref y);

            scrollContainer.RecalculateContentLayout();
            scrollContainer.ScrollToTop();
        }

        private void AddOverviewCards(ref int y)
        {
            EngineeringDataBootstrapResult result = ServiceLocator.EngineeringData;
            int registrationCount = ServiceLocator.EngineeringDataRegistry.GetRegistrations().Count;
            int diagnosticCount = result.Diagnostics.Count;
            int errorCount = result.Diagnostics.Count(item =>
                item.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase));

            EngineeringCardPanel registryCard = CreateCard(
                "EngineeringData bootstrap",
                "XML katalógusok és szabálycsomagok runtime állapota",
                result.Succeeded ? EngineeringCardStatus.Success : EngineeringCardStatus.Warning,
                HvacIconKind.Import,
                new Point(18, y),
                new Size(365, 158));

            registryCard.ContentPanel.Controls.Add(CreateMetricLabel(
                "Regisztrált adatkészletek",
                registrationCount.ToString(),
                new Point(18, 14),
                result.Succeeded ? palette.Success : palette.Warning));
            registryCard.ContentPanel.Controls.Add(CreateMetricLabel(
                "Diagnosztikai üzenetek",
                diagnosticCount + " db, hibák: " + errorCount,
                new Point(18, 72),
                errorCount == 0 ? palette.TextPrimary : palette.Danger));

            EngineeringCardPanel ruleCard = CreateCard(
                "Rule package állapot",
                "rules-*.xml fájlok betöltési összesítője",
                result.RuleResult.Succeeded ? EngineeringCardStatus.Success : EngineeringCardStatus.Warning,
                HvacIconKind.Safety,
                new Point(402, y),
                new Size(365, 158));

            ruleCard.ContentPanel.Controls.Add(CreateMetricLabel(
                "RuleSet / DesignMethod",
                result.RuleResult.RegisteredRuleSetCount + " / " +
                result.RuleResult.RegisteredDesignMethodCount,
                new Point(18, 14),
                palette.Info));
            ruleCard.ContentPanel.Controls.Add(CreateMetricLabel(
                "Fájlok",
                "betöltve: " + result.RuleResult.LoadedFileCount +
                ", kihagyva: " + result.RuleResult.SkippedFileCount +
                ", hibás: " + result.RuleResult.FailedFileCount,
                new Point(18, 72),
                result.RuleResult.FailedFileCount == 0 ? palette.TextPrimary : palette.Danger));

            scrollContainer.ContentControls.Add(registryCard);
            scrollContainer.ContentControls.Add(ruleCard);
            y += 178;
        }

        private void AddRegistryTable(ref int y)
        {
            EngineeringCardPanel card = CreateCard(
                "Registry tartalom",
                "A bootstrap által regisztrált XML/adatkészletek",
                EngineeringCardStatus.Info,
                HvacIconKind.Info,
                new Point(18, y),
                new Size(749, 270));

            EngineeringDataGridView table = CreateTable(new Point(18, 20), new Size(705, 180));
            table.AddTextColumn("content", "ContentSet", 180);
            table.AddTextColumn("version", "Verzió", 70, DataGridViewContentAlignment.MiddleCenter);
            table.AddTextColumn("kind", "Típus", 115);
            table.AddTextColumn("records", "Rekord", 74, DataGridViewContentAlignment.MiddleRight);
            table.AddTextColumn("model", "Model", 220);

            foreach (var registration in ServiceLocator.EngineeringDataRegistry.GetRegistrations()
                .OrderBy(item => item.ContentSetId, StringComparer.OrdinalIgnoreCase))
            {
                table.Rows.Add(
                    registration.ContentSetId,
                    registration.Version,
                    registration.ContentKind.ToString(),
                    ResolveRecordCount(registration.Value).ToString(),
                    registration.RecordType.Name);
            }

            card.ContentPanel.Controls.Add(table);
            scrollContainer.ContentControls.Add(card);
            y += 290;
        }

        private void AddFixtureTable(ref int y)
        {
            EngineeringCardPanel card = CreateCard(
                "catalog-fixtures.xml",
                "Vizes berendezések próbaolvasása SimpleCatalog<FixtureDefinition> alapján",
                EngineeringCardStatus.Success,
                HvacIconKind.SanitaryWater,
                new Point(18, y),
                new Size(749, 285));

            EngineeringDataGridView table = CreateTable(new Point(18, 20), new Size(705, 195));
            table.AddTextColumn("id", "Id", 135);
            table.AddTextColumn("name", "Megnevezés", 170);
            table.AddTextColumn("category", "Kategória", 105);
            table.AddNumericColumn("lu", "LU", 55);
            table.AddNumericColumn("du", "DU", 55);
            table.AddTextColumn("dn", "DN", 55, DataGridViewContentAlignment.MiddleRight);

            if (ServiceLocator.EngineeringDataRegistry.TryGet(
                "Catalog.Fixtures",
                "1.0",
                out SimpleCatalog<FixtureDefinition>? fixtures))
            {
                foreach (FixtureDefinition fixture in fixtures.Items.Values.Take(12))
                {
                    table.Rows.Add(
                        fixture.Id,
                        fixture.DisplayName,
                        fixture.Category,
                        fixture.PotableLoadingUnit,
                        fixture.WastewaterDu,
                        fixture.MinimumWasteDn);
                }
            }
            else
            {
                table.Rows.Add("missing", "Catalog.Fixtures@1.0 nem érhető el", "", "", "", "");
            }

            card.ContentPanel.Controls.Add(table);
            scrollContainer.ContentControls.Add(card);
            y += 305;
        }

        private void AddMaterialTable(ref int y)
        {
            EngineeringCardPanel card = CreateCard(
                "catalog-materials.xml",
                "Építőanyag-katalógus próbaolvasása",
                EngineeringCardStatus.Info,
                HvacIconKind.BuildingEnergy,
                new Point(18, y),
                new Size(749, 255));

            EngineeringDataGridView table = CreateTable(new Point(18, 20), new Size(705, 165));
            table.AddTextColumn("id", "Id", 155);
            table.AddTextColumn("name", "Megnevezés", 260);
            table.AddNumericColumn("lambda", "lambda [W/mK]", 120);
            table.AddNumericColumn("density", "rho [kg/m3]", 120);

            if (ServiceLocator.EngineeringDataRegistry.TryGet(
                "Catalog.Materials",
                "1.0",
                out SimpleCatalog<MaterialDefinition>? materials))
            {
                foreach (MaterialDefinition material in materials.Items.Values.Take(10))
                {
                    table.Rows.Add(
                        material.Id,
                        material.DisplayName,
                        material.Lambda,
                        material.Density);
                }
            }
            else
            {
                table.Rows.Add("missing", "Catalog.Materials@1.0 nem érhető el", "", "");
            }

            card.ContentPanel.Controls.Add(table);
            scrollContainer.ContentControls.Add(card);
            y += 275;
        }

        private void AddDiagnosticsTable(ref int y)
        {
            EngineeringCardPanel card = CreateCard(
                "Betöltési diagnosztika",
                "A bootstrap üzenetei, warning/error ellenőrzéshez",
                ServiceLocator.EngineeringData.Succeeded ? EngineeringCardStatus.Success : EngineeringCardStatus.Warning,
                HvacIconKind.Search,
                new Point(18, y),
                new Size(749, 315));

            EngineeringDataGridView table = CreateTable(new Point(18, 20), new Size(705, 225));
            table.AddTextColumn("level", "Szint", 80);
            table.AddTextColumn("message", "Üzenet", 580);

            foreach (string diagnostic in ServiceLocator.EngineeringData.Diagnostics)
            {
                string level = ResolveDiagnosticLevel(diagnostic);
                int rowIndex = table.Rows.Add(level, diagnostic);
                table.SetRowState(table.Rows[rowIndex], ResolveDiagnosticState(level));
            }

            card.ContentPanel.Controls.Add(table);
            scrollContainer.ContentControls.Add(card);
            y += 335;
        }

        private EngineeringCardPanel CreateCard(
            string title,
            string subtitle,
            EngineeringCardStatus status,
            HvacIconKind icon,
            Point location,
            Size size)
        {
            var card = new EngineeringCardPanel
            {
                Title = title,
                Subtitle = subtitle,
                Status = status,
                IconKind = icon,
                ShowIcon = true,
                ShowStatusBadge = true,
                Location = location,
                Size = size
            };
            card.ApplyTheme(palette);
            return card;
        }

        private EngineeringDataGridView CreateTable(Point location, Size size)
        {
            var table = new EngineeringDataGridView
            {
                Location = location,
                Size = size,
                AllowMouseWheelBubbleAtEdges = false
            };
            table.ApplyTheme(palette);
            return table;
        }

        private Label CreateMetricLabel(
            string caption,
            string value,
            Point location,
            Color valueColor)
        {
            var label = new Label
            {
                Text = caption + Environment.NewLine + value,
                Location = location,
                Size = new Size(315, 48),
                ForeColor = palette.TextSecondary,
                BackColor = Color.Transparent,
                Font = ThemeFonts.Caption,
                UseCompatibleTextRendering = false
            };
            label.Paint += (_, args) =>
            {
                TextRenderer.DrawText(
                    args.Graphics,
                    value,
                    ThemeFonts.Subtitle,
                    new Rectangle(0, 19, label.Width, 26),
                    valueColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding);
            };
            return label;
        }

        private static int ResolveRecordCount(object value)
        {
            return value switch
            {
                SimpleCatalog<FixtureDefinition> catalog => catalog.Items.Count,
                SimpleCatalog<MaterialDefinition> catalog => catalog.Items.Count,
                SimpleCatalog<OpeningDefinition> catalog => catalog.Items.Count,
                SimpleCatalog<BuildingFunctionDefinition> catalog => catalog.Items.Count,
                SimpleCatalog<BuildingProfileDefinition> catalog => catalog.Items.Count,
                SimpleCatalog<BuildingFunctionMapping> catalog => catalog.Items.Count,
                SimpleCatalog<ClimateRegionDefinition> catalog => catalog.Items.Count,
                SimpleCatalog<EngineeringDictionaryEntry> catalog => catalog.Items.Count,
                _ => 1
            };
        }

        private static string ResolveDiagnosticLevel(string diagnostic)
        {
            if (diagnostic.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
                return "ERROR";
            if (diagnostic.StartsWith("WARNING:", StringComparison.OrdinalIgnoreCase))
                return "WARNING";
            return "INFO";
        }

        private static EngineeringTableRowState ResolveDiagnosticState(string level)
        {
            return level switch
            {
                "ERROR" => EngineeringTableRowState.Danger,
                "WARNING" => EngineeringTableRowState.Warning,
                _ => EngineeringTableRowState.Info
            };
        }
    }
}
