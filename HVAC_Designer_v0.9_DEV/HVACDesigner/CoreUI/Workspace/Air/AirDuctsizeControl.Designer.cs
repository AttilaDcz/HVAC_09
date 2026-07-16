namespace HVACDesigner.CoreUI.Workspace.Air
{
    partial class AirDuctsizeControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            gbResult = new GroupBox();
            txtVelocityReal = new TextBox();
            label5 = new Label();
            lblDiameter = new Label();
            txtDiameter = new TextBox();
            txtHeightReal = new TextBox();
            lblWidth = new Label();
            lblHeight = new Label();
            txtWidthReal = new TextBox();
            btnCalculate = new Button();
            gbInput = new GroupBox();
            gbRectangularInput = new GroupBox();
            txtHeight = new TextBox();
            txtWidth = new TextBox();
            rbHeight = new RadioButton();
            rbWidth = new RadioButton();
            label7 = new Label();
            rbRoundUp = new RadioButton();
            rbRoundDown = new RadioButton();
            rbRoundNone = new RadioButton();
            label6 = new Label();
            label1 = new Label();
            lblVelocityTitle = new Label();
            txtFlow = new TextBox();
            tbVelocity = new HVACDesigner.CoreUI.Components.Engineering.EngineeringSlider();
            label4 = new Label();
            rbRectangular = new RadioButton();
            rbCircular = new RadioButton();
            checkBox1 = new CheckBox();
            checkedListBox1 = new CheckedListBox();
            comboBox1 = new ComboBox();
            gbResult.SuspendLayout();
            gbInput.SuspendLayout();
            gbRectangularInput.SuspendLayout();
            SuspendLayout();
            // 
            // gbResult
            // 
            gbResult.Controls.Add(txtVelocityReal);
            gbResult.Controls.Add(label5);
            gbResult.Controls.Add(lblDiameter);
            gbResult.Controls.Add(txtDiameter);
            gbResult.Controls.Add(txtHeightReal);
            gbResult.Controls.Add(lblWidth);
            gbResult.Controls.Add(lblHeight);
            gbResult.Controls.Add(txtWidthReal);
            gbResult.Location = new Point(58, 425);
            gbResult.Name = "gbResult";
            gbResult.Size = new Size(242, 138);
            gbResult.TabIndex = 5;
            gbResult.TabStop = false;
            gbResult.Text = "Eredmények";
            // 
            // txtVelocityReal
            // 
            txtVelocityReal.Location = new Point(23, 115);
            txtVelocityReal.Name = "txtVelocityReal";
            txtVelocityReal.ReadOnly = true;
            txtVelocityReal.Size = new Size(84, 23);
            txtVelocityReal.TabIndex = 11;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(9, 91);
            label5.Name = "label5";
            label5.Size = new Size(132, 15);
            label5.TabIndex = 10;
            label5.Text = "Valós légsebesség [m/s]";
            // 
            // lblDiameter
            // 
            lblDiameter.AutoSize = true;
            lblDiameter.Location = new Point(6, 28);
            lblDiameter.Name = "lblDiameter";
            lblDiameter.Size = new Size(80, 15);
            lblDiameter.TabIndex = 2;
            lblDiameter.Text = "Átmérő [mm]";
            // 
            // txtDiameter
            // 
            txtDiameter.Location = new Point(24, 55);
            txtDiameter.Name = "txtDiameter";
            txtDiameter.ReadOnly = true;
            txtDiameter.Size = new Size(84, 23);
            txtDiameter.TabIndex = 3;
            // 
            // txtHeightReal
            // 
            txtHeightReal.Location = new Point(138, 55);
            txtHeightReal.Name = "txtHeightReal";
            txtHeightReal.ReadOnly = true;
            txtHeightReal.Size = new Size(84, 23);
            txtHeightReal.TabIndex = 7;
            // 
            // lblWidth
            // 
            lblWidth.AutoSize = true;
            lblWidth.Location = new Point(9, 28);
            lblWidth.Name = "lblWidth";
            lblWidth.Size = new Size(89, 15);
            lblWidth.TabIndex = 4;
            lblWidth.Text = "Szélesség [mm]";
            // 
            // lblHeight
            // 
            lblHeight.AutoSize = true;
            lblHeight.Location = new Point(123, 28);
            lblHeight.Name = "lblHeight";
            lblHeight.Size = new Size(93, 15);
            lblHeight.TabIndex = 6;
            lblHeight.Text = "Magasság [mm]";
            // 
            // txtWidthReal
            // 
            txtWidthReal.Location = new Point(24, 55);
            txtWidthReal.Name = "txtWidthReal";
            txtWidthReal.ReadOnly = true;
            txtWidthReal.Size = new Size(84, 23);
            txtWidthReal.TabIndex = 5;
            // 
            // btnCalculate
            // 
            btnCalculate.Location = new Point(318, 181);
            btnCalculate.Name = "btnCalculate";
            btnCalculate.Size = new Size(75, 23);
            btnCalculate.TabIndex = 4;
            btnCalculate.Text = "Számolj";
            btnCalculate.UseVisualStyleBackColor = true;
            btnCalculate.Click += btnCalculate_Click;
            // 
            // gbInput
            // 
            gbInput.Controls.Add(gbRectangularInput);
            gbInput.Controls.Add(rbRoundUp);
            gbInput.Controls.Add(rbRoundDown);
            gbInput.Controls.Add(btnCalculate);
            gbInput.Controls.Add(rbRoundNone);
            gbInput.Controls.Add(label6);
            gbInput.Controls.Add(label1);
            gbInput.Controls.Add(lblVelocityTitle);
            gbInput.Controls.Add(txtFlow);
            gbInput.Controls.Add(tbVelocity);
            gbInput.Controls.Add(label4);
            gbInput.Location = new Point(58, 86);
            gbInput.Name = "gbInput";
            gbInput.Size = new Size(595, 333);
            gbInput.TabIndex = 3;
            gbInput.TabStop = false;
            gbInput.Text = "Bemeneti adatok";
            gbInput.Enter += gbInput_Enter;
            // 
            // gbRectangularInput
            // 
            gbRectangularInput.Controls.Add(txtHeight);
            gbRectangularInput.Controls.Add(txtWidth);
            gbRectangularInput.Controls.Add(rbHeight);
            gbRectangularInput.Controls.Add(rbWidth);
            gbRectangularInput.Controls.Add(label7);
            gbRectangularInput.Location = new Point(367, 19);
            gbRectangularInput.Margin = new Padding(0);
            gbRectangularInput.Name = "gbRectangularInput";
            gbRectangularInput.Size = new Size(216, 130);
            gbRectangularInput.TabIndex = 12;
            gbRectangularInput.TabStop = false;
            // 
            // txtHeight
            // 
            txtHeight.Location = new Point(45, 88);
            txtHeight.Name = "txtHeight";
            txtHeight.Size = new Size(84, 23);
            txtHeight.TabIndex = 11;
            // 
            // txtWidth
            // 
            txtWidth.Location = new Point(45, 88);
            txtWidth.Name = "txtWidth";
            txtWidth.Size = new Size(84, 23);
            txtWidth.TabIndex = 9;
            // 
            // rbHeight
            // 
            rbHeight.AutoSize = true;
            rbHeight.Location = new Point(22, 63);
            rbHeight.Name = "rbHeight";
            rbHeight.Size = new Size(111, 19);
            rbHeight.TabIndex = 2;
            rbHeight.TabStop = true;
            rbHeight.Text = "Magasság [mm]";
            rbHeight.UseVisualStyleBackColor = true;
            rbHeight.CheckedChanged += rbHeight_CheckedChanged;
            // 
            // rbWidth
            // 
            rbWidth.AutoSize = true;
            rbWidth.Location = new Point(22, 39);
            rbWidth.Name = "rbWidth";
            rbWidth.Size = new Size(107, 19);
            rbWidth.TabIndex = 1;
            rbWidth.TabStop = true;
            rbWidth.Text = "Szélesség [mm]";
            rbWidth.UseVisualStyleBackColor = true;
            rbWidth.CheckedChanged += rbWidht_CheckedChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(6, 21);
            label7.Name = "label7";
            label7.Size = new Size(206, 15);
            label7.TabIndex = 0;
            label7.Text = "Adja meg a légcsatorna egyik méretét";
            // 
            // rbRoundUp
            // 
            rbRoundUp.AutoSize = true;
            rbRoundUp.Location = new Point(23, 297);
            rbRoundUp.Name = "rbRoundUp";
            rbRoundUp.Size = new Size(203, 19);
            rbRoundUp.TabIndex = 14;
            rbRoundUp.TabStop = true;
            rbRoundUp.Text = "Kerekítés szabványos méretre (fel)";
            rbRoundUp.UseVisualStyleBackColor = true;
            // 
            // rbRoundDown
            // 
            rbRoundDown.AutoSize = true;
            rbRoundDown.Location = new Point(23, 272);
            rbRoundDown.Name = "rbRoundDown";
            rbRoundDown.Size = new Size(199, 19);
            rbRoundDown.TabIndex = 13;
            rbRoundDown.TabStop = true;
            rbRoundDown.Text = "Kerekítés szabványos méretre (le)";
            rbRoundDown.UseVisualStyleBackColor = true;
            // 
            // rbRoundNone
            // 
            rbRoundNone.AutoSize = true;
            rbRoundNone.Location = new Point(23, 247);
            rbRoundNone.Name = "rbRoundNone";
            rbRoundNone.Size = new Size(104, 19);
            rbRoundNone.TabIndex = 12;
            rbRoundNone.TabStop = true;
            rbRoundNone.Text = "Nincs kerekítés";
            rbRoundNone.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(6, 224);
            label6.Name = "label6";
            label6.Size = new Size(54, 15);
            label6.TabIndex = 11;
            label6.Text = "Kerekítés";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 82);
            label1.Name = "label1";
            label1.Size = new Size(152, 15);
            label1.TabIndex = 10;
            label1.Text = "Tervezett légsebesség [m/s]";
            // 
            // lblVelocityTitle
            // 
            lblVelocityTitle.AutoSize = true;
            lblVelocityTitle.Location = new Point(213, 100);
            lblVelocityTitle.Name = "lblVelocityTitle";
            lblVelocityTitle.Size = new Size(38, 15);
            lblVelocityTitle.TabIndex = 7;
            lblVelocityTitle.Text = "label1";
            // 
            // txtFlow
            // 
            txtFlow.Location = new Point(17, 54);
            txtFlow.Name = "txtFlow";
            txtFlow.Size = new Size(84, 23);
            txtFlow.TabIndex = 9;
            // 
            // tbVelocity
            // 
            tbVelocity.BackColor = Color.White;
            tbVelocity.Location = new Point(17, 117);
            tbVelocity.Decimals = 1;
            tbVelocity.LabelText = "Tervezési légsebesség";
            tbVelocity.MajorTickCount = 4;
            tbVelocity.Maximum = 5D;
            tbVelocity.Minimum = 0.5D;
            tbVelocity.Name = "tbVelocity";
            tbVelocity.ShowLabel = false;
            tbVelocity.ShowValue = false;
            tbVelocity.Size = new Size(170, 52);
            tbVelocity.SliderMode = HVACDesigner.CoreUI.Components.Engineering.EngineeringSliderMode.Stepped;
            tbVelocity.Step = 0.1D;
            tbVelocity.TabIndex = 6;
            tbVelocity.UnitLabel = "m/s";
            tbVelocity.Value = 2D;
            tbVelocity.ValueChanged += tbVelocity_Scroll;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 19);
            label4.Name = "label4";
            label4.Size = new Size(118, 15);
            label4.TabIndex = 8;
            label4.Text = "Térfogatáram [m3/h]";
            // 
            // rbRectangular
            // 
            rbRectangular.AutoSize = true;
            rbRectangular.Location = new Point(58, 50);
            rbRectangular.Name = "rbRectangular";
            rbRectangular.Size = new Size(138, 19);
            rbRectangular.TabIndex = 1;
            rbRectangular.Text = "Szögletes légcsatorna";
            rbRectangular.UseVisualStyleBackColor = true;
            rbRectangular.CheckedChanged += rbRectangular_CheckedChanged;
            // 
            // rbCircular
            // 
            rbCircular.AutoSize = true;
            rbCircular.Location = new Point(58, 25);
            rbCircular.Name = "rbCircular";
            rbCircular.Size = new Size(107, 19);
            rbCircular.TabIndex = 0;
            rbCircular.Text = "Kör légcsatorna";
            rbCircular.UseVisualStyleBackColor = true;
            rbCircular.CheckedChanged += rbCircular_CheckedChanged;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(347, 436);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(82, 19);
            checkBox1.TabIndex = 7;
            checkBox1.Text = "checkBox1";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkedListBox1
            // 
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Items.AddRange(new object[] { "Elso", "Második", "Harmadik", "Negyedik" });
            checkedListBox1.Location = new Point(478, 425);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new Size(120, 94);
            checkedListBox1.TabIndex = 8;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Egy", "Kettő", "Három", "Négy" });
            comboBox1.Location = new Point(614, 453);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 23);
            comboBox1.TabIndex = 9;
            // 
            // AirDuctsizeControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(comboBox1);
            Controls.Add(checkedListBox1);
            Controls.Add(checkBox1);
            Controls.Add(gbResult);
            Controls.Add(gbInput);
            Controls.Add(rbRectangular);
            Controls.Add(rbCircular);
            Name = "AirDuctsizeControl";
            Size = new Size(940, 600);
            gbResult.ResumeLayout(false);
            gbResult.PerformLayout();
            gbInput.ResumeLayout(false);
            gbInput.PerformLayout();
            gbRectangularInput.ResumeLayout(false);
            gbRectangularInput.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox gbResult;
        private TextBox txtVelocityReal;
        private Label label5;
        private Button btnCalculate;
        private GroupBox gbInput;
        private TextBox txtFlow;
        private Label label4;
        private TextBox txtHeightReal;
        private Label lblHeight;
        private TextBox txtWidthReal;
        private Label lblWidth;
        private TextBox txtDiameter;
        private Label lblDiameter;
        private RadioButton rbRectangular;
        private RadioButton rbCircular;
        private Label lblVelocityTitle;
        private HVACDesigner.CoreUI.Components.Engineering.EngineeringSlider tbVelocity;
        private Label label1;
        private TextBox txtHeight;
        private TextBox txtWidth;
        private RadioButton rbRoundUp;
        private RadioButton rbRoundDown;
        private RadioButton rbRoundNone;
        private Label label6;
        private GroupBox gbRectangularInput;
        private RadioButton rbHeight;
        private RadioButton rbWidth;
        private Label label7;
        private CheckBox checkBox1;
        private CheckedListBox checkedListBox1;
        private ComboBox comboBox1;

    }
}
