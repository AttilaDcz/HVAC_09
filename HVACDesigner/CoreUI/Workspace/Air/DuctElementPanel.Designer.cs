using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Structural;

namespace HVACDesigner.CoreUI.Workspace.Air
{
    partial class DuctElementPanel
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
            cardCatalog = new HVACSectionPanel();
            rbCircular = new RadioButton();
            rbRectangular = new RadioButton();
            lblCategory = new Label();
            cmbCategory = new ComboBox();
            lblType = new Label();
            cmbType = new ComboBox();
            lblMaterial = new Label();
            cmbMaterialOverride = new ComboBox();

            cardGeometry = new HVACSectionPanel();
            lblInletSize = new Label();
            cmbInletD = new ComboBox();
            cmbInletW = new ComboBox();
            lblX1 = new Label();
            cmbInletH = new ComboBox();
            lblInletUnit = new Label();

            lblOutletSize = new Label();
            cmbOutletD = new ComboBox();
            cmbOutletW = new ComboBox();
            lblX2 = new Label();
            cmbOutletH = new ComboBox();
            lblOutletUnit = new Label();

            lblBranchSize = new Label();
            cmbBranchD = new ComboBox();
            cmbBranchW = new ComboBox();
            lblX3 = new Label();
            cmbBranchH = new ComboBox();
            lblBranchUnit = new Label();

            // ÚJ: Második mellékág a Keresztidomokhoz (Cross fitting)
            lblBranch2Size = new Label();
            cmbBranch2D = new ComboBox();
            cmbBranch2W = new ComboBox();
            lblX4 = new Label();
            cmbBranch2H = new ComboBox();
            lblBranch2Unit = new Label();

            lblLength = new Label();
            txtLength = new TextBox();
            lblLengthUnit = new Label();

            lblAirflow = new Label();
            txtAirflow = new TextBox();
            lblAirflowUnit = new Label();

            // ÚJ: Második plusz légmennyiség mező a Keresztidomokhoz
            lblAirflow2 = new Label();
            txtAirflow2 = new TextBox();
            lblAirflow2Unit = new Label();

            lblLossMode = new Label();
            cmbLossCalculationMode = new ComboBox();
            lblZeta = new Label();
            txtZeta = new TextBox();
            lblFixedDeltaP = new Label();
            txtFixedPressureDrop = new TextBox();
            lblPascalUnit = new Label();

            btnSave = new Button();
            btnCancel = new Button();

