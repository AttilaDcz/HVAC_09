using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HVACDesigner.Data.Models.Duct;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.CoreUI.Workspace.Air;

namespace HVACDesigner.CoreUI.Workspace.Air
{
    public partial class DuctElementPanel : UserControl
    {
        private readonly DuctNetworkController _controller;
        private DuctBranch? _currentBranch;
        private IReadOnlyList<DuctMaterial> _materials = Array.Empty<DuctMaterial>();
        private bool _isEditMode = false;
        private bool _isUpdatingUi = false;
        private string _computedName = "Új elem";

        private CheckBox chkAddAdditionalAirflow = null!;

        public DuctElementPanel(DuctNetworkController controller)
        {
            InitializeComponent();
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));

            chkAddAdditionalAirflow = new CheckBox
            {
                Text = "Plusz légmennyiség / Elágazás",
                Location = new Point(285, 216),
                AutoSize = true,
                ForeColor = Color.White,
                Visible = false
            };
            chkAddAdditionalAirflow.CheckedChanged += (s, e) => {
                if (!_isUpdatingUi) { txtAirflow.Enabled = chkAddAdditionalAirflow.Checked; }
            };
            cardGeometry.Controls.Add(chkAddAdditionalAirflow);
            this.BackColor = ThemeManager.CurrentPalette.Window;
            RegisterUiEvents();
            LoadXmlSizesAndMaterials();

