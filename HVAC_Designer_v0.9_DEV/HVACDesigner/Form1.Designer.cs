namespace HVACDesigner
{
    partial class Form1
    {
        // A Visual Studio által megkövetelt kötelező tervezőkomponens
        private System.ComponentModel.IContainer components = null;

        // Erőforrások tiszta felszabadítása
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        // A tervező alapértelmezett inicializáló metódusa
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form1 alapbeállításai a statikus LayoutMetrics szerint
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 780);
            this.MinimumSize = new System.Drawing.Size(1024, 600);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HVAC Designer";

            this.ResumeLayout(false);
        }

        #endregion
    }
}