            cardCatalog.SuspendLayout();
            cardGeometry.SuspendLayout();
            SuspendLayout();
            // 
            // cardCatalog
            // 
            cardCatalog.BackColor = Color.FromArgb(45, 45, 48);
            cardCatalog.Controls.Add(rbCircular);
            cardCatalog.Controls.Add(rbRectangular);
            cardCatalog.Controls.Add(lblCategory);
            cardCatalog.Controls.Add(cmbCategory);
            cardCatalog.Controls.Add(lblType);
            cardCatalog.Controls.Add(cmbType);
            cardCatalog.Controls.Add(lblMaterial);
            cardCatalog.Controls.Add(cmbMaterialOverride);
            cardCatalog.Location = new Point(15, 15);
            cardCatalog.Name = "cardCatalog";
            cardCatalog.Padding = new Padding(15, 50, 15, 15);
            cardCatalog.SectionTitle = "Elem típusa és Katalógus";
            cardCatalog.Size = new Size(570, 230);
            cardCatalog.TabIndex = 0;
            cardCatalog.TabStop = false;
            // 
            // rbCircular
            // 
            rbCircular.AutoSize = true;
            rbCircular.Checked = true;
            rbCircular.Location = new Point(25, 55);
            rbCircular.Name = "rbCircular";
            rbCircular.Size = new Size(119, 19);
            rbCircular.TabIndex = 0;
            rbCircular.TabStop = true;
            rbCircular.Text = "Kör keresztmetszet";
            rbCircular.UseVisualStyleBackColor = true;
            // 
            // rbRectangular
            // 
            rbRectangular.AutoSize = true;
            rbRectangular.Location = new Point(170, 55);
            rbRectangular.Name = "rbRectangular";
            rbRectangular.Size = new Size(156, 19);
            rbRectangular.TabIndex = 1;
            rbRectangular.Text = "Négyszög keresztmetszet";
            rbRectangular.UseVisualStyleBackColor = true;
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(22, 93);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(87, 15);
            lblCategory.TabIndex = 2;
            lblCategory.Text = "Elem kategória:";
            // 
            // cmbCategory
            // 
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.Location = new Point(140, 90);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Size = new Size(405, 23);
            cmbCategory.TabIndex = 3;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Location = new Point(22, 138);
            lblType.Name = "lblType";
            lblType.Size = new Size(74, 15);
            lblType.TabIndex = 4;
            lblType.Text = "Pontos típus:";
            // 
            // cmbType
            // 
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Location = new Point(140, 135);
            cmbType.Name = "cmbType";
            cmbType.Size = new Size(405, 23);
            cmbType.TabIndex = 5;
            // 
            // lblMaterial
            // 
            lblMaterial.AutoSize = true;
            lblMaterial.Location = new Point(22, 183);
            lblMaterial.Name = "lblMaterial";
            lblMaterial.Size = new Size(108, 15);
            lblMaterial.TabIndex = 6;
            lblMaterial.Text = "Anyag felülbírálás:";
            // 
            // cmbMaterialOverride
            // 
            cmbMaterialOverride.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMaterialOverride.Location = new Point(140, 180);
            cmbMaterialOverride.Name = "cmbMaterialOverride";
            cmbMaterialOverride.Size = new Size(405, 23);
            cmbMaterialOverride.TabIndex = 7;
            // 
            // cardGeometry
            // 
            cardGeometry.BackColor = Color.FromArgb(45, 45, 48);
            cardGeometry.Controls.Add(lblInletSize);
            cardGeometry.Controls.Add(cmbInletD);
            cardGeometry.Controls.Add(cmbInletW);
            cardGeometry.Controls.Add(lblX1);
            cardGeometry.Controls.Add(cmbInletH);
            cardGeometry.Controls.Add(lblInletUnit);
            cardGeometry.Controls.Add(lblOutletSize);
            cardGeometry.Controls.Add(cmbOutletD);
            cardGeometry.Controls.Add(cmbOutletW);
            cardGeometry.Controls.Add(lblX2);
            cardGeometry.Controls.Add(cmbOutletH);
            cardGeometry.Controls.Add(lblOutletUnit);
            cardGeometry.Controls.Add(lblBranchSize);
            cardGeometry.Controls.Add(cmbBranchD);
            cardGeometry.Controls.Add(cmbBranchW);
            cardGeometry.Controls.Add(lblX3);
            cardGeometry.Controls.Add(cmbBranchH);
            cardGeometry.Controls.Add(lblBranchUnit);
            cardGeometry.Controls.Add(lblBranch2Size);
            cardGeometry.Controls.Add(cmbBranch2D);
            cardGeometry.Controls.Add(cmbBranch2W);
            cardGeometry.Controls.Add(lblX4);
            cardGeometry.Controls.Add(cmbBranch2H);
            cardGeometry.Controls.Add(lblBranch2Unit);
            cardGeometry.Controls.Add(lblLength);
            cardGeometry.Controls.Add(txtLength);
            cardGeometry.Controls.Add(lblLengthUnit);
            cardGeometry.Controls.Add(lblAirflow);
            cardGeometry.Controls.Add(txtAirflow);
            cardGeometry.Controls.Add(lblAirflowUnit);
            cardGeometry.Controls.Add(lblAirflow2);
            cardGeometry.Controls.Add(txtAirflow2);
            cardGeometry.Controls.Add(lblAirflow2Unit);
            cardGeometry.Controls.Add(lblLossMode);
            cardGeometry.Controls.Add(cmbLossCalculationMode);
            cardGeometry.Controls.Add(lblZeta);
            cardGeometry.Controls.Add(txtZeta);
            cardGeometry.Controls.Add(lblFixedDeltaP);
            cardGeometry.Controls.Add(txtFixedPressureDrop);
            cardGeometry.Controls.Add(lblPascalUnit);
            cardGeometry.Location = new Point(15, 260);
            cardGeometry.Name = "cardGeometry";
            cardGeometry.Padding = new Padding(15, 50, 15, 15);
            cardGeometry.SectionTitle = "Geometria és Légtechnikai adatok";
            cardGeometry.Size = new Size(570, 360);
            cardGeometry.TabIndex = 1;
            cardGeometry.TabStop = false;
            // 
            // lblInletSize
            // 
            lblInletSize.Location = new Point(22, 43);
            lblInletSize.Size = new Size(110, 15);
            lblInletSize.Text = "Belépő méret:";
            // 
            // cmbInletD
            // 
            cmbInletD.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbInletD.Location = new Point(140, 40);
            cmbInletD.Size = new Size(90, 23);
            // 
            // cmbInletW
            // 
            cmbInletW.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbInletW.Location = new Point(140, 40);
            cmbInletW.Size = new Size(80, 23);
            cmbInletW.Visible = false;
            // 
            // lblX1
            // 
            lblX1.Location = new Point(225, 43);
            lblX1.Size = new Size(15, 15);
            lblX1.Text = "x";
            lblX1.Visible = false;
            // 
            // cmbInletH
            // 
            cmbInletH.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbInletH.Location = new Point(245, 40);
            cmbInletH.Size = new Size(80, 23);
            cmbInletH.Visible = false;
            // 
            // lblInletUnit
            // 
            lblInletUnit.Location = new Point(330, 43);
            lblInletUnit.Size = new Size(29, 15);
            lblInletUnit.Text = "mm";
            // 
            // lblOutletSize
            // 
            lblOutletSize.Location = new Point(22, 78);
            lblOutletSize.Size = new Size(110, 15);
            lblOutletSize.Text = "Kilépő méret:";
            lblOutletSize.Visible = false;
            // 
            // cmbOutletD
            // 
            cmbOutletD.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOutletD.Location = new Point(140, 75);
            cmbOutletD.Size = new Size(90, 23);
            cmbOutletD.Visible = false;
            // 
            // cmbOutletW
            // 
            cmbOutletW.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOutletW.Location = new Point(140, 75);
            cmbOutletW.Size = new Size(80, 23);
            cmbOutletW.Visible = false;
            // 
            // lblX2
            // 
            lblX2.Location = new Point(225, 78);
            lblX2.Size = new Size(15, 15);
            lblX2.Text = "x";
            lblX2.Visible = false;
            // 
            // cmbOutletH
            // 
            cmbOutletH.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOutletH.Location = new Point(245, 75);
            cmbOutletH.Size = new Size(80, 23);
            cmbOutletH.Visible = false;
            // 
            // lblOutletUnit
            // 
            lblOutletUnit.Location = new Point(330, 78);
            lblOutletUnit.Size = new Size(29, 15);
            lblOutletUnit.Text = "mm";
            lblOutletUnit.Visible = false;
            // 
            // lblBranchSize
            // 
            lblBranchSize.Location = new Point(22, 113);
            lblBranchSize.Size = new Size(110, 15);
            lblBranchSize.Text = "Mellékág 1 méret:";
            lblBranchSize.Visible = false;
            // 
            // cmbBranchD
            // 
            cmbBranchD.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBranchD.Location = new Point(140, 110);
            cmbBranchD.Size = new Size(90, 23);
            cmbBranchD.Visible = false;
            // 
            // cmbBranchW
            // 
            cmbBranchW.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBranchW.Location = new Point(140, 110);
            cmbBranchW.Size = new Size(80, 23);
            cmbBranchW.Visible = false;
            // 
            // lblX3
            // 
            lblX3.Location = new Point(225, 113);
            lblX3.Size = new Size(15, 15);
            lblX3.Text = "x";
            lblX3.Visible = false;
            // 
            // cmbBranchH
            // 
            cmbBranchH.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBranchH.Location = new Point(245, 110);
            cmbBranchH.Size = new Size(80, 23);
            cmbBranchH.Visible = false;
            // 
            // lblBranchUnit
            // 
            lblBranchUnit.Location = new Point(330, 113);
            lblBranchUnit.Size = new Size(29, 15);
            lblBranchUnit.Text = "mm";
            lblBranchUnit.Visible = false;
            // 
            // lblBranch2Size
            // 
            lblBranch2Size.Location = new Point(22, 148);
            lblBranch2Size.Size = new Size(110, 15);
            lblBranch2Size.Text = "Mellékág 2 méret:";
            lblBranch2Size.Visible = false;
            // 
            // cmbBranch2D
            // 
            cmbBranch2D.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBranch2D.Location = new Point(140, 145);
            cmbBranch2D.Size = new Size(90, 23);
            cmbBranch2D.Visible = false;
            // 
            // cmbBranch2W
            // 
            cmbBranch2W.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBranch2W.Location = new Point(140, 145);
            cmbBranch2W.Size = new Size(80, 23);
            cmbBranch2W.Visible = false;
            // 
            // lblX4
            // 
            lblX4.Location = new Point(225, 148);
            lblX4.Size = new Size(15, 15);
            lblX4.Text = "x";
            lblX4.Visible = false;
            // 
            // cmbBranch2H
            // 
            cmbBranch2H.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBranch2H.Location = new Point(245, 145);
            cmbBranch2H.Size = new Size(80, 23);
            cmbBranch2H.Visible = false;
            // 
            // lblBranch2Unit
            // 
            lblBranch2Unit.Location = new Point(330, 148);
            lblBranch2Unit.Size = new Size(29, 15);
            lblBranch2Unit.Text = "mm";
            lblBranch2Unit.Visible = false;
            // 
            // lblLength
            // 
            lblLength.Location = new Point(22, 183);
            lblLength.Size = new Size(110, 15);
            lblLength.Text = "Hosszúság:";
            // 
            // txtLength
            // 
            txtLength.Location = new Point(140, 180);
            txtLength.Size = new Size(90, 23);
            txtLength.Text = "1.00";
            // 
            // lblLengthUnit
            // 
            lblLengthUnit.Location = new Point(235, 183);
            lblLengthUnit.Size = new Size(18, 15);
            lblLengthUnit.Text = "m";
            // 
            // lblAirflow
            // 
            lblAirflow.Location = new Point(22, 218);
            lblAirflow.Size = new Size(110, 15);
            lblAirflow.Text = "Térfogatáram 1:";
            // 
            // txtAirflow
            // 
            txtAirflow.Location = new Point(140, 215);
            txtAirflow.Size = new Size(90, 23);
            txtAirflow.Text = "150";
            // 
            // lblAirflowUnit
            // 
            lblAirflowUnit.Location = new Point(235, 218);
            lblAirflowUnit.Size = new Size(34, 15);
            lblAirflowUnit.Text = "m³/h";
            // 
            // lblAirflow2
            // 
            lblAirflow2.Location = new Point(285, 218);
            lblAirflow2.Size = new Size(95, 15);
            lblAirflow2.Text = "Térfogatáram 2:";
            lblAirflow2.Visible = false;
            // 
            // txtAirflow2
            // 
            txtAirflow2.Location = new Point(390, 215);
            txtAirflow2.Size = new Size(90, 23);
            txtAirflow2.Text = "0";
            txtAirflow2.Visible = false;
            // 
            // lblAirflow2Unit
            // 
            lblAirflow2Unit.Location = new Point(485, 218);
            lblAirflow2Unit.Size = new Size(34, 15);
            lblAirflow2Unit.Text = "m³/h";
            lblAirflow2Unit.Visible = false;
            // 
            // lblLossMode
            // 
            lblLossMode.Location = new Point(22, 258);
            lblLossMode.Size = new Size(110, 15);
            lblLossMode.Text = "Ellenállás mód:";
            // 
            // cmbLossCalculationMode
            // 
            cmbLossCalculationMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLossCalculationMode.Location = new Point(140, 255);
            cmbLossCalculationMode.Size = new Size(220, 23);
            // 
            // lblZeta
            // 
            lblZeta.Location = new Point(22, 298);
            lblZeta.Size = new Size(110, 15);
            lblZeta.Text = "Zeta tényező (ζ):";
            // 
            // txtZeta
            // 
            txtZeta.Location = new Point(140, 295);
            txtZeta.Size = new Size(90, 23);
            txtZeta.Text = "0.00";
            // 
            // lblFixedDeltaP
            // 
            lblFixedDeltaP.Location = new Point(22, 298);
            lblFixedDeltaP.Size = new Size(110, 15);
            lblFixedDeltaP.Text = "Fix nyomásesés:";
            lblFixedDeltaP.Visible = false;
            // 
            // txtFixedPressureDrop
            // 
            txtFixedPressureDrop.Location = new Point(140, 295);
            txtFixedPressureDrop.Size = new Size(90, 23);
            txtFixedPressureDrop.Text = "20";
            txtFixedPressureDrop.Visible = false;
            // 
            // lblPascalUnit
            // 
            lblPascalUnit.Location = new Point(235, 298);
            lblPascalUnit.Size = new Size(20, 15);
            lblPascalUnit.Text = "Pa";
            lblPascalUnit.Visible = false;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(355, 640);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(110, 30);
            btnSave.Text = "Hozzáadás";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(475, 640);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(110, 30);
            btnCancel.Text = "Mégse";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // DuctElementPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(cardCatalog);
            Controls.Add(cardGeometry);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            Name = "DuctElementPanel";
            Size = new Size(600, 690);
            cardCatalog.ResumeLayout(false);
            cardCatalog.PerformLayout();
            cardGeometry.ResumeLayout(false);
            cardGeometry.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private HVACSectionPanel cardCatalog;
        private System.Windows.Forms.RadioButton rbCircular;
        private System.Windows.Forms.RadioButton rbRectangular;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Label lblMaterial;
        private System.Windows.Forms.ComboBox cmbMaterialOverride;

