using System;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Data;
using HVACDesigner.CoreUI.Components.Structural;

namespace HVACDesigner.CoreUI.Workspace.Air
{
    partial class DuctNetworkPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            pnlWorkCanvas = new Panel();
            cardProjectInfo = new HVACSectionPanel();
            txtDesignerName = new TextBox();
            txtProjectName = new TextBox();
            txtProjectAddress = new TextBox();
            cmbSystemType = new ComboBox();
            dtpProjectDate = new DateTimePicker();
            cardGlobalSettings = new HVACSectionPanel();
            cmbDefaultMaterial = new ComboBox();
            lblGlobalRoughness = new Label();
            txtGlobalRoughness = new TextBox();
            lblRoughnessUnit = new Label();
            lblTemperature = new Label();
            txtAirTemperature = new TextBox();
            lblTempUnit = new Label();
            lblDensity = new Label();
            txtAirDensity = new TextBox();
            lblDensityUnit = new Label();
            lblSafetyFactor = new Label();
            txtSafetyFactor = new TextBox();
            lblSafetyUnit = new Label();
            cardNetworkCalculation = new HVACSectionPanel();
            btnAddElement = new Button();
            btnEditElement = new Button();
            btnDeleteElement = new Button();
            btnMoveUp = new Button();
            btnMoveDown = new Button();
            btnCalculate = new Button();
            lblCriticalPathStatus = new Label();
            lblActiveBranch = new Label();
            cmbActiveBranch = new ComboBox();
            btnNewBranch = new Button();
            dgvNetworkElements = new HVACDataGridView();
            lblTotalPressureLoss = new Label();

