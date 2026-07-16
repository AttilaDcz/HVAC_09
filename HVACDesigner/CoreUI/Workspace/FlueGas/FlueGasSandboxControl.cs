using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Charts;
using HVACDesigner.CoreUI.Components.Data;
using HVACDesigner.CoreUI.Components.Dialogs;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Components.Help;
using HVACDesigner.CoreUI.Components.Results;
using HVACDesigner.CoreUI.Components.Structural;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Notifications;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.EngineeringData.Importing;
using HVACDesigner.EngineeringData.Rules;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Workspace.FlueGas
{
    // Theme Sandbox 2.0 – kibővített, de működőképes verzió, Mono és ToggleTestTheme nélkül
    public partial class FlueGasSandboxControl : UserControl, IThemeable
    {
        private HVACScrollableContainer _scrollContainer = null!;

        // Tesztpad vezérlőinek magánváltozói
        private TextBox txtSampleNormal = null!, txtSampleActive = null!, txtSampleDisabled = null!;
        private ComboBox cmbSampleNormal = null!;
        private EngineeringCheckBox cbSampleChecked = null!, cbSampleUnchecked = null!;
        private EngineeringButton btnNormal = null!, btnHover = null!, btnAccent = null!, btnDisabled = null!;
        private EngineeringToolTip? _sandboxToolTip;

        public FlueGasSandboxControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            ThemePalette palette = ThemeManager.CurrentPalette;

            _scrollContainer = new HVACScrollableContainer
            {
                Dock = DockStyle.Fill,                     // ⭐ KÖTELEZŐ
                BackColor = palette.Window,                // ⭐ Transparent helyett
                Tag = "NoTheme"
            };

            SuspendLayout();

            Controls.Add(_scrollContainer);

            BackColor = palette.Window;
            ForeColor = palette.TextPrimary;
            Name = "FlueGasSandboxControl";
            Size = new Size(1216, 683);

            ResumeLayout(false);

            RebuildUI();                                   // ⭐ KÖTELEZŐ
        }

        public void ApplyTheme(ThemePalette palette)
        {
            BackColor = palette.Window;
            ForeColor = palette.TextPrimary;

            if (_scrollContainer != null)
            {
                _scrollContainer.ApplyTheme(palette);
                RebuildUI();
            }
        }

        private void RebuildUI()
        {
            ThemePalette palette = ThemeManager.CurrentPalette;
            _sandboxToolTip?.Dispose();
            _sandboxToolTip = new EngineeringToolTip();
            _sandboxToolTip.ApplyTheme(palette);

            Control[] oldControls = new Control[_scrollContainer.ContentControls.Count];
            _scrollContainer.ContentControls.CopyTo(oldControls, 0);
            _scrollContainer.ContentControls.Clear();

            foreach (Control control in oldControls)
            {
                control.Dispose();
            }

            int currentTop = 15;

            HVACSectionPanel cardThemeTop = CreateCard(
                "0. Témaváltás gyors teszt",
                currentTop, 235, palette.Surface);
            _scrollContainer.ContentControls.Add(cardThemeTop);
            AddThemeToggleControls(cardThemeTop, palette);
            AddEngineeringTextBoxSamples(cardThemeTop, palette);

            currentTop += 250;

            // =========================================================================
            // CARD 1: STANDARD BEVITELI VEZÉRLŐK MÁTRIXA
            // =========================================================================
            HVACSectionPanel cardControls = CreateCard(
                "1. Alapvezérlők és Beviteli Állapotok (Surface)",
                currentTop, 225, palette.Surface);
            _scrollContainer.ContentControls.Add(cardControls);

            AddLabelInput(cardControls, "Normál mező (segédszöveggel):",
                20, 40, 160, palette.SurfaceAlt, palette.TextSecondary, out txtSampleNormal);
            txtSampleNormal.Text = "Példa: Ø150 mm";

            AddLabelInput(cardControls, "Aktív / Fókuszált állapot:",
                200, 40, 160, palette.SurfaceAlt, palette.TextSecondary, out txtSampleActive);
            txtSampleActive.Text = "Éppen ide gépel a mérnök...";

            AddLabelInput(cardControls, "Letiltott (Disabled) állapot:",
                380, 40, 160, palette.SurfaceAlt, palette.TextDisabled, out txtSampleDisabled);
            txtSampleDisabled.Text = "Nem szerkeszthető fix érték";
            txtSampleDisabled.Enabled = false;

            Label lblCmb = new Label
            {
                Text = "Legördülő lista (ComboBox):",
                Location = new Point(20, 95),
                Size = new Size(160, 16),
                ForeColor = palette.TextSecondary,
                Font = new Font("Segoe UI", 8.5F),
                UseCompatibleTextRendering = false
            };
            cmbSampleNormal = new ComboBox
            {
                Location = new Point(20, 113),
                Size = new Size(160, 24),
                BackColor = palette.SurfaceAlt,
                ForeColor = palette.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F)
            };
            cmbSampleNormal.Items.AddRange(new object[]
            {
                "Kondenzációs gázkazán",
                "Atmoszférikus kazán",
                "Szilárdtüzelésű berendezés"
            });
            cmbSampleNormal.SelectedIndex = 0;
            cardControls.Controls.AddRange(new Control[] { lblCmb, cmbSampleNormal });

            cbSampleChecked = new EngineeringCheckBox
            {
                Text = "Kijelölt opció",
                Location = new Point(200, 113),
                Size = new Size(160, 26),
                Checked = true
            };
            cbSampleChecked.ApplyTheme(palette);

            cbSampleUnchecked = new EngineeringCheckBox
            {
                Text = "Üres opció",
                Location = new Point(380, 113),
                Size = new Size(160, 26),
                Checked = false
            };
            cbSampleUnchecked.ApplyTheme(palette);
            cardControls.Controls.AddRange(new Control[] { cbSampleChecked, cbSampleUnchecked });

            EngineeringRadioButton radioCircular = new EngineeringRadioButton
            {
                Text = "Kör keresztmetszet",
                Location = new Point(200, 150),
                Size = new Size(170, 26),
                Checked = true
            };
            radioCircular.ApplyTheme(palette);

            EngineeringRadioButton radioRectangular = new EngineeringRadioButton
            {
                Text = "Négyszög keresztmetszet",
                Location = new Point(380, 150),
                Size = new Size(190, 26)
            };
            radioRectangular.ApplyTheme(palette);

            cardControls.Controls.AddRange(new Control[] { radioCircular, radioRectangular });

            currentTop += 240;

            // =========================================================================
            // CARD 2: MÉRNÖKI ADATTÁBLA
            // =========================================================================
            HVACSectionPanel cardTable = CreateCard(
                "2. Mérnöki adattábla (EngineeringDataGridView)",
                currentTop, 255, palette.Surface);
            _scrollContainer.ContentControls.Add(cardTable);
            AddEngineeringTableSample(cardTable, palette);

            currentTop += 270;

            // =========================================================================
            // CARD 3: MÉRNÖKI GRAFIKON
            // =========================================================================
            HVACSectionPanel cardChart = CreateCard(
                "3. Szivattyú-hálózat jelleggörbe (EngineeringChart)",
                currentTop, 375, palette.Surface);
            _scrollContainer.ContentControls.Add(cardChart);
            AddPumpSystemChartSample(cardChart, palette);

            currentTop += 390;

            // =========================================================================
            // CARD 4: MÉRNÖKI EREDMÉNYKÁRTYÁK
            // =========================================================================
            HVACSectionPanel cardResults = CreateCard(
                "4. Mérnöki eredménykártyák (AI rétegekre előkészítve)",
                currentTop, 700, palette.Surface);
            _scrollContainer.ContentControls.Add(cardResults);
            AddEngineeringResultCardSamples(cardResults, palette);

            currentTop += 715;

            // =========================================================================
            // CARD 5: INTERAKTÍV ELEMEK ÉS AKCENTUSOK
            // =========================================================================
            HVACSectionPanel cardButtons = CreateCard(
                "5. Interaktív Gombok és Kattintási Állapotok (Accent)",
                currentTop, 150, palette.Surface);
            _scrollContainer.ContentControls.Add(cardButtons);

            btnNormal = new EngineeringButton
            {
                Text = "Normál funkció",
                Location = new Point(20, 45),
                Size = new Size(120, 30),
                Variant = EngineeringButtonVariant.Secondary
            };
            btnNormal.ApplyTheme(palette);

            btnHover = new EngineeringButton
            {
                Text = "Ikonos művelet",
                Location = new Point(155, 45),
                Size = new Size(130, 30),
                Variant = EngineeringButtonVariant.Secondary,
                IconKind = HvacIconKind.Settings,
                IconPlacement = EngineeringButtonIconPlacement.Left
            };
            btnHover.ApplyTheme(palette);

            btnAccent = new EngineeringButton
            {
                Text = "Fő akció",
                Location = new Point(300, 45),
                Size = new Size(140, 30),
                Variant = EngineeringButtonVariant.Primary,
                IconKind = HvacIconKind.SaveProject,
                IconPlacement = EngineeringButtonIconPlacement.Left
            };
            btnAccent.ApplyTheme(palette);

            btnDisabled = new EngineeringButton
            {
                Text = "Letiltott gomb",
                Location = new Point(455, 45),
                Size = new Size(110, 30),
                Variant = EngineeringButtonVariant.Secondary,
                Enabled = false
            };
            btnDisabled.ApplyTheme(palette);

            EngineeringButton btnGhost = new EngineeringButton
            {
                Text = "Ghost toolbar",
                Location = new Point(20, 88),
                Size = new Size(130, 30),
                Variant = EngineeringButtonVariant.Ghost,
                IconKind = HvacIconKind.Search,
                IconPlacement = EngineeringButtonIconPlacement.Left,
                ShowBorder = false
            };
            btnGhost.ApplyTheme(palette);

            EngineeringButton btnDeleteDanger = new EngineeringButton
            {
                Text = "Törlés",
                Location = new Point(165, 88),
                Size = new Size(110, 30),
                Variant = EngineeringButtonVariant.Danger
            };
            btnDeleteDanger.ApplyTheme(palette);

            EngineeringButton btnIconOnly = new EngineeringButton
            {
                Text = string.Empty,
                Location = new Point(290, 88),
                Size = new Size(34, 30),
                Variant = EngineeringButtonVariant.Ghost,
                IconKind = HvacIconKind.Info,
                IconPlacement = EngineeringButtonIconPlacement.IconOnly,
                ShowBorder = false
            };
            btnIconOnly.ApplyTheme(palette);

            cardButtons.Controls.AddRange(new Control[]
            {
                btnNormal,
                btnHover,
                btnAccent,
                btnDisabled,
                btnGhost,
                btnDeleteDanger,
                btnIconOnly
            });

            currentTop += 165;

            // =========================================================================
            // CARD 6: ENGINEERING DIALOG
            // =========================================================================
            HVACSectionPanel cardDialogs = CreateCard(
                "6. EngineeringDialog minták (üzenet, megerősítés, szerkesztés)",
                currentTop, 150, palette.Surface);
            _scrollContainer.ContentControls.Add(cardDialogs);
            AddEngineeringDialogSamples(cardDialogs, palette);

            currentTop += 165;

            // =========================================================================
            // CARD 7: ENGINEERING TOOLTIP
            // =========================================================================
            HVACSectionPanel cardToolTips = CreateCard(
                "7. EngineeringToolTip minták (rövid súgó, mérnöki magyarázat)",
                currentTop, 150, palette.Surface);
            _scrollContainer.ContentControls.Add(cardToolTips);
            AddEngineeringToolTipSamples(cardToolTips, palette);

            currentTop += 165;

            // =========================================================================
            // CARD 8: ENGINEERING NOTIFICATIONS
            // =========================================================================
            HVACSectionPanel cardNotifications = CreateCard(
                "8. EngineeringNotification minták (toast + EngineeringData adapter)",
                currentTop, 150, palette.Surface);
            _scrollContainer.ContentControls.Add(cardNotifications);
            AddEngineeringNotificationSamples(cardNotifications, palette);

            currentTop += 165;

            // =========================================================================
            // CARD 9: ENGINEERING SLIDER
            // =========================================================================
            HVACSectionPanel cardSlider = CreateCard(
                "9. EngineeringSlider minták (skála, lépték, mértékegység)",
                currentTop, 250, palette.Surface);
            _scrollContainer.ContentControls.Add(cardSlider);
            AddEngineeringSliderSamples(cardSlider, palette);

            currentTop += 265;

            // =========================================================================
            // CARD 10: ENGINEERING TABHOST
            // =========================================================================
            HVACSectionPanel cardTabs = CreateCard(
                "10. EngineeringTabHost minták (fülek, ágak, állapotok)",
                currentTop, 430, palette.Surface);
            _scrollContainer.ContentControls.Add(cardTabs);
            AddEngineeringTabHostSamples(cardTabs, palette);

            currentTop += 445;

            // =========================================================================
            // CARD 11: SEMANTIC COLORS – PANELEK
            // =========================================================================
            HVACSectionPanel cardSemantic = CreateCard(
                "11. Szemantikus Mérnöki Visszajelzések (Riasztási szintek)",
                currentTop, 270, palette.Surface);
            _scrollContainer.ContentControls.Add(cardSemantic);

            Panel pnlSuccess = CreateStatusBlock(cardSemantic, "Sikeres ellenőrzés (Success)", 45, palette.Success);
            AddStatusLabel(pnlSuccess,
                "✓ MEGFELELŐ: A füstgáz áramlási sebessége (4.2 m/s) a megengedett 7.0 m/s határértéken belül van. A rendszer stabil.");

            Panel pnlInfo = CreateStatusBlock(cardSemantic, "Rendszer információ (Info)", 95, palette.Info);
            AddStatusLabel(pnlInfo,
                "ℹ INFORMÁCIÓ: 4 darab Bosch Condens 7000 F kazán közösített égéstermék-elvezetése szoftveresen konfigurálva.");

            Panel pnlWarning = CreateStatusBlock(cardSemantic, "Biztonsági figyelmeztetés (Warning)", 145, palette.Warning);
            AddStatusLabel(pnlWarning,
                "⚠️ FIGYELMEZTETÉS: A természetes huzathatás a kritikus minimum közelében van. Ellenőrizze a hatásos kéménymagasságot!");

            Panel pnlDanger = CreateStatusBlock(cardSemantic, "Kritikus hiba (Danger)", 195, palette.Danger);
            AddStatusLabel(pnlDanger,
                "❌ KRITIKUS HIBÁK: Áramlási torlódás! A választott Ø150 mm-es átmérő elégtelen, a füstgáz visszaáramlás kockázata fennáll!");

            currentTop += 285;

            // =========================================================================
            // CARD 12: SEMANTIC COLORS – GOMBOK
            // =========================================================================
            HVACSectionPanel cardSemanticButtons = CreateCard(
                "12. Szemantikus Gombok (Success / Info / Warning / Danger)",
                currentTop, 110, palette.Surface);
            _scrollContainer.ContentControls.Add(cardSemanticButtons);

            EngineeringButton btnSuccess = CreateSemanticButton("Success", new Point(20, 45), EngineeringButtonVariant.Success);
            EngineeringButton btnInfo = CreateSemanticButton("Info", new Point(155, 45), EngineeringButtonVariant.Info);
            EngineeringButton btnWarning = CreateSemanticButton("Warning", new Point(290, 45), EngineeringButtonVariant.Warning);
            EngineeringButton btnDanger = CreateSemanticButton("Danger", new Point(425, 45), EngineeringButtonVariant.Danger);

            cardSemanticButtons.Controls.AddRange(new Control[] { btnSuccess, btnInfo, btnWarning, btnDanger });

            currentTop += 125;

            // =========================================================================
            // CARD 13: ENGINEERING CARD PANEL
            // =========================================================================
            HVACSectionPanel cardPanelSamples = CreateCard(
                "13. EngineeringCardPanel minták (GroupBox kiváltás)",
                currentTop, 470, palette.Surface);
            _scrollContainer.ContentControls.Add(cardPanelSamples);
            AddEngineeringCardPanelSamples(cardPanelSamples, palette);

            currentTop += 485;

            // =========================================================================
            // CARD 14: THEMEFONTS TESZT
            // =========================================================================
            HVACSectionPanel cardFonts = CreateCard(
                "14. Betűkészletek (ThemeFonts) – Cím, Törzs, Caption",
                currentTop, 160, palette.Surface);
            _scrollContainer.ContentControls.Add(cardFonts);

            AddFontSample(cardFonts, "Title – Mérnöki főcím", 20, 45, ThemeFonts.Title, palette.TextPrimary);
            AddFontSample(cardFonts, "Body – Normál szöveg", 20, 75, ThemeFonts.Body, palette.TextSecondary);
            AddFontSample(cardFonts, "Caption – Kiegészítő információ", 20, 105, ThemeFonts.Caption, palette.TextDisabled);

            // Mono helyett fallback:
            AddFontSample(cardFonts, "Mono – Számítási képletek / kód (fallback Segoe UI Mono)",
                20, 135, new Font("Consolas", 9F), palette.TextPrimary);

            currentTop += 175;

            // =========================================================================
            // CARD 15: SURFACE RÉTEGEK DEMÓ
            // =========================================================================
            HVACSectionPanel cardSurfaces = CreateCard(
                "15. Surface rétegek (Window / Surface / SurfaceAlt / Hover / Accent)",
                currentTop, 140, palette.Surface);
            _scrollContainer.ContentControls.Add(cardSurfaces);

            AddSurfaceBlock(cardSurfaces, "Window", 20, 45, palette.Window);
            AddSurfaceBlock(cardSurfaces, "Surface", 140, 45, palette.Surface);
            AddSurfaceBlock(cardSurfaces, "SurfaceAlt", 260, 45, palette.SurfaceAlt);
            AddSurfaceBlock(cardSurfaces, "SurfaceHover", 380, 45, palette.SurfaceHover);
            AddSurfaceBlock(cardSurfaces, "Accent", 500, 45, palette.Accent);

            currentTop += 155;

            // =========================================================================
            // CARD 16: THEME VÁLTÁS TESZT
            // =========================================================================
            HVACSectionPanel cardThemeChange = CreateCard(
                "16. Téma váltás teszt (ThemeManager)",
                currentTop, 115, palette.Surface);
            _scrollContainer.ContentControls.Add(cardThemeChange);
            AddThemeToggleControls(cardThemeChange, palette);

            _scrollContainer.RecalculateContentLayout();
            _scrollContainer.ScrollToTop();
        }

        private void AddThemeToggleControls(HVACSectionPanel parent, ThemePalette palette)
        {
            string nextThemeText = ThemeManager.CurrentThemeMode == AppThemeMode.Dark
                ? "Világos témára váltás"
                : "Sötét témára váltás";

            EngineeringButton btnThemeTest = new EngineeringButton
            {
                Text = nextThemeText,
                Location = new Point(20, 40),
                Size = new Size(180, 30),
                Variant = EngineeringButtonVariant.Secondary,
                IconKind = HvacIconKind.Settings,
                IconPlacement = EngineeringButtonIconPlacement.Left
            };
            btnThemeTest.ApplyTheme(palette);

            btnThemeTest.Click += (s, e) =>
            {
                BeginInvoke(new Action(() =>
                {
                    ThemeManager.CurrentThemeMode = ThemeManager.CurrentThemeMode == AppThemeMode.Dark
                        ? AppThemeMode.Light
                        : AppThemeMode.Dark;
                }));
            };

            Label lblThemeState = new Label
            {
                Text = $"Aktuális téma: {ThemeManager.CurrentThemeMode}",
                Location = new Point(220, 46),
                Size = new Size(250, 20),
                ForeColor = palette.TextSecondary,
                Font = ThemeFonts.Body,
                UseCompatibleTextRendering = false
            };

            parent.Controls.AddRange(new Control[] { btnThemeTest, lblThemeState });
        }

        private void AddEngineeringTextBoxSamples(HVACSectionPanel parent, ThemePalette palette)
        {
            EngineeringTextBox diameterBox = new EngineeringTextBox
            {
                LabelText = "Átmérő",
                QuantityKind = QuantityKind.AirDimension,
                ValueSi = 0.16,
                MinSi = 0.001,
                MaxSi = 1.2,
                Location = new Point(20, 92),
                Size = new Size(150, 58)
            };

            EngineeringComboBox diameterCatalogBox = new EngineeringComboBox
            {
                LabelText = "Katalógus átmérő [mm]",
                Location = new Point(190, 92),
                Size = new Size(170, 58)
            };
            diameterCatalogBox.Items.AddRange(new object[] { 30, 40, 50, 63, 80, 100, 125, 160, 200 });
            diameterCatalogBox.SelectedItem = 160;

            EngineeringTextBox flowBox = new EngineeringTextBox
            {
                LabelText = "Légmennyiség",
                LabelPosition = EngineeringLabelPosition.Left,
                QuantityKind = QuantityKind.AirFlow,
                ValueSi = 0.125,
                MinSi = 0,
                Location = new Point(20, 178),
                Size = new Size(235, 34)
            };

            EngineeringTextBox pressureBox = new EngineeringTextBox
            {
                LabelVisible = false,
                UnitVisible = false,
                QuantityKind = QuantityKind.AirPressure,
                ValueSi = 42,
                Location = new Point(275, 178),
                Size = new Size(120, 34)
            };

            diameterBox.ApplyTheme(palette);
            diameterCatalogBox.ApplyTheme(palette);
            flowBox.ApplyTheme(palette);
            pressureBox.ApplyTheme(palette);

            parent.Controls.AddRange(new Control[] { diameterBox, diameterCatalogBox, flowBox, pressureBox });
        }

        private void AddEngineeringDialogSamples(HVACSectionPanel parent, ThemePalette palette)
        {
            Label resultLabel = new Label
            {
                Text = "Dialógus eredmény: még nincs művelet.",
                Location = new Point(20, 100),
                Size = new Size(560, 22),
                ForeColor = palette.TextSecondary,
                BackColor = Color.Transparent,
                Font = ThemeFonts.Caption,
                UseCompatibleTextRendering = false
            };

            EngineeringButton infoButton = new EngineeringButton
            {
                Text = "Info üzenet",
                Location = new Point(20, 45),
                Size = new Size(125, 34),
                Variant = EngineeringButtonVariant.Info,
                IconKind = HvacIconKind.Info,
                IconPlacement = EngineeringButtonIconPlacement.Left
            };
            infoButton.ApplyTheme(palette);
            infoButton.Click += (_, _) =>
            {
                EngineeringDialog.ShowMessage(
                    FindForm(),
                    "Számítás mentve",
                    "A mérnöki adatok és a számítási állapot sikeresen frissült.",
                    EngineeringDialogSeverity.Success,
                    HvacIconKind.SaveProject);
                resultLabel.Text = "Dialógus eredmény: info üzenet bezárva.";
            };

            EngineeringButton confirmButton = new EngineeringButton
            {
                Text = "Megerősítés",
                Location = new Point(165, 45),
                Size = new Size(135, 34),
                Variant = EngineeringButtonVariant.Warning,
                IconKind = HvacIconKind.Safety,
                IconPlacement = EngineeringButtonIconPlacement.Left
            };
            confirmButton.ApplyTheme(palette);
            confirmButton.Click += (_, _) =>
            {
                DialogResult result = EngineeringDialog.ShowConfirmation(
                    FindForm(),
                    "Elem módosítása",
                    "Biztosan mented a módosított légcsatorna elemet?",
                    EngineeringDialogSeverity.Warning,
                    HvacIconKind.DuctNetwork);
                resultLabel.Text = $"Dialógus eredmény: megerősítés = {result}.";
            };

            EngineeringButton editorButton = new EngineeringButton
            {
                Text = "Editor minta",
                Location = new Point(320, 45),
                Size = new Size(135, 34),
                Variant = EngineeringButtonVariant.Primary,
                IconKind = HvacIconKind.ProjectProperties,
                IconPlacement = EngineeringButtonIconPlacement.Left
            };
            editorButton.ApplyTheme(palette);
            editorButton.Click += (_, _) =>
            {
                using EngineeringDialog dialog = new EngineeringDialog
                {
                    DialogTitle = "Új füstgáz ág",
                    DialogSubtitle = "Könnyű editor dialógus CoreUI input vezérlőkkel.",
                    IconKind = HvacIconKind.FlueGas,
                    Severity = EngineeringDialogSeverity.Info,
                    ButtonSet = EngineeringDialogButtonSet.SaveCancel,
                    DialogSize = EngineeringDialogSize.Medium,
                    ValidationText = "Minta validációs sáv: a későbbi rule réteg ide adhat rövid jelzést."
                };

                Panel editorPanel = new Panel
                {
                    BackColor = palette.Surface
                };

                EngineeringTextBox branchName = new EngineeringTextBox
                {
                    LabelText = "Ág neve",
                    UnitVisible = false,
                    Text = "Fő gyűjtőkémény",
                    Location = new Point(0, 0),
                    Size = new Size(260, 58)
                };

                EngineeringComboBox materialBox = new EngineeringComboBox
                {
                    LabelText = "Anyag",
                    Location = new Point(0, 78),
                    Size = new Size(220, 58)
                };
                materialBox.Items.AddRange(new object[] { "Rozsdamentes acél", "PPS műanyag", "Kerámia bélés" });
                materialBox.SelectedIndex = 0;

                branchName.ApplyTheme(palette);
                materialBox.ApplyTheme(palette);
                editorPanel.Controls.AddRange(new Control[] { branchName, materialBox });

                dialog.SetContent(editorPanel);

                DialogResult result = dialog.ShowDialog(FindForm());
                resultLabel.Text = result == DialogResult.OK
                    ? $"Dialógus eredmény: mentés | {branchName.Text} | {materialBox.SelectedItem}"
                    : $"Dialógus eredmény: editor = {result}.";
            };

            parent.Controls.AddRange(new Control[] { infoButton, confirmButton, editorButton, resultLabel });
        }

        private void AddEngineeringToolTipSamples(HVACSectionPanel parent, ThemePalette palette)
        {
            EngineeringButton infoHelpButton = new EngineeringButton
            {
                Text = "Rövid súgó",
                Location = new Point(20, 45),
                Size = new Size(130, 34),
                Variant = EngineeringButtonVariant.Info,
                IconKind = HvacIconKind.Info,
                IconPlacement = EngineeringButtonIconPlacement.Left
            };
            infoHelpButton.ApplyTheme(palette);

            EngineeringButton warningHelpButton = new EngineeringButton
            {
                Text = "Figyelmeztető help",
                Location = new Point(170, 45),
                Size = new Size(170, 34),
                Variant = EngineeringButtonVariant.Warning,
                IconKind = HvacIconKind.Safety,
                IconPlacement = EngineeringButtonIconPlacement.Left
            };
            warningHelpButton.ApplyTheme(palette);

            EngineeringTextBox sampleInput = new EngineeringTextBox
            {
                LabelText = "Nyomásveszteség",
                QuantityKind = QuantityKind.AirPressure,
                ValueSi = 86,
                Location = new Point(365, 35),
                Size = new Size(150, 58)
            };
            sampleInput.ApplyTheme(palette);

            Label hintLabel = new Label
            {
                Text = "Vidd az egeret a gombokra vagy a mezőre.",
                Location = new Point(20, 100),
                Size = new Size(520, 22),
                ForeColor = palette.TextSecondary,
                BackColor = Color.Transparent,
                Font = ThemeFonts.Caption,
                UseCompatibleTextRendering = false
            };

            _sandboxToolTip?.SetHelp(
                infoHelpButton,
                "Gyors, rövid magyarázat egy ikonhoz vagy gombhoz. Fejlécben és kártyákban is ezt a motort használjuk.",
                "CoreUI tooltip",
                EngineeringToolTipKind.Info);

            _sandboxToolTip?.SetHelp(
                warningHelpButton,
                "Olyan helyeken hasznos, ahol a művelet nem hibás, de mérnöki figyelmet igényel. A bal jelzőszín a theme Warning színéből jön.",
                "Tervezési figyelmeztetés",
                EngineeringToolTipKind.Warning);

            _sandboxToolTip?.SetHelpRecursive(
                sampleInput,
                "Belső érték SI-ben van tárolva, a kijelzés a QuantityUnitService szerinti bevett épületgépészeti egységet mutatja.",
                "Mértékegységes mező",
                EngineeringToolTipKind.Success);

            parent.Controls.AddRange(new Control[] { infoHelpButton, warningHelpButton, sampleInput, hintLabel });
        }

        private void AddEngineeringNotificationSamples(HVACSectionPanel parent, ThemePalette palette)
        {
            EngineeringButton successButton = new EngineeringButton
            {
                Text = "Siker toast",
                Location = new Point(20, 45),
                Size = new Size(130, 34),
                Variant = EngineeringButtonVariant.Success,
                IconKind = HvacIconKind.Certification,
                IconPlacement = EngineeringButtonIconPlacement.Left
            };
            successButton.ApplyTheme(palette);
            successButton.Click += (_, _) =>
            {
                EngineeringNotificationService.Success(
                    "Számítás kész",
                    "A füstgáz ellenőrzés sikeresen lefutott.");
            };

            EngineeringButton importButton = new EngineeringButton
            {
                Text = "Import warning",
                Location = new Point(170, 45),
                Size = new Size(155, 34),
                Variant = EngineeringButtonVariant.Warning,
                IconKind = HvacIconKind.Import,
                IconPlacement = EngineeringButtonIconPlacement.Left
            };
            importButton.ApplyTheme(palette);
            importButton.Click += (_, _) =>
            {
                DataPackageImportResult sampleResult = CreateSampleImportResult();
                EngineeringNotificationService.ShowMany(
                    EngineeringDataNotificationAdapter.FromImportResult(sampleResult));
            };

            EngineeringButton ruleButton = new EngineeringButton
            {
                Text = "Rule error",
                Location = new Point(345, 45),
                Size = new Size(135, 34),
                Variant = EngineeringButtonVariant.Danger,
                IconKind = HvacIconKind.Safety,
                IconPlacement = EngineeringButtonIconPlacement.Left
            };
            ruleButton.ApplyTheme(palette);
            ruleButton.Click += (_, _) =>
            {
                RulePackageBootstrapResult sampleResult = CreateSampleRuleBootstrapResult();
                EngineeringNotificationService.ShowMany(
                    EngineeringDataNotificationAdapter.FromRuleBootstrapResult(sampleResult));
            };

            Label noteLabel = new Label
            {
                Text = "A második és harmadik gomb EngineeringData eredményből képez magyar toastot.",
                Location = new Point(20, 100),
                Size = new Size(560, 22),
                ForeColor = palette.TextSecondary,
                BackColor = Color.Transparent,
                Font = ThemeFonts.Caption,
                UseCompatibleTextRendering = false
            };

            parent.Controls.AddRange(new Control[] { successButton, importButton, ruleButton, noteLabel });
        }

        private static DataPackageImportResult CreateSampleImportResult()
        {
            var diagnostics = new[]
            {
                new ImportDiagnostic(
                    ImportDiagnosticSeverity.Warning,
                    ImportFailureScope.Property,
                    "IMPORT_PROPERTY_WARNING",
                    "Roughness value is missing, fallback value applied.",
                    "duct-materials",
                    "galvanized-steel",
                    "roughness")
            };

            var contentSet = new ContentSetImportResult(
                "duct-materials",
                24,
                1,
                true,
                diagnostics);

            return new DataPackageImportResult(
                "hvac-base-data",
                "2026.07",
                new[] { contentSet });
        }

        private static RulePackageBootstrapResult CreateSampleRuleBootstrapResult()
        {
            var diagnostics = new[]
            {
                new RulePackageDiagnostic(
                    RulePackageDiagnosticSeverity.Error,
                    "DUPLICATE_RULE_SET",
                    "Duplicate RuleSet key. First file: Data/Xml/rules-a.xml",
                    "Data/Xml/rules-b.xml",
                    "air.velocity.limit@2026.1")
            };

            return new RulePackageBootstrapResult(
                8,
                6,
                1,
                1,
                14,
                4,
                diagnostics);
        }

        private void AddEngineeringSliderSamples(HVACSectionPanel parent, ThemePalette palette)
        {
            Label valueLabel = new Label
            {
                Text = "Aktuális beállítások: sebesség 3,5 m/s | terhelés 35 % | intenzitás 0,030 l/(s·m²)",
                Location = new Point(20, 210),
                Size = new Size(610, 24),
                ForeColor = palette.TextSecondary,
                BackColor = Color.Transparent,
                Font = ThemeFonts.Caption,
                UseCompatibleTextRendering = false
            };

            EngineeringSlider velocitySlider = new EngineeringSlider
            {
                LabelText = "Tervezési légsebesség",
                Location = new Point(20, 45),
                Size = new Size(610, 68),
                Minimum = 0.5,
                Maximum = 12.0,
                Step = 0.1,
                Value = 3.5,
                Decimals = 1,
                UnitLabel = "m/s",
                MajorTickCount = 6,
                SliderMode = EngineeringSliderMode.Continuous,
                ShowTicks = true,
                ShowScaleLabels = true
            };
            velocitySlider.ApplyTheme(palette);

            EngineeringSlider loadSlider = new EngineeringSlider
            {
                LabelText = "Ventilátor terhelés",
                Location = new Point(20, 116),
                Size = new Size(290, 68),
                Minimum = 0,
                Maximum = 100,
                Step = 5,
                Value = 35,
                Decimals = 0,
                UnitLabel = "%",
                MajorTickCount = 5,
                SliderMode = EngineeringSliderMode.Stepped,
                ShowTicks = true,
                ShowScaleLabels = true
            };
            loadSlider.ApplyTheme(palette);

            EngineeringSlider intensitySlider = new EngineeringSlider
            {
                LabelText = "Finom mérnöki tartomány",
                Location = new Point(340, 116),
                Size = new Size(290, 68),
                Minimum = 0.010,
                Maximum = 0.080,
                Step = 0.005,
                Value = 0.030,
                Decimals = 3,
                UnitLabel = "l/(s·m²)",
                MajorTickCount = 4,
                SliderMode = EngineeringSliderMode.Stepped,
                ShowTicks = true,
                ShowScaleLabels = true
            };
            intensitySlider.ApplyTheme(palette);

            void UpdateValueLabel()
            {
                valueLabel.Text =
                    $"Aktuális beállítások: sebesség {velocitySlider.Value:F1} m/s | " +
                    $"terhelés {loadSlider.Value:F0} % | " +
                    $"intenzitás {intensitySlider.Value:F3} l/(s·m²)";
            }

            velocitySlider.ValueChanged += (_, _) => UpdateValueLabel();
            loadSlider.ValueChanged += (_, _) => UpdateValueLabel();
            intensitySlider.ValueChanged += (_, _) => UpdateValueLabel();

            parent.Controls.AddRange(new Control[]
            {
                velocitySlider,
                loadSlider,
                intensitySlider,
                valueLabel
            });
        }

        private void AddEngineeringTabHostSamples(HVACSectionPanel parent, ThemePalette palette)
        {
            EngineeringTabHost smallTabs = new EngineeringTabHost
            {
                Location = new Point(20, 45),
                Size = new Size(285, 320),
                TabStyle = EngineeringTabStyle.Standard,
                OverflowMode = EngineeringTabOverflowMode.Clip
            };
            smallTabs.ApplyTheme(palette);

            smallTabs.Pages.Add(CreatePersistentTab(
                "general",
                "Általános",
                "Téma és autosave",
                HvacIconKind.Settings,
                EngineeringTabSeverity.Info,
                CreateTabContentPanel(
                    palette,
                    "Általános beállítások",
                    "Persistent content: a mezők állapota tabváltás után is megmarad.",
                    "Téma: sötét/világos\nAutosave: 5 perc\nÁllapot: menthető")));

            smallTabs.Pages.Add(CreatePersistentTab(
                "units",
                "Egységek",
                "SI kijelzés",
                HvacIconKind.DuctSizing,
                EngineeringTabSeverity.Success,
                CreateTabContentPanel(
                    palette,
                    "Mértékegységek",
                    "Itt később az EngineeringUnitSelector részei jelenhetnek meg.",
                    "Légmennyiség: m³/h\nNyomás: Pa\nÁtmérő: mm")));

            smallTabs.Pages.Add(CreatePersistentTab(
                "paths",
                "Útvonalak",
                "LocalAppData",
                HvacIconKind.OpenProject,
                EngineeringTabSeverity.Warning,
                CreateTabContentPanel(
                    palette,
                    "Adatútvonalak",
                    "A hosszabb cím tooltipet és ellipsist kap.",
                    "Settings\nAutosave\nUser XML\nExport")));

            EngineeringTabHost branchTabs = new EngineeringTabHost
            {
                Location = new Point(330, 45),
                Size = new Size(500, 320),
                TabStyle = EngineeringTabStyle.Segmented,
                OverflowMode = EngineeringTabOverflowMode.ScrollButtons,
                ShowAddButton = true
            };
            branchTabs.ApplyTheme(palette);

            Panel sharedBranchPanel = CreateBranchSharedPanel(palette, out Label branchTitle, out Label branchDetails);
            branchTabs.ContentHost.Controls.Add(sharedBranchPanel);

            AddBranchTab(branchTabs, new DemoBranchState("main", "Főág", "OK", 600, 86, EngineeringTabSeverity.Success, "Kritikus útvonal, számított össznyomás: 86 Pa."));
            AddBranchTab(branchTabs, new DemoBranchState("b1", "Mellékág 1", "Figyelés", 220, 54, EngineeringTabSeverity.Warning, "Kiegyenlítés szükséges, a veszteség közelít a főághoz."));
            AddBranchTab(branchTabs, new DemoBranchState("b2", "Mellékág 2 - keleti irodatömb hosszú név", "Üres", 0, 0, EngineeringTabSeverity.Info, "Hosszú név ellipsissel, tooltipben teljes névvel."));
            AddBranchTab(branchTabs, new DemoBranchState("b3", "Kazánházi ág", "Kritikus", 420, 134, EngineeringTabSeverity.Danger, "Kritikus veszteség, ellenőrizni kell az idomokat."));
            AddBranchTab(branchTabs, new DemoBranchState("b4", "Tetőtéri ág", "OK", 180, 39, EngineeringTabSeverity.Success, "Rendben, kis veszteségű mellékág."));
            AddBranchTab(branchTabs, new DemoBranchState("b5", "Raktár elszívás", "Info", 310, 61, EngineeringTabSeverity.Info, "Sok tab esetén a fejléc scrollozható."));

            void UpdateBranchInfo(EngineeringTabPage? page)
            {
                if (page?.Model is not DemoBranchState branch)
                    return;

                branchTitle.Text = branch.Name;
                branchDetails.Text =
                    $"Modell alapú tab | Légmennyiség: {branch.FlowM3h:F0} m³/h | Δp: {branch.PressureLossPa:F0} Pa\n{branch.Description}";
                branchTitle.ForeColor = branch.Severity switch
                {
                    EngineeringTabSeverity.Success => palette.Success,
                    EngineeringTabSeverity.Warning => palette.Warning,
                    EngineeringTabSeverity.Danger => palette.Danger,
                    EngineeringTabSeverity.Info => palette.Info,
                    _ => palette.TextPrimary
                };
            }

            branchTabs.SelectedTabChanged += (_, args) => UpdateBranchInfo(args.NewPage);
            branchTabs.AddTabRequested += (_, _) =>
            {
                DialogResult result = ShowBranchCreateDialog(branchTabs);
                if (result == DialogResult.Cancel)
                    return;

                DemoBranchState? selectedBranch = branchTabs.SelectedPage?.Model as DemoBranchState;
                DemoBranchState branch = result == DialogResult.Yes && selectedBranch != null
                    ? CreateCopiedBranch(branchTabs, selectedBranch)
                    : CreateEmptyBranch(branchTabs);

                AddBranchTab(branchTabs, branch);
                branchTabs.SelectedKey = branch.Key;
            };
            branchTabs.TabClosing += (_, args) =>
            {
                if (args.Page.Key == "main")
                    args.Cancel = true;
            };
            branchTabs.SelectedKey = "main";
            UpdateBranchInfo(branchTabs.SelectedPage);

            Label hint = new Label
            {
                Text = "Bal: persistent content. Jobb: shared editor + model; a tab csak ágat választ, a tartalom közös.",
                Location = new Point(20, 375),
                Size = new Size(790, 26),
                ForeColor = palette.TextSecondary,
                BackColor = Color.Transparent,
                Font = ThemeFonts.Caption,
                UseCompatibleTextRendering = false
            };

            parent.Controls.AddRange(new Control[] { smallTabs, branchTabs, hint });
        }

        private DialogResult ShowBranchCreateDialog(EngineeringTabHost branchTabs)
        {
            using EngineeringDialog dialog = new EngineeringDialog
            {
                DialogTitle = "Új mellékág",
                DialogSubtitle = "Válaszd ki, hogy tiszta ágat hozol létre, vagy az aktuális ágat másolod.",
                Severity = EngineeringDialogSeverity.Info,
                IconKind = HvacIconKind.DuctNetwork,
                ButtonSet = EngineeringDialogButtonSet.OkCancel,
                DialogSize = EngineeringDialogSize.Small,
                PrimaryButtonText = "Üres ág",
                SecondaryButtonText = "Mégse",
                ContentPadding = new Padding(22, 12, 22, 12),
                HeaderHeight = 92
            };

            bool canCopy = branchTabs.SelectedPage?.Model is DemoBranchState;
            EngineeringButton copyButton = new EngineeringButton
            {
                Text = "Aktuális ág másolása",
                Variant = EngineeringButtonVariant.Secondary,
                ButtonSize = EngineeringButtonSize.Normal,
                AutoWidth = true,
                Enabled = canCopy,
                Location = new Point(0, 4)
            };
            copyButton.ApplyTheme(ThemeManager.CurrentPalette);
            copyButton.Click += (_, _) =>
            {
                dialog.DialogResult = DialogResult.Yes;
                dialog.Close();
            };

            Label hintLabel = new Label
            {
                Text = canCopy
                    ? "Másoláskor az aktuális ág adatai új azonosítóval és új névvel kerülnek át."
                    : "Nincs kiválasztott másolható ág.",
                Location = new Point(0, 46),
                Size = new Size(360, 44),
                ForeColor = ThemeManager.CurrentPalette.TextSecondary,
                BackColor = Color.Transparent,
                Font = ThemeFonts.Caption,
                UseCompatibleTextRendering = false
            };

            Panel content = new Panel { Dock = DockStyle.Fill };
            content.Controls.Add(copyButton);
            content.Controls.Add(hintLabel);
            dialog.SetContent(content);

            Form? owner = FindForm();
            return owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        }

        private static DemoBranchState CreateEmptyBranch(EngineeringTabHost branchTabs)
        {
            int number = branchTabs.Pages.Count + 1;
            return new DemoBranchState(
                CreateUniqueBranchKey(branchTabs, "new"),
                "Új mellékág " + number,
                "Új",
                0,
                0,
                EngineeringTabSeverity.Info,
                "Tiszta, alapértékes ág. Valós modulban itt jönne létre az üres DuctBranch modell.");
        }

        private static DemoBranchState CreateCopiedBranch(EngineeringTabHost branchTabs, DemoBranchState source)
        {
            return new DemoBranchState(
                CreateUniqueBranchKey(branchTabs, "copy"),
                source.Name + " másolata",
                source.Badge,
                source.FlowM3h,
                source.PressureLossPa,
                source.Severity,
                "Másolat innen: " + source.Name + ". " + source.Description);
        }

        private static string CreateUniqueBranchKey(EngineeringTabHost branchTabs, string prefix)
        {
            int number = branchTabs.Pages.Count + 1;
            string key;
            do
            {
                key = prefix + "-" + number;
                number++;
            }
            while (branchTabs.Pages.Any(page => string.Equals(page.Key, key, StringComparison.OrdinalIgnoreCase)));

            return key;
        }

        private EngineeringTabPage CreatePersistentTab(
            string key,
            string text,
            string badge,
            HvacIconKind icon,
            EngineeringTabSeverity severity,
            Control content)
        {
            return new EngineeringTabPage
            {
                Key = key,
                Text = text,
                BadgeText = badge,
                IconKind = icon,
                Severity = severity,
                Content = content,
                ToolTipText = text + " - persistent content"
            };
        }

        private Panel CreateTabContentPanel(
            ThemePalette palette,
            string title,
            string body,
            string detail)
        {
            Panel panel = new Panel
            {
                BackColor = palette.Surface,
                Padding = new Padding(14)
            };

            Label titleLabel = new Label
            {
                Text = title,
                Location = new Point(14, 16),
                Size = new Size(240, 24),
                ForeColor = palette.TextPrimary,
                BackColor = Color.Transparent,
                Font = ThemeFonts.BodyBold,
                UseCompatibleTextRendering = false
            };

            Label bodyLabel = new Label
            {
                Text = body,
                Location = new Point(14, 48),
                Size = new Size(245, 52),
                ForeColor = palette.TextSecondary,
                BackColor = Color.Transparent,
                Font = ThemeFonts.Caption,
                UseCompatibleTextRendering = false
            };

            TextBox detailBox = new TextBox
            {
                Text = detail,
                Location = new Point(14, 110),
                Size = new Size(240, 120),
                Multiline = true,
                BackColor = palette.SurfaceAlt,
                ForeColor = palette.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = ThemeFonts.Body
            };

            panel.Controls.AddRange(new Control[] { titleLabel, bodyLabel, detailBox });
            return panel;
        }

        private Panel CreateBranchSharedPanel(
            ThemePalette palette,
            out Label titleLabel,
            out Label detailLabel)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = palette.Surface,
                Padding = new Padding(16)
            };

            titleLabel = new Label
            {
                Location = new Point(16, 18),
                Size = new Size(440, 30),
                BackColor = Color.Transparent,
                ForeColor = palette.TextPrimary,
                Font = ThemeFonts.Title,
                UseCompatibleTextRendering = false
            };

            detailLabel = new Label
            {
                Location = new Point(16, 58),
                Size = new Size(440, 70),
                BackColor = Color.Transparent,
                ForeColor = palette.TextSecondary,
                Font = ThemeFonts.Body,
                UseCompatibleTextRendering = false
            };

            EngineeringDataGridView table = new EngineeringDataGridView
            {
                Location = new Point(16, 145),
                Size = new Size(440, 95)
            };
            table.AddTextColumn("colIndex", "#", 34, DataGridViewContentAlignment.MiddleCenter);
            table.AddTextColumn("colElement", "Elem", 155);
            table.AddNumericColumn("colFlow", "qv [m³/h]", 92);
            table.AddNumericColumn("colLoss", "Δp [Pa]", 78);
            table.Rows.Add("1", "Egyenes szakasz", 300d, 18d);
            table.Rows.Add("2", "90° könyök", 300d, 11d);
            table.Rows.Add("3", "Szabályozó", 300d, 27d);
            table.ApplyTheme(palette);

            panel.Controls.AddRange(new Control[] { titleLabel, detailLabel, table });
            return panel;
        }

        private void AddBranchTab(EngineeringTabHost tabs, DemoBranchState branch)
        {
            tabs.Pages.Add(new EngineeringTabPage
            {
                Key = branch.Key,
                Text = branch.Name,
                BadgeText = branch.Badge,
                IconKind = HvacIconKind.DuctNetwork,
                Severity = branch.Severity,
                Model = branch,
                CanClose = branch.Key != "main",
                ToolTipText = branch.Name + " | " + branch.Description
            });
        }

        private sealed class DemoBranchState
        {
            public DemoBranchState(
                string key,
                string name,
                string badge,
                double flowM3h,
                double pressureLossPa,
                EngineeringTabSeverity severity,
                string description)
            {
                Key = key;
                Name = name;
                Badge = badge;
                FlowM3h = flowM3h;
                PressureLossPa = pressureLossPa;
                Severity = severity;
                Description = description;
            }

            public string Key { get; }
            public string Name { get; }
            public string Badge { get; }
            public double FlowM3h { get; }
            public double PressureLossPa { get; }
            public EngineeringTabSeverity Severity { get; }
            public string Description { get; }
        }

        private void AddEngineeringTableSample(HVACSectionPanel parent, ThemePalette palette)
        {
            EngineeringDataGridView table = new EngineeringDataGridView
            {
                Location = new Point(20, 45),
                Size = new Size(535, 165),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            table.Columns.Clear();
            table.AddTextColumn("colIndex", "#", 34, DataGridViewContentAlignment.MiddleCenter);
            table.AddTextColumn("colName", "Elem", 150);
            table.AddTextColumn("colSize", "Méret", 78, DataGridViewContentAlignment.MiddleCenter);
            table.AddNumericColumn("colFlow", "Légmenny. [m³/h]", 92);
            table.AddNumericColumn("colVelocity", "v [m/s]", 72);
            table.AddNumericColumn("colLoss", "Δp [Pa]", 74);
            table.AddTextColumn("colState", "Állapot", 92, DataGridViewContentAlignment.MiddleCenter);

            table.SetColumnEditable("colFlow", true);
            table.SetNumericFormat("colFlow", 0);
            table.SetNumericFormat("colVelocity", 1);
            table.SetNumericFormat("colLoss", 0);

            int row0 = table.Rows.Add("1", "Kondenzációs kazán bekötés", "Ø160", 420d, 4.2d, 18d, "OK");
            int row1 = table.Rows.Add("2", "90° könyök, préselt", "Ø160", 420d, 4.2d, 11d, "OK");
            int row2 = table.Rows.Add("3", "Közösítő idom", "Ø200", 840d, 7.4d, 36d, "Figyelés");
            int row3 = table.Rows.Add("4", "Függőleges gyűjtőkémény", "Ø200", 840d, 8.1d, 64d, "Kritikus");

            table.SetRowState(table.Rows[row0], EngineeringTableRowState.Success);
            table.SetRowState(table.Rows[row1], EngineeringTableRowState.Info);
            table.SetRowState(table.Rows[row2], EngineeringTableRowState.Warning);
            table.SetRowState(table.Rows[row3], EngineeringTableRowState.Danger);

            table.SetRowModel(table.Rows[row0], "fg-boiler-connection");
            table.SetRowModel(table.Rows[row1], "fg-elbow-90");
            table.SetRowModel(table.Rows[row2], "fg-collector");
            table.SetRowModel(table.Rows[row3], "fg-vertical-stack");

            table.ApplyTheme(palette);

            table.Rows[row0].Cells["colState"].Style.ForeColor = palette.Success;
            table.Rows[row1].Cells["colState"].Style.ForeColor = palette.Info;
            table.Rows[row2].Cells["colState"].Style.ForeColor = palette.Warning;
            table.Rows[row3].Cells["colState"].Style.ForeColor = palette.Danger;

            Label tableHint = new Label
            {
                Text = "Szerkeszthető minta: légmennyiség oszlop. Duplakatt: sorhoz kötött szerkesztési esemény.",
                Location = new Point(22, 216),
                Size = new Size(530, 20),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                AutoEllipsis = true,
                ForeColor = palette.TextSecondary,
                BackColor = Color.Transparent,
                Font = ThemeFonts.Caption,
                UseCompatibleTextRendering = false
            };

            table.RowEditRequested += (_, args) =>
            {
                string elementName = Convert.ToString(args.Row.Cells["colName"].Value) ?? "ismeretlen elem";
                string modelId = Convert.ToString(args.RowModel) ?? "nincs modellazonosító";
                tableHint.Text = $"Duplakatt: {elementName} | modell: {modelId}";
            };

            parent.Controls.AddRange(new Control[] { table, tableHint });
        }

        private void AddPumpSystemChartSample(HVACSectionPanel parent, ThemePalette palette)
        {
            EngineeringChart chart = new EngineeringChart
            {
                Location = new Point(20, 45),
                Size = new Size(640, 300),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                ChartTitle = "Szivattyú és hálózat jelleggörbe - munkaponttal",
                LegendPlacement = EngineeringChartLegendPlacement.Right
            };

            chart.AddAxis("flow", "Térfogatáram", "m3/h", EngineeringChartAxisPosition.Bottom, 0, 30)
                .LabelFormat = "0";
            chart.Axes[0].MajorTickInterval = 5;

            chart.AddAxis("head", "Emelőmagasság", "m", EngineeringChartAxisPosition.Left, 0, 40)
                .LabelFormat = "0";
            chart.Axes[1].MajorTickInterval = 5;

            chart.AddAxis("efficiency", "Hatásfok", "%", EngineeringChartAxisPosition.Right, 0, 90)
                .LabelFormat = "0";
            chart.Axes[2].MajorTickInterval = 15;

            EngineeringChartSeries pumpCurve = chart.AddSeries(
                "Szivattyú H-Q",
                "flow",
                "head",
                new[]
                {
                    new EngineeringChartPoint(0, 34),
                    new EngineeringChartPoint(5, 32),
                    new EngineeringChartPoint(10, 28),
                    new EngineeringChartPoint(15, 23),
                    new EngineeringChartPoint(20, 17),
                    new EngineeringChartPoint(25, 10),
                    new EngineeringChartPoint(30, 4)
                });
            pumpCurve.Color = palette.Accent;
            pumpCurve.StrokeWidth = 2.4f;

            EngineeringChartSeries trimmedPumpCurve = chart.AddSeries(
                "Szivattyú H-Q, alacsonyabb fordulat",
                "flow",
                "head",
                new[]
                {
                    new EngineeringChartPoint(0, 26),
                    new EngineeringChartPoint(5, 24.5),
                    new EngineeringChartPoint(10, 21),
                    new EngineeringChartPoint(15, 16.5),
                    new EngineeringChartPoint(20, 11),
                    new EngineeringChartPoint(25, 5)
                });
            trimmedPumpCurve.Color = palette.Accent;
            trimmedPumpCurve.LinePattern = EngineeringChartLinePattern.Dash;
            trimmedPumpCurve.StrokeWidth = 2f;

            EngineeringChartSeries systemCurve = chart.AddSeries(
                "Hálózat jelleggörbe",
                "flow",
                "head",
                new[]
                {
                    new EngineeringChartPoint(0, 4),
                    new EngineeringChartPoint(5, 5.2),
                    new EngineeringChartPoint(10, 8),
                    new EngineeringChartPoint(15, 12.8),
                    new EngineeringChartPoint(20, 19),
                    new EngineeringChartPoint(25, 27),
                    new EngineeringChartPoint(30, 37)
                });
            systemCurve.Color = palette.Warning;
            systemCurve.StrokeWidth = 2.2f;

            EngineeringChartSeries throttledSystemCurve = chart.AddSeries(
                "Fojtott hálózat",
                "flow",
                "head",
                new[]
                {
                    new EngineeringChartPoint(0, 7),
                    new EngineeringChartPoint(5, 8.8),
                    new EngineeringChartPoint(10, 13),
                    new EngineeringChartPoint(15, 20),
                    new EngineeringChartPoint(20, 29),
                    new EngineeringChartPoint(25, 39)
                });
            throttledSystemCurve.Color = palette.Warning;
            throttledSystemCurve.LinePattern = EngineeringChartLinePattern.DashDot;

            EngineeringChartSeries efficiencyCurve = chart.AddSeries(
                "Hatásfok",
                "flow",
                "efficiency",
                new[]
                {
                    new EngineeringChartPoint(0, 12),
                    new EngineeringChartPoint(5, 38),
                    new EngineeringChartPoint(10, 62),
                    new EngineeringChartPoint(15, 78),
                    new EngineeringChartPoint(20, 74),
                    new EngineeringChartPoint(25, 55),
                    new EngineeringChartPoint(30, 28)
                });
            efficiencyCurve.Color = palette.Success;
            efficiencyCurve.LinePattern = EngineeringChartLinePattern.Dot;
            efficiencyCurve.StrokeWidth = 2f;

            EngineeringChartWorkingPoint workingPoint = chart.AddWorkingPoint(
                "Munkapont",
                "flow",
                "head",
                18.2,
                18.0);
            workingPoint.Color = palette.Danger;
            workingPoint.Label = "M";

            chart.ApplyTheme(palette);
            parent.Controls.Add(chart);
        }

        private void AddEngineeringResultCardSamples(HVACSectionPanel parent, ThemePalette palette)
        {
            EngineeringResultCard velocityCard = CreateResultCard(
                20,
                45,
                new EngineeringResultCardModel(
                    "Légsebesség",
                    "4.2",
                    "m/s",
                    EngineeringResultStatus.Success,
                    subtitle: "Főág számított értéke",
                    limitText: "max. 7.0 m/s",
                    sourceText: "Előkészített rule adapter: air.velocity.limit",
                    recommendationText: "Nincs beavatkozási igény.",
                    aiLevel: EngineeringAiSupportLevel.RuleCheck,
                    diagnostics: new[]
                    {
                        new EngineeringResultDiagnostic(
                            "AIR-V-OK",
                            "A légsebesség az ajánlott tartományban van.",
                            EngineeringResultDiagnosticSeverity.Info)
                    },
                    references: new[]
                    {
                        new EngineeringResultReference("Tervezési profil", "komfort légtechnika")
                    }),
                palette);

            EngineeringResultCard pressureCard = CreateResultCard(
                290,
                45,
                new EngineeringResultCardModel(
                    "Nyomásveszteség",
                    "86",
                    "Pa",
                    EngineeringResultStatus.Warning,
                    subtitle: "Szakasz összesített vesztesége",
                    limitText: "figyelési küszöb: 75 Pa",
                    sourceText: "Számítási trace + későbbi szabálycsomag",
                    recommendationText: "Ellenőrizd a könyökök számát és a választott átmérőt.",
                    aiLevel: EngineeringAiSupportLevel.Recommendation,
                    diagnostics: new[]
                    {
                        new EngineeringResultDiagnostic(
                            "AIR-DP-WARN",
                            "A veszteség közelíti az előzetes figyelési küszöböt.",
                            EngineeringResultDiagnosticSeverity.Warning)
                    },
                    references: new[]
                    {
                        new EngineeringResultReference("Belső mérnöki ajánlás", "légcsatorna hálózat")
                    }),
                palette);

            EngineeringResultCard backflowCard = CreateResultCard(
                20,
                380,
                new EngineeringResultCardModel(
                    "Visszaáramlási kockázat",
                    "Magas",
                    string.Empty,
                    EngineeringResultStatus.Danger,
                    subtitle: "Füstgáz biztonsági ellenőrzés",
                    limitText: "kritikus huzathiány",
                    sourceText: "Későbbi szabvány/rule réteghez előkészítve",
                    recommendationText: "Növeld az átmérőt vagy ellenőrizd a hatásos magasságot.",
                    aiLevel: EngineeringAiSupportLevel.Recommendation,
                    diagnostics: new[]
                    {
                        new EngineeringResultDiagnostic(
                            "FG-BACKFLOW",
                            "A választott kialakítás visszaáramlási kockázatot jelez.",
                            EngineeringResultDiagnosticSeverity.Error)
                    },
                    references: new[]
                    {
                        new EngineeringResultReference("Füstgáz tervezési ellenőrzés", "biztonsági réteg")
                    }),
                palette);

            EngineeringResultCard assistantCard = CreateResultCard(
                290,
                380,
                new EngineeringResultCardModel(
                    "AI mérnöki asszisztens",
                    "3",
                    "javaslat",
                    EngineeringResultStatus.Info,
                    subtitle: "Jövőbeli mélyebb értelmezési réteg",
                    limitText: "nem automatikus döntés",
                    sourceText: "AI L3 csak validált számítási és szabályadatból dolgozhat",
                    recommendationText: "Részletekben később indoklás, alternatíva és kockázat jelenhet meg.",
                    aiLevel: EngineeringAiSupportLevel.Assistant,
                    diagnostics: new[]
                    {
                        new EngineeringResultDiagnostic(
                            "AI-L3-READY",
                            "A kártya modellje felkészült későbbi AI magyarázatra.",
                            EngineeringResultDiagnosticSeverity.Info)
                    },
                    references: new[]
                    {
                        new EngineeringResultReference("CalculationTrace", "RuleReference adapter")
                    }),
                palette);

            parent.Controls.AddRange(new Control[]
            {
                velocityCard,
                pressureCard,
                backflowCard,
                assistantCard
            });
        }

        private EngineeringResultCard CreateResultCard(
            int left,
            int top,
            EngineeringResultCardModel model,
            ThemePalette palette)
        {
            EngineeringResultCard card = new EngineeringResultCard
            {
                Location = new Point(left, top),
                Size = new Size(255, 164),
                Model = model
            };

            card.ApplyTheme(palette);
            return card;
        }

        private HVACSectionPanel CreateCard(string title, int topY, int height, Color cardBg)
        {
            return new HVACSectionPanel
            {
                SectionTitle = title,
                Location = new Point(15, topY),
                Size = new Size(575, height),
                BackColor = cardBg,
                ForeColor = ThemeManager.CurrentPalette.TextPrimary,
                Padding = new Padding(15, 40, 15, 10)
            };
        }

        private void AddLabelInput(HVACSectionPanel parent, string labelText, int leftX, int topY,
                                   int width, Color inputBg, Color textClr, out TextBox targetTextBox)
        {
            Label lbl = new Label
            {
                Text = labelText,
                Location = new Point(leftX, topY),
                Size = new Size(width, 16),
                ForeColor = textClr,
                Font = new Font("Segoe UI", 8.5F),
                UseCompatibleTextRendering = false
            };
            targetTextBox = new TextBox
            {
                Location = new Point(leftX, topY + 18),
                Size = new Size(width, 24),
                BackColor = inputBg,
                ForeColor = ThemeManager.CurrentPalette.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F)
            };
            parent.Controls.AddRange(new Control[] { lbl, targetTextBox });
        }

        private Panel CreateStatusBlock(HVACSectionPanel parent, string title, int topY, Color statusColor)
        {
            Panel pnl = new Panel
            {
                Location = new Point(20, topY),
                Size = new Size(535, 42),
                BackColor = ThemeManager.CurrentPalette.SurfaceAlt,
                BorderStyle = BorderStyle.None
            };

            Panel pnlIndicator = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(4, 42),
                BackColor = statusColor
            };
            pnl.Controls.Add(pnlIndicator);

            parent.Controls.Add(pnl);
            return pnl;
        }

        private void AddStatusLabel(Panel parent, string text)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new Point(12, 4),
                Size = new Size(515, 34),
                BackColor = parent.BackColor,
                ForeColor = ThemeManager.CurrentPalette.TextPrimary,
                Font = new Font("Segoe UI", 9F),
                UseCompatibleTextRendering = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
            parent.Controls.Add(lbl);
        }

        private EngineeringButton CreateSemanticButton(
            string text,
            Point location,
            EngineeringButtonVariant variant)
        {
            EngineeringButton btn = new EngineeringButton
            {
                Text = text,
                Location = location,
                Size = new Size(120, 30),
                Variant = variant
            };
            btn.ApplyTheme(ThemeManager.CurrentPalette);

            return btn;
        }

        private void AddEngineeringCardPanelSamples(HVACSectionPanel parent, ThemePalette palette)
        {
            EngineeringCardPanel inputCard = new EngineeringCardPanel
            {
                Title = "Bemeneti adatok",
                Subtitle = "Egyszerű, ikon nélküli adatcsoport",
                Location = new Point(20, 45),
                Size = new Size(260, 150),
                Status = EngineeringCardStatus.None,
                ShowIcon = false,
                ShowStatusBadge = false,
                ShowHeaderActions = false
            };
            inputCard.ApplyTheme(palette);
            AddCardSampleText(inputCard.ContentPanel, "Átmérő", "160 mm", 12);
            AddCardSampleText(inputCard.ContentPanel, "Térfogatáram", "450 m³/h", 45);

            EngineeringCardPanel ruleCard = new EngineeringCardPanel
            {
                Title = "Szabályellenőrzés",
                Subtitle = "AI L1/L2 fogadására előkészítve",
                Location = new Point(300, 45),
                Size = new Size(330, 190),
                Status = EngineeringCardStatus.Warning,
                IconKind = HvacIconKind.Safety,
                ShowIcon = true,
                ShowStatusBadge = true,
                FooterText = "Forrás: későbbi rule réteg / szabványkapcsolat",
                ShowFooter = true,
                ContentToolTipText = "A nyomásveszteség közelít az előzetes figyelési küszöbhöz. A kártya státusza, bal csíkja és badge-e együtt változik."
            };
            ruleCard.ApplyTheme(palette);
            ruleCard.AddHeaderAction(HvacIconKind.Info, "Részletek");
            ruleCard.AddHeaderAction(HvacIconKind.Search, "Trace keresése");
            AddCardBodyLabel(
                ruleCard.ContentPanel,
                "A nyomásveszteség közelít az előzetes figyelési küszöbhöz. A kártya státusza, bal csíkja és badge-e együtt változik.");

            EngineeringCardPanel collapsedCard = new EngineeringCardPanel
            {
                Title = "Összecsukható részletek",
                Subtitle = "Hosszú trace vagy opcionális beállítások",
                Location = new Point(20, 255),
                Size = new Size(300, 160),
                Status = EngineeringCardStatus.Info,
                IconKind = HvacIconKind.ProjectProperties,
                ShowIcon = true,
                ShowStatusBadge = true,
                IsCollapsible = true,
                ContentToolTipText = "Kattintás a kártya fejlécére: tartalom ki/be. Ez később jól jön részletes számítási trace-ekhez."
            };
            collapsedCard.ApplyTheme(palette);
            AddCardBodyLabel(
                collapsedCard.ContentPanel,
                "Kattintás a kártya fejlécére: tartalom ki/be. Ez később jól jön részletes számítási trace-ekhez.");

            EngineeringCardPanel flatCard = new EngineeringCardPanel
            {
                Title = string.Empty,
                Location = new Point(340, 255),
                Size = new Size(290, 145),
                Variant = EngineeringCardVariant.Flat,
                ShowHeader = false,
                ShowBorder = true,
                ShowSeparator = false,
                ShowAccentStrip = false
            };
            flatCard.ApplyTheme(palette);
            AddCardBodyLabel(
                flatCard.ContentPanel,
                "Header nélküli / flat variáns: kisebb, beágyazott felülethez vagy kiegészítő adatsávhoz.");

            parent.Controls.AddRange(new Control[]
            {
                inputCard,
                ruleCard,
                collapsedCard,
                flatCard
            });
        }

        private void AddCardSampleText(Panel parent, string label, string value, int top)
        {
            ThemePalette palette = ThemeManager.CurrentPalette;
            Label lbl = new Label
            {
                Text = label,
                Location = new Point(0, top),
                Size = new Size(120, 18),
                BackColor = parent.BackColor,
                ForeColor = palette.TextSecondary,
                Font = ThemeFonts.Caption,
                UseCompatibleTextRendering = false
            };

            Label val = new Label
            {
                Text = value,
                Location = new Point(125, top),
                Size = new Size(105, 18),
                BackColor = parent.BackColor,
                ForeColor = palette.TextPrimary,
                Font = ThemeFonts.BodyBold,
                TextAlign = ContentAlignment.MiddleRight,
                UseCompatibleTextRendering = false
            };

            parent.Controls.AddRange(new Control[] { lbl, val });
        }

        private void AddCardBodyLabel(Panel parent, string text)
        {
            ThemePalette palette = ThemeManager.CurrentPalette;
            Label label = new Label
            {
                Text = text,
                Location = new Point(0, 10),
                Size = new Size(Math.Max(120, parent.Width - 4), Math.Max(64, parent.Height - 14)),
                BackColor = parent.BackColor,
                ForeColor = palette.TextSecondary,
                Font = ThemeFonts.Caption,
                AutoEllipsis = true,
                UseCompatibleTextRendering = false
            };
            parent.Controls.Add(label);

            if (parent.Parent is EngineeringCardPanel card)
                card.RefreshContentLayout();
        }

        private void AddFontSample(HVACSectionPanel parent, string text, int leftX, int topY,
                                   Font font, Color color)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new Point(leftX, topY),
                Size = new Size(540, 20),
                ForeColor = color,
                Font = font,
                UseCompatibleTextRendering = false
            };
            parent.Controls.Add(lbl);
        }

        private void AddSurfaceBlock(HVACSectionPanel parent, string label, int leftX, int topY, Color bg)
        {
            Panel pnl = new Panel
            {
                Location = new Point(leftX, topY),
                Size = new Size(100, 40),
                BackColor = bg,
                BorderStyle = BorderStyle.None
            };

            Label lbl = new Label
            {
                Text = label,
                Location = new Point(5, 12),
                Size = new Size(90, 16),
                BackColor = bg,
                ForeColor = GetReadableTextColor(bg),
                Font = new Font("Segoe UI", 8.5F),
                UseCompatibleTextRendering = false,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnl.Controls.Add(lbl);
            parent.Controls.Add(pnl);
        }

        private Color GetReadableTextColor(Color background)
        {
            double luminance =
                (0.299 * background.R +
                 0.587 * background.G +
                 0.114 * background.B) / 255.0;

            return luminance > 0.58
                ? Color.FromArgb(26, 32, 44)
                : Color.White;
        }
    }
}