        private HVACSectionPanel cardGeometry;
        private System.Windows.Forms.Label lblInletSize;
        private System.Windows.Forms.ComboBox cmbInletD;
        private System.Windows.Forms.ComboBox cmbInletW;
        private System.Windows.Forms.Label lblX1;
        private System.Windows.Forms.ComboBox cmbInletH;
        private System.Windows.Forms.Label lblInletUnit;

        private System.Windows.Forms.Label lblOutletSize;
        private System.Windows.Forms.ComboBox cmbOutletD;
        private System.Windows.Forms.ComboBox cmbOutletW;
        private System.Windows.Forms.Label lblX2;
        private System.Windows.Forms.ComboBox cmbOutletH;
        private System.Windows.Forms.Label lblOutletUnit;

        private System.Windows.Forms.Label lblBranchSize;
        private System.Windows.Forms.ComboBox cmbBranchD;
        private System.Windows.Forms.ComboBox cmbBranchW;
        private System.Windows.Forms.Label lblX3;
        private System.Windows.Forms.ComboBox cmbBranchH;
        private System.Windows.Forms.Label lblBranchUnit;

        // ÚJ VEZÉRLŐK A KERESZTIDOMHOZ
        private System.Windows.Forms.Label lblBranch2Size;
        private System.Windows.Forms.ComboBox cmbBranch2D;
        private System.Windows.Forms.ComboBox cmbBranch2W;
        private System.Windows.Forms.Label lblX4;
        private System.Windows.Forms.ComboBox cmbBranch2H;
        private System.Windows.Forms.Label lblBranch2Unit;

        private System.Windows.Forms.Label lblLength;
        private System.Windows.Forms.TextBox txtLength;
        private System.Windows.Forms.Label lblLengthUnit;
        private System.Windows.Forms.Label lblAirflow;
        private System.Windows.Forms.TextBox txtAirflow;
        private System.Windows.Forms.Label lblAirflowUnit;

        // ÚJ VEZÉRLŐK A 2. LÉGMENNYISÉGHEZ
        private System.Windows.Forms.Label lblAirflow2;
        private System.Windows.Forms.TextBox txtAirflow2;
        private System.Windows.Forms.Label lblAirflow2Unit;

        private System.Windows.Forms.Label lblLossMode;
        private System.Windows.Forms.ComboBox cmbLossCalculationMode;
        private System.Windows.Forms.Label lblZeta;
        private System.Windows.Forms.TextBox txtZeta;
        private System.Windows.Forms.Label lblFixedDeltaP;
        private System.Windows.Forms.TextBox txtFixedPressureDrop;
        private System.Windows.Forms.Label lblPascalUnit;

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;


    }
}