            this.Load += DuctElementPanel_Load;
            this.ParentChanged += DuctElementPanel_ParentChanged;
        }

        public void SetCurrentBranch(DuctBranch branch)
        {
            _currentBranch = branch;

            if (_currentBranch != null && !_isEditMode)
            {
                ApplyMemoryCache();
            }

            UpdateCalculatedAirflowDisplay();
        }

        private void RegisterUiEvents()
        {
            rbCircular.CheckedChanged += CrossSection_CheckedChanged;
            rbRectangular.CheckedChanged += CrossSection_CheckedChanged;
            cmbCategory.SelectedIndexChanged += cmbCategory_SelectedIndexChanged;
            cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;
            cmbLossCalculationMode.SelectedIndexChanged += cmbLossCalculationMode_SelectedIndexChanged;

            cmbInletD.SelectedIndexChanged += (s, e) => { if (!_isUpdatingUi && SzimmetrikusIdomE()) cmbOutletD.SelectedIndex = cmbInletD.SelectedIndex; };
            cmbInletW.SelectedIndexChanged += (s, e) => { if (!_isUpdatingUi && SzimmetrikusIdomE()) cmbOutletW.SelectedIndex = cmbInletW.SelectedIndex; };
            cmbInletH.SelectedIndexChanged += (s, e) => { if (!_isUpdatingUi && SzimmetrikusIdomE()) cmbOutletH.SelectedIndex = cmbInletH.SelectedIndex; };

            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;
        }

        private bool SzimmetrikusIdomE()
        {
            string cat = cmbCategory.SelectedItem?.ToString() ?? "";
            string type = cmbType.SelectedItem?.ToString() ?? "";
            return cat == "Könyök / Idom" && !type.ToLower().Contains("átmenet") && !type.ToLower().Contains("bővítő") && !type.ToLower().Contains("szűkítő");
        }

        private void DuctElementPanel_ParentChanged(object? sender, EventArgs e)
        {
            if (this.ParentForm is Form parentForm)
            {
                parentForm.ClientSize = new System.Drawing.Size(this.Width, this.Height);
                parentForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                parentForm.MaximizeBox = false;
                parentForm.MinimizeBox = false;
                parentForm.StartPosition = FormStartPosition.CenterParent;
            }
        }

        private void LoadXmlSizesAndMaterials()
        {
            _isUpdatingUi = true;

            cmbCategory.Items.Clear();
            cmbCategory.Items.AddRange(new object[] {
                "Csőszakasz",
                "Könyök / Idom",
                "Hálózati tartozék (Zsalu, Hangcsillapító)",
                "Végpont (Anemosztát, Rács)",
                "T-idom / Elágazás (Összegző)"
            });
            cmbCategory.SelectedIndex = 0;

            cmbLossCalculationMode.Items.Clear();
            cmbLossCalculationMode.Items.AddRange(new object[] {
                "Zeta tényező alapú",
                "Gyártói fix nyomásesés (Δp)"
            });
            cmbLossCalculationMode.SelectedIndex = 0;

            cmbMaterialOverride.Items.Clear();
            cmbMaterialOverride.Items.Add("--- Globális hálózati anyag ---");
            try
            {
                _materials = _controller.DataProvider.GetMaterials();
                foreach (var mat in _materials) cmbMaterialOverride.Items.Add(mat.Name);
            }
            catch { }
            cmbMaterialOverride.SelectedIndex = 0;

            try
            {
                cmbInletD.Items.Clear(); cmbOutletD.Items.Clear(); cmbBranchD.Items.Clear(); cmbBranch2D.Items.Clear();
                foreach (var s in _controller.DataProvider.GetCircularDuctSizes())
                {
                    cmbInletD.Items.Add(s.Diameter); cmbOutletD.Items.Add(s.Diameter); cmbBranchD.Items.Add(s.Diameter); cmbBranch2D.Items.Add(s.Diameter);
                }

                cmbInletW.Items.Clear(); cmbOutletW.Items.Clear(); cmbBranchW.Items.Clear(); cmbBranch2W.Items.Clear();
                cmbInletH.Items.Clear(); cmbOutletH.Items.Clear(); cmbBranchH.Items.Clear(); cmbBranch2H.Items.Clear();
                var rectSizes = _controller.DataProvider.GetRectangularDuctSizes();
                var widths = rectSizes.Select(r => r.Width).Distinct().OrderBy(w => w).ToList();
                var heights = rectSizes.Select(r => r.Height).Distinct().OrderBy(h => h).ToList();

                foreach (var w in widths) { cmbInletW.Items.Add(w); cmbOutletW.Items.Add(w); cmbBranchW.Items.Add(w); cmbBranch2W.Items.Add(w); }
                foreach (var h in heights) { cmbInletH.Items.Add(h); cmbOutletH.Items.Add(h); cmbBranchH.Items.Add(h); cmbBranch2H.Items.Add(h); }

                SetComboBoxDefaultIndexes();
            }
            catch { }

            _isUpdatingUi = false;
        }

        private void SetComboBoxDefaultIndexes()
        {
            if (cmbInletD.Items.Count > 0 && cmbInletD.SelectedIndex == -1) { cmbInletD.SelectedIndex = 0; cmbOutletD.SelectedIndex = 0; cmbBranchD.SelectedIndex = 0; cmbBranch2D.SelectedIndex = 0; }
            if (cmbInletW.Items.Count > 0 && cmbInletW.SelectedIndex == -1) { cmbInletW.SelectedIndex = 0; cmbInletH.SelectedIndex = 0; cmbOutletW.SelectedIndex = 0; cmbOutletH.SelectedIndex = 0; cmbBranchW.SelectedIndex = 0; cmbBranchH.SelectedIndex = 0; cmbBranch2W.SelectedIndex = 0; cmbBranch2H.SelectedIndex = 0; }
        }

        private void DuctElementPanel_Load(object? sender, EventArgs e)
        {
            if (_controller.LastAddedElement != null)
            {
                // ============================================
                // 1. SZERKESZTÉS (EDIT) ÜZEMMÓD INDÍTÁSA
                // ============================================
                _isEditMode = true;
                var target = _controller.LastAddedElement;

                _computedName = target.Name;
                txtAirflow.Text = target.Airflow.ToString("F0");

                if (target is DuctSegment segment) txtLength.Text = segment.Length.ToString("F2", CultureInfo.InvariantCulture);
                else if (target is DuctTransition trans) txtLength.Text = trans.Length.ToString("F2", CultureInfo.InvariantCulture);

                _isUpdatingUi = true;

                if (target is DuctSegment) cmbCategory.SelectedIndex = 0;
                else if (target is BranchFitting) cmbCategory.SelectedIndex = 4;
                else if (target is DuctLouver louver && louver.Category == "Terminál") cmbCategory.SelectedIndex = 3;
                else if (target is DuctLouver) cmbCategory.SelectedIndex = 2;
                else cmbCategory.SelectedIndex = 1;

                // UI frissítés feloldása, hogy a listák kényszerítve legenerálódjanak
                _isUpdatingUi = false;
                cmbCategory_SelectedIndexChanged(null, EventArgs.Empty);
                _isUpdatingUi = true;

                string rawCleanName = target.Name.Split('-')[0].Trim();
                if (cmbType.Items.Contains(rawCleanName)) cmbType.SelectedItem = rawCleanName;

                if (target.Geometry != null)
                {
                    rbCircular.Checked = target.Geometry.Shape == GeometryShape.Circular;
                    rbRectangular.Checked = target.Geometry.Shape != GeometryShape.Circular;

                    if (cmbInletD.Items.Contains(target.Geometry.InletDiameter)) cmbInletD.SelectedItem = target.Geometry.InletDiameter;
                    if (cmbOutletD.Items.Contains(target.Geometry.OutletDiameter)) cmbOutletD.SelectedItem = target.Geometry.OutletDiameter;
                    if (cmbBranchD.Items.Contains(target.Geometry.BranchDiameter)) cmbBranchD.SelectedItem = target.Geometry.BranchDiameter;

                    if (cmbInletW.Items.Contains(target.Geometry.InletWidth)) cmbInletW.SelectedItem = target.Geometry.InletWidth;
                    if (cmbInletH.Items.Contains(target.Geometry.InletHeight)) cmbInletH.SelectedItem = target.Geometry.InletHeight;
                    if (cmbOutletW.Items.Contains(target.Geometry.OutletWidth)) cmbOutletW.SelectedItem = target.Geometry.OutletWidth;
                    if (cmbOutletH.Items.Contains(target.Geometry.OutletHeight)) cmbOutletH.SelectedItem = target.Geometry.OutletHeight;
                    if (cmbBranchW.Items.Contains(target.Geometry.BranchWidth)) cmbBranchW.SelectedItem = target.Geometry.BranchWidth;
                    if (cmbBranchH.Items.Contains(target.Geometry.BranchHeight)) cmbBranchH.SelectedItem = target.Geometry.BranchHeight;
                }

                if (target is BranchFitting branchFitting)
                {
                    chkAddAdditionalAirflow.Checked = true;
                    txtAirflow.Text = branchFitting.BranchAirflow.ToString("F0");
                }
                _isUpdatingUi = false;

                btnSave.Text = "Mentés";
            }
            else
            {
                // ============================================
                // 2. ÚJ HOZZÁADÁS ÜZEMMÓD INDÍTÁSA
                // ============================================
                _isEditMode = false;
                btnSave.Text = "Hozzáadás";
                chkAddAdditionalAirflow.Checked = false;

                _isUpdatingUi = false;
                cmbCategory_SelectedIndexChanged(null, EventArgs.Empty);

                // ÚJ: Ha van elmentett előző geometria a kontrollerben, azt automatikusan rákényszerítjük a felületre
                if (_controller.LastUsedGeometry != null)
                {
                    _isUpdatingUi = true;
                    var geo = _controller.LastUsedGeometry;

                    rbCircular.Checked = (geo.Shape == GeometryShape.Circular);
                    rbRectangular.Checked = (geo.Shape != GeometryShape.Circular);

                    if (geo.Shape == GeometryShape.Circular)
                    {
                        if (cmbInletD.Items.Contains(geo.InletDiameter)) cmbInletD.SelectedItem = geo.InletDiameter;
                        if (cmbOutletD.Items.Contains(geo.OutletDiameter)) cmbOutletD.SelectedItem = geo.OutletDiameter;
                        if (cmbBranchD.Items.Contains(geo.BranchDiameter)) cmbBranchD.SelectedItem = geo.BranchDiameter;
                    }
                    else
                    {
                        if (cmbInletW.Items.Contains(geo.InletWidth)) cmbInletW.SelectedItem = geo.InletWidth;
                        if (cmbInletH.Items.Contains(geo.InletHeight)) cmbInletH.SelectedItem = geo.InletHeight;
                        if (cmbOutletW.Items.Contains(geo.OutletWidth)) cmbOutletW.SelectedItem = geo.OutletWidth;
                        if (cmbOutletH.Items.Contains(geo.OutletHeight)) cmbOutletH.SelectedItem = geo.OutletHeight;
                        if (cmbBranchW.Items.Contains(geo.BranchWidth)) cmbBranchW.SelectedItem = geo.BranchWidth;
                        if (cmbBranchH.Items.Contains(geo.BranchHeight)) cmbBranchH.SelectedItem = geo.BranchHeight;
                    }
                    _isUpdatingUi = false;
                }
            }

            FrissitMezokLathatosagat();
        }

        private void CrossSection_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingUi) return;
            cmbCategory_SelectedIndexChanged(null, EventArgs.Empty);
        }

        private void cmbCategory_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbCategory.SelectedItem == null) return;

            bool oldState = _isUpdatingUi;
            _isUpdatingUi = true;

            cmbType.Items.Clear();
            string cat = cmbCategory.SelectedItem?.ToString() ?? string.Empty;
            bool isCircular = rbCircular.Checked;

            if (cat == "Csőszakasz")
            {
                if (isCircular)
                {
                    cmbType.Items.Add("Kör horganyzott légcsatorna");
                    cmbType.Items.Add("Flexibilis kör légcsatorna");
                    cmbType.Items.Add("Félmerev PE kör cső");
                }
                else
                {
                    cmbType.Items.Add("Négyszög horganyzott légcsatorna");
                    cmbType.Items.Add("Légkezelő rezgéscsillapító csatlakozó");
                }
            }
            else if (cat == "Könyök / Idom")
            {
                if (isCircular)
                {
                    foreach (var f in _controller.DataProvider.GetCircularFittings()) cmbType.Items.Add(f.Name);
                    foreach (var t in _controller.DataProvider.GetTransitionFittings().Where(t => t.Name.StartsWith("Kör"))) cmbType.Items.Add(t.Name);
                }
                else
                {
                    foreach (var r in _controller.DataProvider.GetRectangularFittings()) cmbType.Items.Add(r.Name);
                    foreach (var t in _controller.DataProvider.GetTransitionFittings().Where(t => t.Name.StartsWith("Négyszög"))) cmbType.Items.Add(t.Name);
                }
            }
            else if (cat == "Hálózati tartozék (Zsalu, Hangcsillapító)")
            {
                var accessories = _controller.DataProvider.GetDuctAccessories()
                    .Where(a => a.Category != "Louver" && a.Category != "Grille" && a.Category != "Diffuser" && a.Category != "RoofCap" && a.Category != "Hood");
                foreach (var a in accessories) cmbType.Items.Add(a.Name);
            }
            else if (cat == "Végpont (Anemosztát, Rács)")
            {
                var terminals = _controller.DataProvider.GetDuctAccessories()
                    .Where(a => a.Category == "Louver" || a.Category == "Grille" || a.Category == "Diffuser" || a.Category == "RoofCap" || a.Category == "Hood");
                foreach (var t in terminals) cmbType.Items.Add(t.Name);
            }
            else if (cat == "T-idom / Elágazás (Összegző)")
            {
                foreach (var b in _controller.DataProvider.GetBranchFittings()) cmbType.Items.Add(b.Name);
            }

            // PONTOS TÍPUS KÉNYSZERÍTETT KIVÁLASZTÁSA: Így az indításkor SOHA nem marad üresen a cella!
            if (cmbType.Items.Count > 0) cmbType.SelectedIndex = 0;

            _isUpdatingUi = oldState;

            cmbType_SelectedIndexChanged(null, EventArgs.Empty);
        }

        private void cmbType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbType.SelectedItem == null) return;

            string name = cmbType.SelectedItem?.ToString() ?? string.Empty;
            _computedName = name;
            double zeta = 0.0;

            var circFit = _controller.DataProvider.GetCircularFittings().FirstOrDefault(f => f.Name == name);
            if (circFit != null) zeta = circFit.DefaultZeta;

            var rectFit = _controller.DataProvider.GetRectangularFittings().FirstOrDefault(f => f.Name == name);
            if (rectFit != null) zeta = rectFit.DefaultZeta;

            var branch = _controller.DataProvider.GetBranchFittings().FirstOrDefault(b => b.Name == name);
            if (branch != null) zeta = branch.DefaultZeta;

            var acc = _controller.DataProvider.GetDuctAccessories().FirstOrDefault(a => a.Name == name);
            if (acc != null) zeta = acc.DefaultZeta;

            var trans = _controller.DataProvider.GetTransitionFittings().FirstOrDefault(t => t.Name == name);
            if (trans != null) zeta = trans.DefaultZeta;

            txtZeta.Text = zeta.ToString("F2", CultureInfo.InvariantCulture);

            FrissitMezokLathatosagat();
        }

        private void cmbLossCalculationMode_SelectedIndexChanged(object? sender, EventArgs e)
        {
            FrissitMezokLathatosagat();
        }

        private void FrissitMezokLathatosagat()
        {
            bool isCircular = rbCircular.Checked;
            string cat = cmbCategory.SelectedItem?.ToString() ?? "";
            string typeName = cmbType.SelectedItem?.ToString() ?? "";

            bool isDuct = (cat == "Csőszakasz");
            bool isFitting = (cat == "Könyök / Idom");
            bool isBranch = (cat == "T-idom / Elágazás (Összegző)");

            bool isTransition = typeName.ToLower().Contains("átmenet") || typeName.ToLower().Contains("szűkítő") || typeName.ToLower().Contains("bővítő");
            bool isCross = typeName.ToLower().Contains("kereszt") || typeName.ToLower().Contains("cross");

            bool isKörNégyszögÁtmenet = isTransition && typeName.StartsWith("Kör") && typeName.Contains("Négyszög");
            bool isNégyszögKörÁtmenet = isTransition && typeName.StartsWith("Négyszög") && typeName.Contains("Kör");

            // 1. BELÉPŐ OLDAL
            if (isNégyszögKörÁtmenet)
            {
                cmbInletD.Visible = false; cmbInletW.Visible = true; lblX1.Visible = true; cmbInletH.Visible = true;
            }
            else
            {
                cmbInletD.Visible = isCircular; cmbInletW.Visible = !isCircular; lblX1.Visible = !isCircular; cmbInletH.Visible = !isCircular;
            }

            // 2. KILÉPŐ OLDAL
            bool showOutlet = isFitting || isTransition || isBranch;
            lblOutletSize.Visible = showOutlet;
            lblOutletUnit.Visible = showOutlet;

            if (isKörNégyszögÁtmenet)
            {
                cmbOutletD.Visible = false; cmbOutletW.Visible = true; lblX2.Visible = true; cmbOutletH.Visible = true;
                cmbOutletW.Enabled = true; cmbOutletH.Enabled = true;
            }
            else if (isNégyszögKörÁtmenet)
            {
                cmbOutletD.Visible = true; cmbOutletW.Visible = false; lblX2.Visible = false; cmbOutletH.Visible = false;
                cmbOutletD.Enabled = true;
            }
            else
            {
                cmbOutletD.Visible = showOutlet && isCircular;
                cmbOutletW.Visible = showOutlet && !isCircular;
                lblX2.Visible = showOutlet && !isCircular;
                cmbOutletH.Visible = showOutlet && !isCircular;

                bool blockOutlet = isFitting && !isTransition;
                cmbOutletD.Enabled = !blockOutlet;
                cmbOutletW.Enabled = !blockOutlet;
                cmbOutletH.Enabled = !blockOutlet;
            }

            // 3. MELLÉKÁG 1 OLDAL
            lblBranchSize.Visible = isBranch || isCross;
            lblBranchUnit.Visible = isBranch || isCross;
            cmbBranchD.Visible = (isBranch || isCross) && isCircular;
            cmbBranchW.Visible = (isBranch || isCross) && !isCircular;
            lblX3.Visible = (isBranch || isCross) && !isCircular;
            cmbBranchH.Visible = (isBranch || isCross) && !isCircular;

            // 4. MELLÉKÁG 2 OLDAL
            lblBranch2Size.Visible = false; lblBranch2Unit.Visible = false; cmbBranch2D.Visible = false; cmbBranch2W.Visible = false; lblX4.Visible = false; cmbBranch2H.Visible = false;

            // 5. INTELIGENS HOSSZ FELIRAT
            if (isDuct || isTransition)
            {
                lblLength.Text = "Átmenet hossza:";
                txtLength.Enabled = true;
            }
            else if (!isCircular && isFitting)
            {
                lblLength.Text = "Idom szárhossza:";
                txtLength.Enabled = true;
            }
            else
            {
                lblLength.Text = "Hossz:";
                txtLength.Text = "0.00";
                txtLength.Enabled = false;
            }

            // 6. ELLENÁLLÁS CSOPORT
            bool allowFixedPa = (cat == "Hálózati tartozék (Zsalu, Hangcsillapító)" || cat == "Végpont (Anemosztát, Rács)");
            bool showLossGroup = !isDuct;

            lblLossMode.Visible = showLossGroup && allowFixedPa;
            cmbLossCalculationMode.Visible = showLossGroup && allowFixedPa;

            if (!allowFixedPa && cmbLossCalculationMode.Items.Count > 0)
            {
                cmbLossCalculationMode.SelectedIndex = 0;
            }
            bool isFixedMode = allowFixedPa && cmbLossCalculationMode.SelectedIndex == 1;

            lblZeta.Visible = showLossGroup && !isFixedMode;
            txtZeta.Visible = showLossGroup && !isFixedMode;
            lblFixedDeltaP.Visible = showLossGroup && isFixedMode;
            txtFixedPressureDrop.Visible = showLossGroup && isFixedMode;
            lblPascalUnit.Visible = showLossGroup && isFixedMode;

            // 7. TÉRFOGATÁRAMOK SZINKRONJA
            lblAirflow2.Visible = false; txtAirflow2.Visible = false; lblAirflow2Unit.Visible = false;

            if (isBranch)
            {
                chkAddAdditionalAirflow.Visible = true;
                lblAirflow.Text = "Mellékág légm.:";
                txtAirflow.Enabled = chkAddAdditionalAirflow.Checked;
            }
            else if (cat == "Végpont (Anemosztát, Rács)")
            {
                chkAddAdditionalAirflow.Visible = false;
                lblAirflow.Text = "Légmennyiség:";
                txtAirflow.Enabled = true;
            }
            else
            {
                chkAddAdditionalAirflow.Visible = false;
                lblAirflow.Text = "Légmennyiség:";
                txtAirflow.Enabled = false;
            }

            UpdateCalculatedAirflowDisplay();
        }

        private void ApplyMemoryCache()
        {
            var cache = _controller.LastAddedElement;
            if (cache == null || cache.Geometry == null) return;

            _isUpdatingUi = true;
            rbCircular.Checked = cache.Geometry.Shape == GeometryShape.Circular;
            rbRectangular.Checked = cache.Geometry.Shape != GeometryShape.Circular;

            if (cache.Geometry.Shape == GeometryShape.Circular)
            {
                int targetD = cache.Geometry.OutletDiameter > 0 ? cache.Geometry.OutletDiameter : cache.Geometry.InletDiameter;
                if (cmbInletD.Items.Contains(targetD)) cmbInletD.SelectedItem = targetD;
                if (cmbOutletD.Items.Contains(targetD)) cmbOutletD.SelectedItem = targetD;
            }
            else
            {
                int targetW = cache.Geometry.OutletWidth > 0 ? cache.Geometry.OutletWidth : cache.Geometry.InletWidth;
                int targetH = cache.Geometry.OutletHeight > 0 ? cache.Geometry.OutletHeight : cache.Geometry.InletHeight;
                if (cmbInletW.Items.Contains(targetW)) cmbInletW.SelectedItem = targetW;
                if (cmbInletH.Items.Contains(targetH)) cmbInletH.SelectedItem = targetH;
                if (cmbOutletW.Items.Contains(targetW)) cmbOutletW.SelectedItem = targetW;
                if (cmbOutletH.Items.Contains(targetH)) cmbOutletH.SelectedItem = targetH;
            }
            _isUpdatingUi = false;
        }

        private double CalculateCurrentBaseAirflow()
        {
            if (_currentBranch == null) return 0;

            double baseAirflow = 0;
            var terminal = _currentBranch.Elements.FirstOrDefault(e => e.Category == "Terminál");
            if (terminal != null) baseAirflow = terminal.Airflow;
            else if (_currentBranch.Elements.Count > 0) baseAirflow = _currentBranch.Elements[0].Airflow;

            double addedAirflow = _currentBranch.Elements.OfType<BranchFitting>().Sum(b => b.BranchAirflow);
            return baseAirflow + addedAirflow;
        }

        private void UpdateCalculatedAirflowDisplay()
        {
            if (_currentBranch == null || _isEditMode) return;
            if (chkAddAdditionalAirflow.Visible && chkAddAdditionalAirflow.Checked) return;
            if (cmbCategory.SelectedItem?.ToString() == "Végpont (Anemosztát, Rács)") return;

            txtAirflow.Text = CalculateCurrentBaseAirflow().ToString("F0");
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            // TŰPONTOS NULL-GUARD VÉDELEM: Megakadályozza a mentés gomb elszállását!
            string typeName = cmbType.SelectedItem?.ToString() ?? "";

            if (!double.TryParse(txtAirflow.Text, out double airflowValue) || airflowValue < 0)
            {
                MessageBox.Show("Adjon meg érvényes térfogatáramot!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            double.TryParse(txtLength.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double len);
            double.TryParse(txtZeta.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double zeta);
            double.TryParse(txtFixedPressureDrop.Text, out double fixPa);

            string cat = cmbCategory.SelectedItem?.ToString() ?? "Csőszakasz";
            bool isFixedPressureMode = (cmbLossCalculationMode.Visible && cmbLossCalculationMode.SelectedIndex == 1);
            bool isTransition = typeName.ToLower().Contains("átmenet") || typeName.ToLower().Contains("szűkítő") || typeName.ToLower().Contains("bővítő");

            double baseFlow = CalculateCurrentBaseAirflow();
            double finalAirflow = (cat == "Végpont (Anemosztát, Rács)" || (chkAddAdditionalAirflow.Visible && chkAddAdditionalAirflow.Checked)) ? airflowValue : baseFlow;

            if (_isEditMode)
            {
                var target = _controller.LastAddedElement;
                if (target != null)
                {
                    var result = MessageBox.Show("Biztosan módosítja az elem adatait?", "Módosítás megerősítése", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No) return;

                    target.Name = _computedName;
                    target.Airflow = (cat == "T-idom / Elágazás (Összegző)") ? baseFlow + airflowValue : finalAirflow;

                    if (target is DuctSegment segment) segment.Length = len;
                    else if (target is DuctTransition trans) { trans.Length = len; trans.Zeta = zeta; trans.PressureLossType = isFixedPressureMode ? PressureLossType.FixedPressure : PressureLossType.Zeta; if (isFixedPressureMode) trans.FixedPressureDrop = fixPa; }
                    else if (target is DuctFitting fitting) { fitting.Zeta = zeta; fitting.PressureLossType = isFixedPressureMode ? PressureLossType.FixedPressure : PressureLossType.Zeta; if (isFixedPressureMode) fitting.FixedPressureDrop = fixPa; }
                    else if (target is DuctLouver louver) { louver.Zeta = zeta; louver.PressureLossType = isFixedPressureMode ? PressureLossType.FixedPressure : PressureLossType.Zeta; if (isFixedPressureMode) louver.FixedPressureDrop = fixPa; }
                    else if (target is BranchFitting br) { br.ZetaMain = zeta; br.BranchZeta = zeta; br.BranchAirflow = airflowValue; br.PressureLossType = isFixedPressureMode ? PressureLossType.FixedPressure : PressureLossType.Zeta; if (isFixedPressureMode) br.FixedPressureDrop = fixPa; }

                    MentsGeometriaObjektumot(target);
                    if (this.ParentForm != null) { this.ParentForm.DialogResult = DialogResult.OK; this.ParentForm.Close(); }
                    return; // VÉDELMI GÁT: Megállítja a kódot, így szerkesztéskor nem fut le az új hozzáadási lánc!
                }
            }

            // HOZZÁADÁS MÓD
            DuctElement? elem = null;

            if (cat == "Csőszakasz")
            {
                var flexibleMaterial = _controller.DataProvider.GetMaterials().FirstOrDefault(m => m.IsFlexible);
                var galvanizedMaterial = _controller.DataProvider.GetMaterials().FirstOrDefault(m => !m.IsFlexible);

                var segment = new DuctSegment { Name = _computedName, Length = len, Airflow = finalAirflow };
                segment.MaterialOverride = typeName.Contains("Flexibilis") ? flexibleMaterial : galvanizedMaterial;
                elem = segment;
            }
            else if (cat == "T-idom / Elágazás (Összegző)")
            {
                double combinedFlow = baseFlow + airflowValue;
                var br = new BranchFitting { Name = _computedName, Airflow = combinedFlow, BranchAirflow = airflowValue, ZetaMain = zeta, BranchZeta = zeta };
                if (isFixedPressureMode) { br.PressureLossType = PressureLossType.FixedPressure; br.FixedPressureDrop = fixPa; }
                elem = br;
            }
            else if (cat == "Végpont (Anemosztát, Rács)")
            {
                var terminal = new DuctLouver { Name = _computedName, Airflow = finalAirflow, Zeta = zeta, Category = "Terminál" };
                if (isFixedPressureMode) { terminal.PressureLossType = PressureLossType.FixedPressure; terminal.FixedPressureDrop = fixPa; }
                elem = terminal;
                if (_currentBranch != null && _currentBranch.Elements.Count == 0) _currentBranch.Airflow = finalAirflow;
            }
            else if (cat == "Hálózati tartozék (Zsalu, Hangcsillapító)")
            {
                var accessory = new DuctLouver { Name = _computedName, Airflow = finalAirflow, Zeta = zeta, Category = "Tartozék" };
                if (isFixedPressureMode) { accessory.PressureLossType = PressureLossType.FixedPressure; accessory.FixedPressureDrop = fixPa; }
                elem = accessory;
            }
            else
            {
                if (isTransition)
                {
                    var trans = new DuctTransition { Name = _computedName, Airflow = finalAirflow, Length = len, Zeta = zeta };
                    if (isFixedPressureMode) { trans.PressureLossType = PressureLossType.FixedPressure; trans.FixedPressureDrop = fixPa; }
                    elem = trans;
                }
                else
                {
                    var fitting = new DuctFitting { Name = _computedName, Airflow = finalAirflow, Zeta = zeta };
                    if (isFixedPressureMode) { fitting.PressureLossType = PressureLossType.FixedPressure; fitting.FixedPressureDrop = fixPa; }
                    elem = fitting;
                }
            }

            if (elem == null)
            {
                MessageBox.Show(
                    "Nem sikerült létrehozni a kiválasztott légtechnikai elemet.",
                    "Elem hozzáadása",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            MentsGeometriaObjektumot(elem);

            // ÚJ: Mentjük a kontroller emlékezetébe az imént összeállított geometriát
            if (elem != null && elem.Geometry != null)
            {
                _controller.LastUsedGeometry = elem.Geometry;
            }

            if (_currentBranch != null) _controller.AddElement(_currentBranch, elem);
            if (this.ParentForm != null) { this.ParentForm.DialogResult = DialogResult.OK; this.ParentForm.Close(); }
        }

        private void MentsGeometriaObjektumot(DuctElement elem)
        {
            if (elem == null) return;

            elem.Geometry = new DuctGeometry();
            string typeName = cmbType.SelectedItem?.ToString() ?? "";

            if (typeName.StartsWith("Kör") && typeName.Contains("Négyszög")) elem.Geometry.Shape = GeometryShape.Circular;
            else elem.Geometry.Shape = rbCircular.Checked ? GeometryShape.Circular : GeometryShape.Rectangular;

            int.TryParse(cmbInletD.SelectedItem?.ToString() ?? "160", out int inletD);
            int.TryParse(cmbOutletD.SelectedItem?.ToString() ?? "160", out int outletD);
            int.TryParse(cmbBranchD.SelectedItem?.ToString() ?? "160", out int branchD);

            int.TryParse(cmbInletW.SelectedItem?.ToString() ?? "400", out int inletW);
            int.TryParse(cmbInletH.SelectedItem?.ToString() ?? "200", out int inletH);
            int.TryParse(cmbOutletW.SelectedItem?.ToString() ?? "400", out int outletW);
            int.TryParse(cmbOutletH.SelectedItem?.ToString() ?? "200", out int outletH);
            int.TryParse(cmbBranchW.SelectedItem?.ToString() ?? "400", out int branchW);
            int.TryParse(cmbBranchH.SelectedItem?.ToString() ?? "200", out int branchH);

            elem.Geometry.InletDiameter = inletD;
            elem.Geometry.OutletDiameter = outletD;
            elem.Geometry.BranchDiameter = branchD;

            elem.Geometry.InletWidth = inletW;
            elem.Geometry.InletHeight = inletH;
            elem.Geometry.OutletWidth = outletW;
            elem.Geometry.OutletHeight = outletH;
            elem.Geometry.BranchWidth = branchW;
            elem.Geometry.BranchHeight = branchH;
        }
        private void btnCancel_Click(object? sender, EventArgs e)
        {
            if (this.ParentForm != null)
            {
                this.ParentForm.DialogResult = DialogResult.Cancel;
                this.ParentForm.Close();
            }
        }
    }
}