            pnlWorkCanvas.SuspendLayout();
            cardProjectInfo.SuspendLayout();
            cardGlobalSettings.SuspendLayout();
            cardNetworkCalculation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvNetworkElements).BeginInit();
            SuspendLayout();
            // 
            // pnlWorkCanvas
            // 
            pnlWorkCanvas.BackColor = Color.Transparent;
            pnlWorkCanvas.Controls.Add(cardProjectInfo);
            pnlWorkCanvas.Controls.Add(cardGlobalSettings);
            pnlWorkCanvas.Controls.Add(cardNetworkCalculation);
            pnlWorkCanvas.Location = new Point(0, 0);
            pnlWorkCanvas.Name = "pnlWorkCanvas";
            pnlWorkCanvas.Size = new Size(800, 750);
            pnlWorkCanvas.TabIndex = 0;
            // 
            // cardProjectInfo
            // 
            cardProjectInfo.BackColor = Color.FromArgb(45, 45, 48);
            cardProjectInfo.Controls.Add(txtDesignerName);
            cardProjectInfo.Controls.Add(txtProjectName);
            cardProjectInfo.Controls.Add(txtProjectAddress);
            cardProjectInfo.Controls.Add(cmbSystemType);
            cardProjectInfo.Controls.Add(dtpProjectDate);
            cardProjectInfo.Location = new Point(0, 15);
            cardProjectInfo.Name = "cardProjectInfo";
            cardProjectInfo.Padding = new Padding(15, 50, 15, 15);
            cardProjectInfo.SectionTitle = "Projekt adatok és információk";
            cardProjectInfo.Size = new Size(453, 240);
            cardProjectInfo.TabIndex = 1;
            cardProjectInfo.TabStop = false;
            // 
            // txtDesignerName
            // 
            txtDesignerName.Location = new Point(20, 55);
            txtDesignerName.Name = "txtDesignerName";
            txtDesignerName.PlaceholderText = "Tervező neve...";
            txtDesignerName.Size = new Size(413, 23);
            txtDesignerName.TabIndex = 0;
            // 
            // txtProjectName
            // 
            txtProjectName.Location = new Point(20, 90);
            txtProjectName.Name = "txtProjectName";
            txtProjectName.PlaceholderText = "Projekt / Létesítmény neve...";
            txtProjectName.Size = new Size(413, 23);
            txtProjectName.TabIndex = 1;
            // 
            // txtProjectAddress
            // 
            txtProjectAddress.Location = new Point(20, 125);
            txtProjectAddress.Name = "txtProjectAddress";
            txtProjectAddress.PlaceholderText = "Projekt pontos címe / Helyszíne...";
            txtProjectAddress.Size = new Size(413, 23);
            txtProjectAddress.TabIndex = 2;
            // 
            // cmbSystemType
            // 
            cmbSystemType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSystemType.Location = new Point(20, 160);
            cmbSystemType.Name = "cmbSystemType";
            cmbSystemType.Size = new Size(413, 23);
            cmbSystemType.TabIndex = 3;
            // 
            // dtpProjectDate
            // 
            dtpProjectDate.Format = DateTimePickerFormat.Short;
            dtpProjectDate.Location = new Point(20, 195);
            dtpProjectDate.Name = "dtpProjectDate";
            dtpProjectDate.Size = new Size(200, 23);
            dtpProjectDate.TabIndex = 4;
            // 
            // cardGlobalSettings
            // 
            cardGlobalSettings.BackColor = Color.FromArgb(45, 45, 48);
            cardGlobalSettings.Controls.Add(cmbDefaultMaterial);
            cardGlobalSettings.Controls.Add(lblGlobalRoughness);
            cardGlobalSettings.Controls.Add(txtGlobalRoughness);
            cardGlobalSettings.Controls.Add(lblRoughnessUnit);
            cardGlobalSettings.Controls.Add(lblTemperature);
            cardGlobalSettings.Controls.Add(txtAirTemperature);
            cardGlobalSettings.Controls.Add(lblTempUnit);
            cardGlobalSettings.Controls.Add(lblDensity);
            cardGlobalSettings.Controls.Add(txtAirDensity);
            cardGlobalSettings.Controls.Add(lblDensityUnit);
            cardGlobalSettings.Controls.Add(lblSafetyFactor);
            cardGlobalSettings.Controls.Add(txtSafetyFactor);
            cardGlobalSettings.Controls.Add(lblSafetyUnit);
            cardGlobalSettings.Location = new Point(459, 15);
            cardGlobalSettings.Name = "cardGlobalSettings";
            cardGlobalSettings.Padding = new Padding(15, 50, 15, 15);
            cardGlobalSettings.SectionTitle = "Hálózati alapbeállítások";
            cardGlobalSettings.Size = new Size(341, 240);
            cardGlobalSettings.TabIndex = 2;
            cardGlobalSettings.TabStop = false;
            // 
            // cmbDefaultMaterial
            // 
            cmbDefaultMaterial.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDefaultMaterial.Location = new Point(20, 55);
            cmbDefaultMaterial.Name = "cmbDefaultMaterial";
            cmbDefaultMaterial.Size = new Size(300, 23);
            cmbDefaultMaterial.TabIndex = 0;
            // 
            // lblGlobalRoughness
            // 
            lblGlobalRoughness.AutoSize = true;
            lblGlobalRoughness.Location = new Point(22, 93);
            lblGlobalRoughness.Name = "lblGlobalRoughness";
            lblGlobalRoughness.Size = new Size(111, 15);
            lblGlobalRoughness.TabIndex = 1;
            lblGlobalRoughness.Text = "Csőfal érdesség (k):";
            // 
            // txtGlobalRoughness
            // 
            txtGlobalRoughness.Location = new Point(140, 90);
            txtGlobalRoughness.Name = "txtGlobalRoughness";
            txtGlobalRoughness.Size = new Size(55, 23);
            txtGlobalRoughness.TabIndex = 2;
            txtGlobalRoughness.Text = "0.15";
            // 
            // lblRoughnessUnit
            // 
            lblRoughnessUnit.AutoSize = true;
            lblRoughnessUnit.Location = new Point(200, 93);
            lblRoughnessUnit.Name = "lblRoughnessUnit";
            lblRoughnessUnit.Size = new Size(29, 15);
            lblRoughnessUnit.TabIndex = 3;
            lblRoughnessUnit.Text = "mm";
            // 
            // lblTemperature
            // 
            lblTemperature.AutoSize = true;
            lblTemperature.Location = new Point(22, 128);
            lblTemperature.Name = "lblTemperature";
            lblTemperature.Size = new Size(77, 15);
            lblTemperature.TabIndex = 4;
            lblTemperature.Text = "Hőmérséklet:";
            // 
            // txtAirTemperature
            // 
            txtAirTemperature.Location = new Point(140, 125);
            txtAirTemperature.Name = "txtAirTemperature";
            txtAirTemperature.Size = new Size(55, 23);
            txtAirTemperature.TabIndex = 5;
            txtAirTemperature.Text = "20";
            // 
            // lblTempUnit
            // 
            lblTempUnit.AutoSize = true;
            lblTempUnit.Location = new Point(200, 128);
            lblTempUnit.Name = "lblTempUnit";
            lblTempUnit.Size = new Size(20, 15);
            lblTempUnit.TabIndex = 6;
            lblTempUnit.Text = "°C";
            // 
            // lblDensity
            // 
            lblDensity.AutoSize = true;
            lblDensity.Location = new Point(22, 163);
            lblDensity.Name = "lblDensity";
            lblDensity.Size = new Size(52, 15);
            lblDensity.TabIndex = 7;
            lblDensity.Text = "Sűrűség:";
            // 
            // txtAirDensity
            // 
            txtAirDensity.Location = new Point(140, 160);
            txtAirDensity.Name = "txtAirDensity";
            txtAirDensity.Size = new Size(55, 23);
            txtAirDensity.TabIndex = 8;
            txtAirDensity.Text = "1.204";
            // 
            // lblDensityUnit
            // 
            lblDensityUnit.AutoSize = true;
            lblDensityUnit.Location = new Point(200, 163);
            lblDensityUnit.Name = "lblDensityUnit";
            lblDensityUnit.Size = new Size(40, 15);
            lblDensityUnit.TabIndex = 9;
            lblDensityUnit.Text = "kg/m³";
            // 
            // lblSafetyFactor
            // 
            lblSafetyFactor.AutoSize = true;
            lblSafetyFactor.Location = new Point(22, 198);
            lblSafetyFactor.Name = "lblSafetyFactor";
            lblSafetyFactor.Size = new Size(111, 15);
            lblSafetyFactor.TabIndex = 10;
            lblSafetyFactor.Text = "Biztonsági tartalék:";
            // 
            // txtSafetyFactor
            // 
            txtSafetyFactor.Location = new Point(140, 195);
            txtSafetyFactor.Name = "txtSafetyFactor";
            txtSafetyFactor.Size = new Size(55, 23);
            txtSafetyFactor.TabIndex = 11;
            txtSafetyFactor.Text = "0";
            // 
            // lblSafetyUnit
            // 
            lblSafetyUnit.AutoSize = true;
            lblSafetyUnit.Location = new Point(200, 198);
            lblSafetyUnit.Name = "lblSafetyUnit";
            lblSafetyUnit.Size = new Size(17, 15);
            lblSafetyUnit.TabIndex = 12;
            lblSafetyUnit.Text = "%";
            // 
            // cardNetworkCalculation
            // 
            cardNetworkCalculation.BackColor = Color.FromArgb(45, 45, 48);
            cardNetworkCalculation.Controls.Add(btnCalculate);
            cardNetworkCalculation.Controls.Add(btnAddElement);
            cardNetworkCalculation.Controls.Add(btnEditElement);
            cardNetworkCalculation.Controls.Add(btnDeleteElement);
            cardNetworkCalculation.Controls.Add(btnMoveUp);
            cardNetworkCalculation.Controls.Add(btnMoveDown);
            cardNetworkCalculation.Controls.Add(lblCriticalPathStatus);
            cardNetworkCalculation.Controls.Add(lblActiveBranch);
            cardNetworkCalculation.Controls.Add(cmbActiveBranch);
            cardNetworkCalculation.Controls.Add(btnNewBranch);
            cardNetworkCalculation.Controls.Add(dgvNetworkElements);
            cardNetworkCalculation.Controls.Add(lblTotalPressureLoss);
            cardNetworkCalculation.Location = new Point(0, 270);
            cardNetworkCalculation.Name = "cardNetworkCalculation";
            cardNetworkCalculation.Padding = new Padding(15, 50, 15, 15);
            cardNetworkCalculation.SectionTitle = "Légtechnikai hálózat elemlista";
            cardNetworkCalculation.Size = new Size(800, 465);
            cardNetworkCalculation.TabIndex = 3;
            cardNetworkCalculation.TabStop = false;
            // 
            // btnAddElement
            // 
            btnAddElement.Location = new Point(20, 55);
            btnAddElement.Name = "btnAddElement";
            btnAddElement.Size = new Size(110, 30);
            btnAddElement.TabIndex = 0;
            btnAddElement.Text = "[+] Hozzáadás";
            btnAddElement.UseVisualStyleBackColor = true;
            // 
            // btnEditElement
            // 
            btnEditElement.Location = new Point(135, 55);
            btnEditElement.Name = "btnEditElement";
            btnEditElement.Size = new Size(110, 30);
            btnEditElement.TabIndex = 1;
            btnEditElement.Text = "[✎] Szerkesztés";
            btnEditElement.UseVisualStyleBackColor = true;
            // 
            // btnDeleteElement
            // 
            btnDeleteElement.Location = new Point(250, 55);
            btnDeleteElement.Name = "btnDeleteElement";
            btnDeleteElement.Size = new Size(80, 30);
            btnDeleteElement.TabIndex = 2;
            btnDeleteElement.Text = "[-] Törlés";
            btnDeleteElement.UseVisualStyleBackColor = true;
            // 
            // btnMoveUp
            // 
            btnMoveUp.Location = new Point(335, 55);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(65, 30);
            btnMoveUp.TabIndex = 3;
            btnMoveUp.Text = "[↑] Fel";
            btnMoveUp.UseVisualStyleBackColor = true;
            // 
            // btnMoveDown
            // 
            btnMoveDown.Location = new Point(405, 55);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(65, 30);
            btnMoveDown.TabIndex = 4;
            btnMoveDown.Text = "[↓] Le";
            btnMoveDown.UseVisualStyleBackColor = true;
            // 
            // btnCalculate
            // 
            btnCalculate.Location = new Point(660, 55);
            btnCalculate.Name = "btnCalculate";
            btnCalculate.Size = new Size(120, 30);
            btnCalculate.TabIndex = 5;
            btnCalculate.Text = "Számítás indítása";
            btnCalculate.UseVisualStyleBackColor = true;
            // 
            // lblCriticalPathStatus
            // 
            lblCriticalPathStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblCriticalPathStatus.AutoSize = true;
            lblCriticalPathStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCriticalPathStatus.ForeColor = Color.LightGreen;
            lblCriticalPathStatus.Location = new Point(640, 15);
            lblCriticalPathStatus.Name = "lblCriticalPathStatus";
            lblCriticalPathStatus.Size = new Size(141, 19);
            lblCriticalPathStatus.TabIndex = 6;
            lblCriticalPathStatus.Text = "✓ KRITIKUS ÚTVONAL";
            lblCriticalPathStatus.TextAlign = ContentAlignment.TopRight;
            // 
            // lblActiveBranch
            // 
            lblActiveBranch.AutoSize = true;
            lblActiveBranch.Location = new Point(20, 103);
            lblActiveBranch.Name = "lblActiveBranch";
            lblActiveBranch.Size = new Size(71, 15);
            lblActiveBranch.TabIndex = 7;
            lblActiveBranch.Text = "Aktuális ág:";
            // 
            // cmbActiveBranch
            // 
            cmbActiveBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbActiveBranch.Location = new Point(95, 100);
            cmbActiveBranch.Name = "cmbActiveBranch";
            cmbActiveBranch.Size = new Size(200, 23);
            cmbActiveBranch.TabIndex = 8;
            // 
            // btnNewBranch
            // 
            btnNewBranch.Location = new Point(300, 100);
            btnNewBranch.Name = "btnNewBranch";
            btnNewBranch.Size = new Size(35, 23);
            btnNewBranch.TabIndex = 9;
            btnNewBranch.Text = "[+]";
            btnNewBranch.UseVisualStyleBackColor = true;
            // 
            // dgvNetworkElements
            // 
            dgvNetworkElements.Location = new Point(20, 135);
            dgvNetworkElements.Name = "dgvNetworkElements";
            dgvNetworkElements.Size = new Size(760, 275);
            dgvNetworkElements.TabIndex = 10;
            // 
            // lblTotalPressureLoss
            // 
            lblTotalPressureLoss.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblTotalPressureLoss.AutoSize = true;
            lblTotalPressureLoss.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalPressureLoss.Location = new Point(450, 425);
            lblTotalPressureLoss.Name = "lblTotalPressureLoss";
            lblTotalPressureLoss.Size = new Size(293, 20);
            lblTotalPressureLoss.TabIndex = 11;
            lblTotalPressureLoss.Text = "Méretezési össznyomásveszteség: 0.0 Pa";
            lblTotalPressureLoss.TextAlign = ContentAlignment.TopRight;
            // 
            // DuctNetworkPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlWorkCanvas);
            Name = "DuctNetworkPanel";
            Size = new Size(800, 750);
            pnlWorkCanvas.ResumeLayout(false);
            cardProjectInfo.ResumeLayout(false);
            cardProjectInfo.PerformLayout();
            cardGlobalSettings.ResumeLayout(false);
            cardGlobalSettings.PerformLayout();
            cardNetworkCalculation.ResumeLayout(false);
            cardNetworkCalculation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvNetworkElements).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlWorkCanvas;
        private HVACDesigner.CoreUI.Components.Structural.HVACSectionPanel cardProjectInfo;
        private System.Windows.Forms.TextBox txtDesignerName;
        private System.Windows.Forms.TextBox txtProjectName;
        private System.Windows.Forms.TextBox txtProjectAddress;
        private System.Windows.Forms.ComboBox cmbSystemType;
        private System.Windows.Forms.DateTimePicker dtpProjectDate;

        private HVACDesigner.CoreUI.Components.Structural.HVACSectionPanel cardGlobalSettings;
        private System.Windows.Forms.ComboBox cmbDefaultMaterial;
        private System.Windows.Forms.Label lblGlobalRoughness;
        private System.Windows.Forms.TextBox txtGlobalRoughness;
        private System.Windows.Forms.Label lblRoughnessUnit;
        private System.Windows.Forms.Label lblTemperature;
        private System.Windows.Forms.TextBox txtAirTemperature;
        private System.Windows.Forms.Label lblTempUnit;
        private System.Windows.Forms.Label lblDensity;
        private System.Windows.Forms.TextBox txtAirDensity;
        private System.Windows.Forms.Label lblDensityUnit;
        private System.Windows.Forms.Label lblSafetyFactor;
        private System.Windows.Forms.TextBox txtSafetyFactor;
        private System.Windows.Forms.Label lblSafetyUnit;

        private HVACDesigner.CoreUI.Components.Structural.HVACSectionPanel cardNetworkCalculation;
        private System.Windows.Forms.Button btnAddElement;
        private System.Windows.Forms.Button btnEditElement;
        private System.Windows.Forms.Button btnDeleteElement;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.Label lblCriticalPathStatus;
        private System.Windows.Forms.Label lblActiveBranch;
        private System.Windows.Forms.ComboBox cmbActiveBranch;
        private System.Windows.Forms.Button btnNewBranch;
        private HVACDesigner.CoreUI.Components.Data.HVACDataGridView dgvNetworkElements;
        private System.Windows.Forms.Label lblTotalPressureLoss;
    }
}
