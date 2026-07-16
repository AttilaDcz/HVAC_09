using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HVACDesigner.Calculations.Common;
using HVACDesigner.CoreUI.Components.Results;
using HVACDesigner.Features.Water;
using HVACDesigner.Services.Export.Common;
using HVACDesigner.Services.Export.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HVACDesigner.Services.Export.Reports.Water
{
    public sealed class WaterReportBuilder : IReportBuilder<WaterReportData>
    {
        public IDocument Build(WaterReportData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(ResolvePageSize(data));
                    page.Margin(36);
                    page.DefaultTextStyle(text => text
                        .FontFamily(ReportStyles.FontFamily)
                        .FontSize(9)
                        .FontColor(ReportStyles.Text));

                    page.Header().Element(container =>
                        ReportHeader.Compose(
                            container,
                            data.Context,
                            "MÉRTÉKADÓ VÍZIGÉNY SZÁMÍTÁSI JEGYZŐKÖNYV"));

                    page.Content().PaddingVertical(14).Column(column =>
                    {
                        if (data.Context.Options.IncludeProjectData)
                            ComposeProjectData(column, data);

                        if (data.Context.Options.IncludeCalculationInputs)
                            ComposeCalculationInputs(column, data);

                        if (data.Context.Options.IncludeResults)
                            ComposeResults(column, data);

                        if (data.Context.Options.IncludeFixtures)
                            ComposeFixtures(column, data);

                        if (data.Context.Options.IncludeStandards)
                            ComposeStandards(column, data);

                        if (data.Context.Options.IncludeNotes)
                            ComposeNotes(column, data);

                        if (data.Context.Options.IncludeSignature)
                            ComposeSignature(column, data);
                    });

                    page.Footer().Element(container =>
                        ReportFooter.Compose(container, data.Context));
                });
            });
        }

        private static PageSize ResolvePageSize(WaterReportData data)
        {
            PageSize size = data.Context.Options.PaperSize == Pdf.PdfPaperSize.A3
                ? PageSizes.A3
                : PageSizes.A4;

            return data.Context.Options.Orientation == Pdf.PdfPageOrientation.Landscape
                ? size.Landscape()
                : size;
        }

        private static void ComposeProjectData(ColumnDescriptor column, WaterReportData data)
        {
            column.Item().Element(container => SectionTitle(container, "Projektadatok"));
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(130);
                    columns.RelativeColumn();
                    columns.ConstantColumn(120);
                    columns.RelativeColumn();
                });

                AddInfoRow(table, "Projekt neve", data.Context.Project.Name, "Rendeltetés", data.BuildingFunction);
                AddInfoRow(table, "Épületprofil", data.BuildingProfile, "Helyrajzi szám", data.Context.Project.TopographicalNumber);
                AddInfoRow(table, "Projekt helye", ProjectAddress(data), "Megrendelő", data.Context.Project.ClientName);

                if (data.Context.Options.IncludeDesignerData)
                {
                    AddInfoRow(table, "Tervező", data.Context.Project.DesignerName, "Cég", data.Context.Project.DesignerCompany);
                    AddInfoRow(table, "Jogosultság", data.Context.Project.EligibilityNumber, "E-mail", data.Context.Project.DesignerEmail);
                }
            });
            column.Item().PaddingBottom(10);
        }

        private static void ComposeCalculationInputs(ColumnDescriptor column, WaterReportData data)
        {
            column.Item().Element(container => SectionTitle(container, "Számítási adatok"));
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(180);
                    columns.RelativeColumn();
                });

                AddPair(table, "Rendeltetés", data.BuildingFunction);
                AddPair(table, "Épületprofil", data.BuildingProfile);
                AddPair(table, data.PrimaryInputLabel, data.PrimaryInputValue);
                AddPair(table, data.SecondaryInputLabel, data.SecondaryInputValue);
                
                if (data.Result.DhwDemand.Status != CalculationStatus.NotApplicable)
                {
                    if (data.Result.DhwDemand.Inputs.TryGetValue("DailyHotWaterRate", out string rate))
                    {
                        string unit = "l/(fő·nap)";
                        if (data.Result.DhwDemand.Result != null)
                        {
                            unit = $"l/({data.Result.DhwDemand.Result.DemandUnitLabel}·nap)";
                        }
                        AddPair(table, "Fajlagos HMV igény", rate + " " + unit);
                    }
                }

                AddPair(table, "Összes ivóvíz terhelési egység", Format(data.Result.PeakDemand.Result?.TotalLoadingUnits));
                AddPair(table, "Összes szennyvíz lefolyási egység", Format(data.Result.Wastewater.Result?.TotalDischargeUnits));

                if (data.Result.RoofDrainage.Status != CalculationStatus.NotApplicable)
                {
                    if (data.Result.RoofDrainage.Inputs.TryGetValue("RainfallIntensity", out string rainfall))
                        AddPair(table, "Mértékadó esőintenzitás", rainfall + " l/(s·m²)");

                    if (data.Result.RoofDrainage.Inputs.TryGetValue("WeightedCatchmentArea", out string area))
                        AddPair(table, "Tényezős tetőfelület", area + " m²");
                }
            });
            column.Item().PaddingBottom(10);
        }

        private static void ComposeResults(ColumnDescriptor column, WaterReportData data)
        {
            column.Item().Element(container => SectionTitle(container, "Eredmények"));
            column.Item().Grid(grid =>
            {
                grid.Columns(2);
                grid.Spacing(8);
                ResultBox(grid, "Napi vízigény", data.Result.DailyDemand.Result?.DailyDemandCubicMetres, "m³/nap");
                ResultBox(grid, "Használati melegvíz", data.Result.DhwDemand.Result?.DailyDhwVolumeCubicMetres, "m³/nap");
                ResultBox(grid, "HMV felmelegítés", data.Result.DhwDemand.Result?.DailyDhwEnergyKwh, "kWh/nap");
                ResultBox(grid, "Mértékadó ivóvízhozam", data.Result.PeakDemand.Result?.DesignFlowLitresPerSecond, "l/s");
                ResultBox(grid, "Mértékadó szennyvízhozam", data.Result.Wastewater.Result?.DesignFlowLitresPerSecond, "l/s");
                ResultBox(grid, "Bekötési minimum", data.Result.Wastewater.Result?.MinimumRequiredDiameter, "DN");

                if (data.Result.Greywater.Status != CalculationStatus.NotApplicable)
                {
                    ResultBox(grid, "Szürkevíz hasznosítható", data.Result.Greywater.Result?.ReusableGreywaterLitresPerDay, "l/nap");
                }

                if (data.Result.RoofDrainage.Status != CalculationStatus.NotApplicable)
                {
                    ResultBox(grid, "Tetővíz mértékadó hozam", data.Result.RoofDrainage.Result?.DesignFlowLitresPerSecond, "l/s");
                }
            });
            column.Item().PaddingBottom(10);
        }

        private static void ComposeFixtures(ColumnDescriptor column, WaterReportData data)
        {
            if (data.Fixtures.Count == 0)
                return;

            column.Item().Element(container => SectionTitle(container, "Berendezések"));
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(58);
                    columns.ConstantColumn(58);
                    columns.ConstantColumn(58);
                    columns.ConstantColumn(58);
                    columns.ConstantColumn(58);
                });

                AddHeader(table, "Szerelvény");
                AddHeader(table, "db");
                AddHeader(table, "LU/db");
                AddHeader(table, "Σ LU");
                AddHeader(table, "DU/db");
                AddHeader(table, "Σ DU");
                AddHeader(table, "Min. DN");

                foreach (WaterReportFixtureRow row in data.Fixtures)
                {
                    AddCell(table, row.Name);
                    AddCell(table, row.Quantity.ToString(CultureInfo.CurrentCulture));
                    AddCell(table, Format(row.PotableLoadingUnit));
                    AddCell(table, Format(row.TotalPotableLoadingUnit));
                    AddCell(table, Format(row.WastewaterUnit));
                    AddCell(table, Format(row.TotalWastewaterUnit));
                    AddCell(table, row.MinimumDn?.ToString(CultureInfo.CurrentCulture) ?? "-");
                }
            });
            column.Item().PaddingBottom(10);
        }

        private static void ComposeStandards(ColumnDescriptor column, WaterReportData data)
        {
            List<string> standards = data.Result.Rules.DailyRule.References
                .Concat(data.Result.Rules.PeakRule.References)
                .Concat(data.Result.Rules.WastewaterRule.References)
                .Concat(data.Result.Rules.GreywaterRule.References)
                .Concat(data.Result.Rules.RoofDrainageRule.References)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            if (standards.Count == 0)
                return;

            column.Item().Element(container => SectionTitle(container, "Alkalmazott szabványok"));
            column.Item().Column(list =>
            {
                foreach (string standard in standards)
                    list.Item().Text("• " + standard).FontSize(9);
            });
            column.Item().PaddingBottom(10);
        }

        private static void ComposeNotes(ColumnDescriptor column, WaterReportData data)
        {
            string notes = data.Context.Options.Notes;
            if (string.IsNullOrWhiteSpace(notes))
                notes = "A számítás a megadott projektadatok és szerelvénylista alapján készült.";

            column.Item().Element(container => SectionTitle(container, "Megjegyzések"));
            column.Item().Text(notes).FontSize(9);
            column.Item().PaddingBottom(10);
        }

        private static void ComposeSignature(ColumnDescriptor column, WaterReportData data)
        {
            string designer = data.Context.Project.DesignerName;
            string eligibility = data.Context.Project.EligibilityNumber;

            column.Item().PaddingTop(24).AlignRight().Column(signature =>
            {
                signature.Item().Width(210).LineHorizontal(1).LineColor(ReportStyles.Text);
                signature.Item().Width(210).AlignCenter().PaddingTop(4).Text(
                    string.IsNullOrWhiteSpace(designer) ? "Tervező" : designer).FontSize(9);
                signature.Item().Width(210).AlignCenter().Text("Gépész tervező").FontSize(8).FontColor(ReportStyles.Muted);

                if (!string.IsNullOrWhiteSpace(eligibility))
                    signature.Item().Width(210).AlignCenter().Text(eligibility).FontSize(8).FontColor(ReportStyles.Muted);
            });
        }

        private static void SectionTitle(IContainer container, string title)
        {
            container.PaddingTop(4).PaddingBottom(5).Text(title)
                .FontFamily(ReportStyles.FontFamily)
                .FontSize(12)
                .Bold()
                .FontColor(ReportStyles.Text);
        }

        private static void ResultBox(GridDescriptor grid, string title, double? value, string unit)
        {
            grid.Item().Border(1).BorderColor(ReportStyles.ResultBorder)
                .Background(ReportStyles.ResultBackground)
                .Padding(10)
                .Column(column =>
                {
                    column.Item().Text(title).FontSize(8).FontColor(ReportStyles.Muted);
                    column.Item().PaddingTop(4).Text(text =>
                    {
                        text.Span(value.HasValue ? Format(value.Value) : "-").FontSize(16).Bold();
                        if (!string.IsNullOrWhiteSpace(unit))
                            text.Span(" " + unit).FontSize(9);
                    });
                });
        }

        private static void ResultBox(GridDescriptor grid, string title, int? value, string unit)
        {
            ResultBox(grid, title, value.HasValue ? (double?)value.Value : null, unit);
        }

        private static void AddInfoRow(TableDescriptor table, string leftLabel, string leftValue, string rightLabel, string rightValue)
        {
            if (!string.IsNullOrWhiteSpace(leftValue))
            {
                AddMutedCell(table, leftLabel);
                AddCell(table, leftValue);
            }
            else
            {
                AddMutedCell(table, "");
                AddCell(table, "");
            }

            if (!string.IsNullOrWhiteSpace(rightValue))
            {
                AddMutedCell(table, rightLabel);
                AddCell(table, rightValue);
            }
            else
            {
                AddMutedCell(table, "");
                AddCell(table, "");
            }
        }

        private static void AddPair(TableDescriptor table, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value))
                return;

            AddMutedCell(table, label);
            AddCell(table, value);
        }

        private static void AddHeader(TableDescriptor table, string text)
        {
            table.Cell().HeaderCell().Text(text).FontSize(8).Bold();
        }

        private static void AddMutedCell(TableDescriptor table, string text)
        {
            table.Cell().BodyCell().Text(text).FontSize(8).FontColor(ReportStyles.Muted);
        }

        private static void AddCell(TableDescriptor table, string text)
        {
            table.Cell().BodyCell().Text(string.IsNullOrWhiteSpace(text) ? "-" : text).FontSize(8);
        }

        private static string ProjectAddress(WaterReportData data)
        {
            return ReportText.FormatAddress(
                data.Context.Project.ProjZipCode,
                data.Context.Project.ProjSettlementName,
                data.Context.Project.ProjStreetName,
                data.Context.Project.ProjStreetType,
                data.Context.Project.ProjHouseNumber,
                data.Context.Project.ProjBuilding,
                data.Context.Project.ProjFloor,
                data.Context.Project.ProjDoorNumber);
        }

        private static string Format(double? value)
        {
            return value.HasValue
                ? value.Value.ToString("0.###", CultureInfo.CurrentCulture)
                : "-";
        }
    }
}
