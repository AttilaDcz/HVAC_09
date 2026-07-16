using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Components.Results;
using HVACDesigner.CoreUI.Components.Structural;

namespace HVACDesigner.CoreUI.Workspace.Air
{
    partial class AirVelocityControl
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
            inputCard = new EngineeringCardPanel();
            txtFlow = new EngineeringTextBox();
            geometryCard = new EngineeringCardPanel();
            rbCircular = new EngineeringRadioButton();
            rbRectangular = new EngineeringRadioButton();
            cmbDiameter = new EngineeringComboBox();
            cmbWidth = new EngineeringComboBox();
            cmbHeight = new EngineeringComboBox();
            btnCalculate = new EngineeringButton();
            resultCard = new EngineeringResultCard();
            inputCard.SuspendLayout();
            geometryCard.SuspendLayout();
            SuspendLayout();
            // 
            // inputCard
            // 
            inputCard.ContentPanel.Controls.Add(txtFlow);
            inputCard.IconKind = CoreUI.Icons.HvacIconKind.AirVelocity;
            inputCard.Location = new Point(24, 20);
            inputCard.Name = "inputCard";
            inputCard.ShowIcon = true;
            inputCard.ShowHeaderActions = false;
            inputCard.ShowStatusBadge = false;
            inputCard.Size = new Size(300, 165);
            inputCard.Status = EngineeringCardStatus.Info;
            inputCard.Subtitle = "Számítási térfogatáram";
            inputCard.TabIndex = 0;
            inputCard.Title = "Légmennyiség";
            // 
            // txtFlow
            // 
            txtFlow.IsRequired = true;
            txtFlow.LabelText = "Térfogatáram";
            txtFlow.Location = new Point(8, 16);
            txtFlow.MaxSi = 27.777777777777779D;
            txtFlow.MinSi = 0.00027777777777777778D;
            txtFlow.Name = "txtFlow";
            txtFlow.QuantityKind = HVACDesigner.Services.QuantityKind.AirFlow;
            txtFlow.Size = new Size(264, 58);
            txtFlow.TabIndex = 0;
            // 
            // geometryCard
            // 
            geometryCard.ContentPanel.Controls.Add(cmbHeight);
            geometryCard.ContentPanel.Controls.Add(cmbWidth);
            geometryCard.ContentPanel.Controls.Add(cmbDiameter);
            geometryCard.ContentPanel.Controls.Add(rbRectangular);
            geometryCard.ContentPanel.Controls.Add(rbCircular);
            geometryCard.IconKind = CoreUI.Icons.HvacIconKind.DuctSizing;
            geometryCard.Location = new Point(24, 205);
            geometryCard.Name = "geometryCard";
            geometryCard.ShowIcon = true;
            geometryCard.ShowHeaderActions = false;
            geometryCard.ShowStatusBadge = false;
            geometryCard.Size = new Size(300, 280);
            geometryCard.Status = EngineeringCardStatus.None;
            geometryCard.Subtitle = "Keresztmetszet kiválasztása";
            geometryCard.TabIndex = 1;
            geometryCard.Title = "Csatornageometria";
            // 
            // rbCircular
            // 
            rbCircular.Checked = true;
            rbCircular.Location = new Point(8, 16);
            rbCircular.Name = "rbCircular";
            rbCircular.Size = new Size(210, 28);
            rbCircular.TabIndex = 0;
            rbCircular.Text = "Kör légcsatorna";
            rbCircular.CheckedChanged += rbCircular_CheckedChanged_1;
            // 
            // rbRectangular
            // 
            rbRectangular.Location = new Point(8, 50);
            rbRectangular.Name = "rbRectangular";
            rbRectangular.Size = new Size(230, 28);
            rbRectangular.TabIndex = 1;
            rbRectangular.Text = "Szögletes légcsatorna";
            rbRectangular.CheckedChanged += rbRectangular_CheckedChanged_1;
            // 
            // cmbDiameter
            // 
            cmbDiameter.LabelText = "Átmérő";
            cmbDiameter.Location = new Point(8, 106);
            cmbDiameter.Name = "cmbDiameter";
            cmbDiameter.PlaceholderText = "Válassz méretet";
            cmbDiameter.Size = new Size(170, 58);
            cmbDiameter.TabIndex = 2;
            // 
            // cmbWidth
            // 
            cmbWidth.LabelText = "Szélesség";
            cmbWidth.Location = new Point(8, 106);
            cmbWidth.Name = "cmbWidth";
            cmbWidth.PlaceholderText = "Válassz méretet";
            cmbWidth.Size = new Size(125, 58);
            cmbWidth.TabIndex = 3;
            // 
            // cmbHeight
            // 
            cmbHeight.LabelText = "Magasság";
            cmbHeight.Location = new Point(145, 106);
            cmbHeight.Name = "cmbHeight";
            cmbHeight.PlaceholderText = "Válassz méretet";
            cmbHeight.Size = new Size(125, 58);
            cmbHeight.TabIndex = 4;
            // 
            // btnCalculate
            // 
            btnCalculate.ButtonSize = EngineeringButtonSize.Large;
            btnCalculate.IconKind = CoreUI.Icons.HvacIconKind.AirVelocity;
            btnCalculate.IconPlacement = EngineeringButtonIconPlacement.Left;
            btnCalculate.Location = new Point(24, 508);
            btnCalculate.Name = "btnCalculate";
            btnCalculate.Size = new Size(300, 40);
            btnCalculate.TabIndex = 2;
            btnCalculate.Text = "Sebesség számítása";
            btnCalculate.Variant = EngineeringButtonVariant.Primary;
            btnCalculate.Click += btnCalculate_Click_1;
            // 
            // resultCard
            // 
            resultCard.Location = new Point(350, 20);
            resultCard.Name = "resultCard";
            resultCard.Size = new Size(310, 164);
            resultCard.TabIndex = 3;
            // 
            // AirVelocityControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(resultCard);
            Controls.Add(btnCalculate);
            Controls.Add(geometryCard);
            Controls.Add(inputCard);
            Name = "AirVelocityControl";
            Size = new Size(690, 580);
            inputCard.ResumeLayout(false);
            geometryCard.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private EngineeringCardPanel inputCard;
        private EngineeringCardPanel geometryCard;
        private EngineeringComboBox cmbWidth;
        private EngineeringComboBox cmbDiameter;
        private EngineeringRadioButton rbRectangular;
        private EngineeringRadioButton rbCircular;
        private EngineeringButton btnCalculate;
        private EngineeringTextBox txtFlow;
        private EngineeringComboBox cmbHeight;
        private EngineeringResultCard resultCard;
    }
}
