using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Workspace.Air;
using HVACDesigner.Data.Models.Duct;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Workspace.Air
{
    public partial class DuctNetworkPanel : UserControl
    {
        private readonly DuctNetworkController _controller;
        private IReadOnlyList<DuctMaterial> _availableMaterials = Array.Empty<DuctMaterial>();
        private bool _isUpdatingUi = false;

        public DuctNetworkPanel()
        {
            InitializeComponent();
            _controller = new DuctNetworkController();

            RegisterUiEvents();
            InitializeDataGridViewColumns();
            LoadGlobalDefaultSettings();
            InitializeDefaultBranch();
        }

        private void RegisterUiEvents()
        {
            btnAddElement.Click += btnAddElement_Click;
            btnEditElement.Click += btnEditElement_Click;
            btnDeleteElement.Click += btnDeleteElement_Click;
            btnMoveUp.Click += btnMoveUp_Click;
            btnMoveDown.Click += btnMoveDown_Click;
            btnCalculate.Click += btnCalculate_Click;

            btnNewBranch.Click += btnNewBranch_Click;
            cmbActiveBranch.SelectedIndexChanged += cmbActiveBranch_SelectedIndexChanged;

            cmbDefaultMaterial.SelectedIndexChanged += cmbDefaultMaterial_SelectedIndexChanged;
            txtGlobalRoughness.TextChanged += txtGlobalRoughness_TextChanged;
            txtAirDensity.TextChanged += txtAirDensity_TextChanged;
            txtSafetyFactor.TextChanged += txtSafetyFactor_TextChanged;

            if (this.ParentForm != null)
            {
                this.ParentForm.Resize += (s, e) => CenterWorkCanvas();
            }
            this.Resize += (s, e) => CenterWorkCanvas();
        }

        private void CenterWorkCanvas()
        {
            if (pnlWorkCanvas != null)
            {
                int targetX = (this.Width - pnlWorkCanvas.Width) / 2;
                if (targetX < 0) targetX = 0;
                pnlWorkCanvas.Location = new Point(targetX, 0);
                pnlWorkCanvas.Height = this.Height;

                cardNetworkCalculation.Height = this.Height - cardNetworkCalculation.Top - 15;
                dgvNetworkElements.Height = cardNetworkCalculation.Height - dgvNetworkElements.Top - 55;
            }
        }

        private void InitializeDataGridViewColumns()
        {
            dgvNetworkElements.Columns.Clear();
            dgvNetworkElements.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIndex", HeaderText = "Sorsz.", ReadOnly = true, FillWeight = 40 });
            dgvNetworkElements.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "Elem típusa / neve", ReadOnly = true, FillWeight = 175 });
            dgvNetworkElements.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSize", HeaderText = "Jellemző méret", ReadOnly = true, FillWeight = 95 });
            dgvNetworkElements.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAirflow", HeaderText = "Légmenny. [m³/h]", ReadOnly = true, FillWeight = 105 });
            dgvNetworkElements.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVelocity", HeaderText = "Légseb. [m/s]", ReadOnly = true, FillWeight = 90 });
            dgvNetworkElements.Columns.Add(new DataGridViewTextBoxColumn { Name = "colElemLoss", HeaderText = "Elemi Δp [Pa]", ReadOnly = true, FillWeight = 95 });
            dgvNetworkElements.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTotalLoss", HeaderText = "Rendszer Δp [Pa]", ReadOnly = true, FillWeight = 105 });
        }

        private void LoadGlobalDefaultSettings()
        {
            _isUpdatingUi = true;

            cmbSystemType.Items.Clear();
            cmbSystemType.Items.AddRange(new object[] {
                "Frisslevegő hálózat (Outside Air)",
                "Befúvó légtechnikai rendszer (Supply Air)",
                "Elszívó légtechnikai hálózat (Extract Air)",
                "Kidobott levegő hálózat (Exhaust Air)"
            });
            cmbSystemType.SelectedIndex = 1;

            try
            {
                _availableMaterials = _controller.DataProvider.GetMaterials();
                cmbDefaultMaterial.Items.Clear();
                foreach (var material in _availableMaterials)
                {
                    cmbDefaultMaterial.Items.Add(material.Name);
                }
                if (cmbDefaultMaterial.Items.Count > 0)
                {
                    cmbDefaultMaterial.SelectedIndex = 0;
                    if (_availableMaterials[0] != null)
                    {
                        txtGlobalRoughness.Text = _availableMaterials[0].Roughness.ToString("F2");
                        _controller.Network.GlobalMaterial = _availableMaterials[0];
                    }
                }
            }
            catch
            {
                cmbDefaultMaterial.Items.Add("Horganyzott acéllemez");
                cmbDefaultMaterial.SelectedIndex = 0;
                txtGlobalRoughness.Text = "0.15";
            }

            _isUpdatingUi = false;
        }

        private void InitializeDefaultBranch()
        {
            _isUpdatingUi = true;

            _controller.Network.Branches.Clear();
            _controller.AddBranch("Főág", 600);

            // FIX: Beállítjuk a DisplayMembert, hogy a ComboBox a belső osztálynév helyett az ág nevét írja ki!
            cmbActiveBranch.DisplayMember = "Name";
            cmbActiveBranch.Items.Clear();
            cmbActiveBranch.Items.Add(_controller.Network.Branches[0]);
            cmbActiveBranch.SelectedIndex = 0;

            _isUpdatingUi = false;
            RefreshNetworkElementsList();
        }

        private DuctBranch? GetSelectedBranch()
        {
            if (cmbActiveBranch.SelectedItem is DuctBranch branch)
            {
                return branch;
            }
            return _controller.Network.Branches.Count > 0 ? _controller.Network.Branches[0] : null;
        }

        private void RefreshNetworkElementsList()
        {
            dgvNetworkElements.Rows.Clear();

            var currentBranch = GetSelectedBranch();
            if (currentBranch == null) return;

            var elements = currentBranch.Elements;

            for (int i = 0; i < elements.Count; i++)
            {
                var elem = elements[i];
                elem.Index = i + 1;

                string displayIndex = elem.Index.ToString();
                string name = elem.Name ?? "Ismeretlen elem";
                string size = elem.SizeLabel ?? "-";
                string airflow = elem.Airflow.ToString("F0");

                dgvNetworkElements.Rows.Add(displayIndex, name, size, airflow, "-", "-", "-");
            }

            if (currentBranch.IsCriticalPath || _controller.Network.Branches.Count <= 1)
            {
                lblCriticalPathStatus.Text = "✓ KRITIKUS ÚTVONAL";
                lblCriticalPathStatus.ForeColor = Color.LightGreen;
            }
            else
            {
                lblCriticalPathStatus.Text = "Mellékág (Kiegyenlítendő)";
                lblCriticalPathStatus.ForeColor = Color.DarkGray;
            }

            lblTotalPressureLoss.Text = "Méretezési össznyomásveszteség: Számításra vár...";
        }

        #region UI Eseménykezelők

        private void cmbDefaultMaterial_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingUi) return;

            if (_availableMaterials != null && cmbDefaultMaterial.SelectedIndex >= 0 && cmbDefaultMaterial.SelectedIndex < _availableMaterials.Count)
            {
                var selectedMaterial = _availableMaterials[cmbDefaultMaterial.SelectedIndex];
                _controller.Network.GlobalMaterial = selectedMaterial;
                txtGlobalRoughness.Text = selectedMaterial.Roughness.ToString("F2");
            }
        }

        private void txtGlobalRoughness_TextChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingUi) return;

            if (double.TryParse(txtGlobalRoughness.Text, out double roughness))
            {
                if (_controller.Network.GlobalMaterial != null)
                {
                    _controller.Network.GlobalMaterial.Roughness = roughness;
                }
            }
        }

        private void txtAirDensity_TextChanged(object? sender, EventArgs e)
        {
            if (double.TryParse(txtAirDensity.Text, out double density))
            {
                _controller.Network.AirDensity = density;
            }
        }

        private void txtSafetyFactor_TextChanged(object? sender, EventArgs e)
        {
            if (double.TryParse(txtSafetyFactor.Text, out double factor))
            {
                // Előkészítve a számítási szorzónak
            }
        }

        private void btnNewBranch_Click(object? sender, EventArgs e)
        {
            using (Form inputForm = new Form { Width = 350, Height = 150, Text = "Új ág létrehozása", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, BackColor = Color.FromArgb(45, 45, 48) })
            {
                Label lblName = new Label { Left = 20, Top = 20, Text = "Ág neve:", Width = 80, ForeColor = Color.White };
                TextBox txtName = new TextBox { Left = 110, Top = 18, Width = 190 };
                Button btnOk = new Button { Text = "OK", Left = 125, Top = 65, Width = 90, DialogResult = DialogResult.OK };

                inputForm.Controls.AddRange(new Control[] { lblName, txtName, btnOk });
                

                if (inputForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtName.Text))
                {
                    _controller.AddBranch(txtName.Text.Trim(), 0);

                    _isUpdatingUi = true;
                    cmbActiveBranch.Items.Clear();
                    foreach (var branch in _controller.Network.Branches)
                    {
                        cmbActiveBranch.Items.Add(branch);
                    }
                    cmbActiveBranch.SelectedIndex = cmbActiveBranch.Items.Count - 1;
                    _isUpdatingUi = false;

                    RefreshNetworkElementsList();
                }
            }
        }

        private void cmbActiveBranch_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingUi) return;
            RefreshNetworkElementsList();
        }

        private void btnAddElement_Click(object? sender, EventArgs e)
        {
            var currentBranch = GetSelectedBranch();
            if (currentBranch == null) return;

            // FIX: Tisztítjuk a cache-t szerkesztés után, hogy az új ablak tiszta lapként nyíljon
            _controller.LastAddedElement = null;

            var elementPanel = new DuctElementPanel(_controller);
            elementPanel.SetCurrentBranch(currentBranch);

            var form = new Form
            {
                Text = "Új elem hozzáadása",
                Width = 600,
                Height = 650,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            elementPanel.Dock = DockStyle.Fill;
            form.Controls.Add(elementPanel);

            if (form.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Elem hozzáadva!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            RefreshNetworkElementsList();
        }

        private void btnEditElement_Click(object? sender, EventArgs e)
        {
            if (dgvNetworkElements.SelectedRows.Count == 0)
            {
                MessageBox.Show("Kérjük, jelöljön ki egy elemet!", "Szerkesztés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedIndex = dgvNetworkElements.SelectedRows[0].Index;
            var currentBranch = GetSelectedBranch();

            if (currentBranch != null && selectedIndex >= 0 && selectedIndex < currentBranch.Elements.Count)
            {
                var targetElement = currentBranch.Elements[selectedIndex];

                // FIX: Átadjuk a kijelölt elemet a kontrollernek, így a DuctElementPanel be tudja tölteni az adatokat!
                _controller.LastAddedElement = targetElement;

                var elementPanel = new DuctElementPanel(_controller);
                elementPanel.SetCurrentBranch(currentBranch);

                var form = new Form
                {
                    Text = "Elem módosítása",
                    Width = 600,
                    Height = 650,
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.FromArgb(45, 45, 48)
                };
                elementPanel.Dock = DockStyle.Fill;
                form.Controls.Add(elementPanel);

                
                if (form.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Elem módosítva!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                RefreshNetworkElementsList();
            }
        }

        private void btnDeleteElement_Click(object? sender, EventArgs e)
        {
            if (dgvNetworkElements.SelectedRows.Count == 0) return;

            int selectedIndex = dgvNetworkElements.SelectedRows[0].Index;
            var currentBranch = GetSelectedBranch();

            if (currentBranch != null && selectedIndex >= 0 && selectedIndex < currentBranch.Elements.Count)
            {
                var result = MessageBox.Show(
                    "Biztosan törölni szeretné a kijelölt elemet?",
                    "Törlés",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    currentBranch.Elements.RemoveAt(selectedIndex);
                    RefreshNetworkElementsList();
                }
            }
        }

        private void btnMoveUp_Click(object? sender, EventArgs e)
        {
            if (dgvNetworkElements.SelectedRows.Count == 0) return;

            int currentIndex = dgvNetworkElements.SelectedRows[0].Index;
            var currentBranch = GetSelectedBranch();

            if (currentBranch != null && currentIndex > 0 && currentIndex < currentBranch.Elements.Count)
            {
                var elementToMove = currentBranch.Elements[currentIndex];
                currentBranch.Elements.RemoveAt(currentIndex);
                currentBranch.Elements.Insert(currentIndex - 1, elementToMove);

                RefreshNetworkElementsList();

                dgvNetworkElements.ClearSelection();
                dgvNetworkElements.Rows[currentIndex - 1].Selected = true;
            }
        }

        private void btnMoveDown_Click(object? sender, EventArgs e)
        {
            if (dgvNetworkElements.SelectedRows.Count == 0) return;

            int currentIndex = dgvNetworkElements.SelectedRows[0].Index;
            var currentBranch = GetSelectedBranch();

            if (currentBranch != null && currentIndex >= 0 && currentIndex < currentBranch.Elements.Count - 1)
            {
                var elementToMove = currentBranch.Elements[currentIndex];
                currentBranch.Elements.RemoveAt(currentIndex);
                currentBranch.Elements.Insert(currentIndex + 1, elementToMove);

                RefreshNetworkElementsList();

                dgvNetworkElements.ClearSelection();
                dgvNetworkElements.Rows[currentIndex + 1].Selected = true;
            }
        }

        private void btnCalculate_Click(object? sender, EventArgs e)
        {
            var currentBranch = GetSelectedBranch();
            if (currentBranch == null || currentBranch.Elements.Count == 0)
            {
                MessageBox.Show("A hálózat még nem tartalmaz elemeket a számításhoz!", "Számítás", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1. Globális fizikai paraméterek beolvasása az UI-ról
            if (double.TryParse(txtAirDensity.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double density))
            {
                _controller.Network.AirDensity = density;
            }

            var activeCalc = new Calculations.Air.DuctCalculator { AirDensity = _controller.Network.AirDensity };

            // A tervezési irány miatt (Rácstól -> Gép felé) az induló légmennyiség az 1. anemosztát értéke
            double runningFlow = currentBranch.FlowM3h;
            double accumulatedPressureLoss = 0;

            _isUpdatingUi = true;
            dgvNetworkElements.SuspendLayout();

            // 2. Dinamikus végiggyaloglás a hálózaton az anemosztáttól a gép felé
            for (int i = 0; i < currentBranch.Elements.Count; i++)
            {
                var elem = currentBranch.Elements[i];
                double elemLoss = 0;

                if (elem is BranchFitting br)
                {
                    // T-idom kezelése rácstól gép felé haladva:
                    // A T-idom előtti szakaszon még a runningFlow áramlott.
                    br.Airflow = runningFlow;

                    // Kiszámítjuk a T-idom ellenállását (ha mellékágként csatlakozik, a kanyarodót, különben az egyenest)
                    var branchLosses = activeCalc.ComputeBranchLoss(br, br.Airflow);
                    elemLoss = br.IsBranchDirection ? branchLosses.mainToBranch : branchLosses.mainToStraight;
                    br.PressureDrop = elemLoss;

                    // GÉPÉSZETI MAGYARÁZAT: Mivel a gép felé haladunk, a T-idom után a csatornába 
                    // belép/ráadódik a mellékág légmennyisége is, így a runningFlow megnövekszik!
                    runningFlow += br.BranchAirflow;
                }
                else
                {
                    // Normál elemek (csövek, könyökök, átmenetek) megkapják az aktuális csatorna-légmennyiséget
                    elem.Airflow = runningFlow;

                    if (elem is DuctSegment seg) elemLoss = activeCalc.ComputeSegmentLoss(seg, runningFlow);
                    else if (elem is DuctTransition trans) elemLoss = activeCalc.ComputeTransitionLoss(trans, runningFlow);
                    else if (elem is DuctFitting fit) elemLoss = activeCalc.ComputeFittingLoss(fit, runningFlow);
                    else elemLoss = elem.CalculatePressureDrop(_controller.Network.AirDensity);
                }
                // Adatszinkron kényszerítése a modellen
                elem.PressureDrop = elemLoss;

                // HAJSZÁLPONTOS OO JAVÍTÁS: Mivel a Category egy string, közvetlenül a "Terminal" értéket nézzük
                if (elem.Category != null && elem.Category.ToString() == "Terminal")
                {
                    elem.Velocity = 0;
                }
                else
                {
                    elem.Velocity = elem.GetVelocity(); // A csövek és idomok pontosan számolnak tovább
                }

                accumulatedPressureLoss += elemLoss;

                // UI DataGridView sor frissítése
                var row = dgvNetworkElements.Rows[i];
                row.Cells["colAirflow"].Value = elem.Airflow.ToString("F0");

                // Ha terminál, akkor jön a kötőjel, egyébként a kiszámolt sebesség
                row.Cells["colVelocity"].Value = elem.Velocity > 0 ? elem.Velocity.ToString("F2") : "-";

                row.Cells["colElemLoss"].Value = elem.PressureDrop.ToString("F1");
                row.Cells["colTotalLoss"].Value = accumulatedPressureLoss.ToString("F1");
            }

            // 3. Biztonsági szorzó érvényesítése a hálózat végén (a gépnél)
            currentBranch.PressureLossPa = accumulatedPressureLoss;

            double safetyPercent = 0;
            double safetyMultiplier = 1.0;
            if (double.TryParse(txtSafetyFactor.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out safetyPercent) && safetyPercent > 0)
            {
                safetyMultiplier = 1.0 + (safetyPercent / 100.0);
            }

            double finalNetworkLoss = accumulatedPressureLoss * safetyMultiplier;
            lblTotalPressureLoss.Text = $"Méretezési össznyomásveszteség: {finalNetworkLoss.ToString("F1")} Pa (Tartalékkal)";

            dgvNetworkElements.ResumeLayout();
            _isUpdatingUi = false;

            MessageBox.Show("Az áramlástani hálózatszámítás sikeresen lefutott a tervezési iránynak megfelelően!", "Számítás kész", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}
